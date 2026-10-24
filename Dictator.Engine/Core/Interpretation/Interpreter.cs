using System;
using System.Collections.Generic;
using System.Linq;
using Dictator.Engine.Core.Ast;
using Dictator.Engine.Core.Definitions;
using Dictator.Engine.Core.Errors;
using Dictator.Engine.Core.Registry;
using Dictator.Engine.Core.Types;
using Dictator.Engine.Core.Values;

namespace Dictator.Engine.Core.Interpretation
{
    public enum EvaluationStatus
    {
        Verdict,
        NotApplicable,
        Aborted,
        CallError,
        EvaluationError
    }

    public enum AbortReason
    {
        None,
        GasExhausted,
        CallDepthExceeded
    }

    public sealed class CallError : LawError
    {
        public const string UnknownNorm = "call.unknown_norm";
        public const string NotANorm = "call.not_a_norm";
        public const string MissingArgument = "call.missing_argument";
        public const string UnexpectedArgument = "call.unexpected_argument";
        public const string ArgumentType = "call.argument_type";

        public CallError(string code, string message, string normId, string? parameterName = null)
            : base(code, message)
        {
            NormId = normId;
            ParameterName = parameterName;
        }

        public string NormId { get; }

        public string? ParameterName { get; }
    }

    public sealed class EvaluationError : LawError
    {
        public const string MissingDefinition = "evaluation.missing_definition";
        public const string InvalidValue = "evaluation.invalid_value";
        public const string InvalidResult = "evaluation.invalid_result";

        public EvaluationError(string code, string message, string normId, string path)
            : base(code, message)
        {
            NormId = normId;
            Path = path;
        }

        public string NormId { get; }

        public string Path { get; }

        public override string ToString() => Code + " [" + NormId + " " + Path + "]: " + Message;
    }

    public sealed class EvaluationOutcome
    {
        private EvaluationOutcome(
            EvaluationStatus status,
            string normId,
            VerdictValue? verdict,
            AbortReason abortReason,
            IReadOnlyList<LawError> errors,
            long gasUsed,
            long registryVersion)
        {
            Status = status;
            NormId = normId;
            Verdict = verdict;
            AbortReason = abortReason;
            Errors = errors;
            GasUsed = gasUsed;
            RegistryVersion = registryVersion;
        }

        public EvaluationStatus Status { get; }

        public string NormId { get; }

        public VerdictValue? Verdict { get; }

        public AbortReason AbortReason { get; }

        public IReadOnlyList<LawError> Errors { get; }

        public long GasUsed { get; }

        public long RegistryVersion { get; }

        public static EvaluationOutcome OfVerdict(string normId, VerdictValue verdict, long gasUsed, long version) =>
            new EvaluationOutcome(EvaluationStatus.Verdict, normId, verdict, AbortReason.None, Array.Empty<LawError>(), gasUsed, version);

        public static EvaluationOutcome OfNotApplicable(string normId, long gasUsed, long version) =>
            new EvaluationOutcome(EvaluationStatus.NotApplicable, normId, null, AbortReason.None, Array.Empty<LawError>(), gasUsed, version);

        public static EvaluationOutcome OfAbort(string normId, AbortReason reason, long gasUsed, long version) =>
            new EvaluationOutcome(EvaluationStatus.Aborted, normId, null, reason, Array.Empty<LawError>(), gasUsed, version);

        public static EvaluationOutcome OfCallErrors(string normId, IEnumerable<CallError> errors, long version) =>
            new EvaluationOutcome(EvaluationStatus.CallError, normId, null, AbortReason.None, errors.ToArray(), 0, version);

        public static EvaluationOutcome OfEvaluationError(string normId, EvaluationError error, long gasUsed, long version) =>
            new EvaluationOutcome(EvaluationStatus.EvaluationError, normId, null, AbortReason.None, new LawError[] { error }, gasUsed, version);

        public override string ToString() =>
            Status + (Verdict != null ? " " + Verdict : string.Empty) + (AbortReason != AbortReason.None ? " " + AbortReason : string.Empty)
            + string.Concat(Errors.Select(e => " " + e)) + " gas=" + GasUsed + " v" + RegistryVersion;
    }

    public class GasPrices
    {
        public static readonly GasPrices Default = new GasPrices();

        public virtual long NodeCost(LawNode node) => 1;

        public virtual long ElementCost => 1;

        public virtual long CallCost => 1;
    }

    public sealed class Interpreter
    {
        public const int DefaultMaxCallDepth = 128;

        public Interpreter(
            DefinitionRegistry registry,
            OperationTable operations,
            OperationTable functions,
            GasPrices? prices = null,
            int maxCallDepth = DefaultMaxCallDepth)
        {
            Registry = registry;
            Operations = operations;
            Functions = functions;
            Prices = prices ?? GasPrices.Default;
            MaxCallDepth = maxCallDepth;
        }

        internal DefinitionRegistry Registry { get; }

        internal OperationTable Operations { get; }

        internal OperationTable Functions { get; }

        internal GasPrices Prices { get; }

        internal int MaxCallDepth { get; }

        public EvaluationOutcome Evaluate(string normId, IReadOnlyDictionary<string, LawValue> arguments, long gasLimit)
        {
            var version = Registry.Version;
            if (!Registry.TryGetDefinition(normId, out var definition))
            {
                return EvaluationOutcome.OfCallErrors(normId, new[] { new CallError(CallError.UnknownNorm, $"Норма «{normId}» не найдена", normId) }, version);
            }

            if (!(definition is NormDefinition norm))
            {
                return EvaluationOutcome.OfCallErrors(normId, new[] { new CallError(CallError.NotANorm, $"«{definition.Name}» не является нормой", normId) }, version);
            }

            var errors = new List<CallError>();
            foreach (var parameter in norm.Parameters)
            {
                if (!arguments.TryGetValue(parameter.Name, out var value))
                {
                    errors.Add(new CallError(CallError.MissingArgument, $"Для нормы «{norm.Name}» не передан аргумент «{parameter.Name}»", normId, parameter.Name));
                }
                else if (Mismatch(value, parameter.Type) is string mismatch)
                {
                    errors.Add(new CallError(
                        CallError.ArgumentType,
                        $"Аргумент «{parameter.Name}» нормы «{norm.Name}» должен иметь тип {parameter.Type}, но {mismatch}",
                        normId,
                        parameter.Name));
                }
            }

            foreach (var name in arguments.Keys)
            {
                if (norm.Parameters.All(p => !string.Equals(p.Name, name, StringComparison.Ordinal)))
                {
                    errors.Add(new CallError(CallError.UnexpectedArgument, $"У нормы «{norm.Name}» нет параметра «{name}»", normId, name));
                }
            }

            if (errors.Count > 0)
            {
                return EvaluationOutcome.OfCallErrors(normId, errors, version);
            }

            return new Execution(this, gasLimit).Run(norm, arguments, version);
        }

        private static string? Mismatch(LawValue value, LawType type)
        {
            if (!value.Type.IsAssignableTo(type))
            {
                return $"получено {value.Type}";
            }

            if (value is ListValue list)
            {
                for (var i = 0; i < list.Items.Count; i++)
                {
                    if (Mismatch(list.Items[i], list.ElementType) is string inner)
                    {
                        return $"в элементе {i} {inner}";
                    }
                }
            }

            return null;
        }
    }

    internal enum CompletionKind
    {
        Normal,
        Return,
        Break,
        Continue,
        Abort,
        Fault
    }

    internal readonly struct Completion
    {
        private Completion(CompletionKind kind, LawValue value, AbortReason abortReason, EvaluationError? error)
        {
            Kind = kind;
            Value = value;
            AbortReason = abortReason;
            Error = error;
        }

        public CompletionKind Kind { get; }

        public LawValue Value { get; }

        public AbortReason AbortReason { get; }

        public EvaluationError? Error { get; }

        public bool IsAbrupt => Kind != CompletionKind.Normal;

        public static Completion Normal(LawValue value) => new Completion(CompletionKind.Normal, value, AbortReason.None, null);

        public static Completion Return(LawValue value) => new Completion(CompletionKind.Return, value, AbortReason.None, null);

        public static Completion Break() => new Completion(CompletionKind.Break, UnitValue.Instance, AbortReason.None, null);

        public static Completion Continue() => new Completion(CompletionKind.Continue, UnitValue.Instance, AbortReason.None, null);

        public static Completion Abort(AbortReason reason) => new Completion(CompletionKind.Abort, UnitValue.Instance, reason, null);

        public static Completion Fault(EvaluationError error) => new Completion(CompletionKind.Fault, UnitValue.Instance, AbortReason.None, error);
    }

    internal sealed class Frame
    {
        public Frame(NormDefinition norm, IReadOnlyDictionary<string, LawValue> parameters)
        {
            Norm = norm;
            Parameters = parameters;
        }

        public NormDefinition Norm { get; }

        public IReadOnlyDictionary<string, LawValue> Parameters { get; }

        public List<Dictionary<string, LawValue>> Scopes { get; } = new List<Dictionary<string, LawValue>>();

        public List<(string Role, int Index)> Path { get; } = new List<(string Role, int Index)>();

        public string PathText => "/" + string.Join("/", Path.Select(s => s.Index < 0 ? s.Role : s.Role + "[" + s.Index + "]"));
    }

    internal sealed class Execution : ILawNodeVisitor<Completion>
    {
        private readonly Interpreter _interpreter;
        private readonly long _gasLimit;
        private readonly Stack<Frame> _frames = new Stack<Frame>();
        private long _gasUsed;

        public Execution(Interpreter interpreter, long gasLimit)
        {
            _interpreter = interpreter;
            _gasLimit = gasLimit;
        }

        private Frame Current => _frames.Peek();

        public EvaluationOutcome Run(NormDefinition norm, IReadOnlyDictionary<string, LawValue> arguments, long version)
        {
            var completion = Invoke(norm, arguments);
            switch (completion.Kind)
            {
                case CompletionKind.Abort:
                    return EvaluationOutcome.OfAbort(norm.Id, completion.AbortReason, _gasUsed, version);
                case CompletionKind.Fault:
                    return EvaluationOutcome.OfEvaluationError(norm.Id, completion.Error!, _gasUsed, version);
            }

            switch (completion.Value)
            {
                case VerdictValue verdict:
                    return EvaluationOutcome.OfVerdict(norm.Id, verdict, _gasUsed, version);
                case NotApplicableValue _:
                    return EvaluationOutcome.OfNotApplicable(norm.Id, _gasUsed, version);
                default:
                    var error = new EvaluationError(EvaluationError.InvalidResult, $"Норма «{norm.Name}» вернула не исход, а {completion.Value.Type}", norm.Id, "/");
                    return EvaluationOutcome.OfEvaluationError(norm.Id, error, _gasUsed, version);
            }
        }

        public Completion VisitLiteral(LiteralNode node) => Completion.Normal(node.Value);

        public Completion VisitDefinitionReference(DefinitionReferenceNode node)
        {
            if (!_interpreter.Registry.TryGetDefinition(node.DefinitionId, out var definition))
            {
                return Fault(EvaluationError.MissingDefinition, $"Определение «{node.DefinitionId}» больше не существует");
            }

            return Completion.Normal(definition is AliasDefinition alias
                ? alias.Value
                : new DefinitionReferenceValue(definition.Kind, definition.Id));
        }

        public Completion VisitParameter(ParameterNode node) =>
            Current.Parameters.TryGetValue(node.Name, out var value)
                ? Completion.Normal(value)
                : Fault(EvaluationError.InvalidValue, $"Параметр «{node.Name}» не передан");

        public Completion VisitLocal(LocalNode node)
        {
            var scope = FindScope(node.Name);
            return scope != null
                ? Completion.Normal(scope[node.Name])
                : Fault(EvaluationError.InvalidValue, $"Переменная «{node.Name}» не объявлена");
        }

        public Completion VisitDeclare(DeclareNode node)
        {
            var value = Eval(node.Value, "value");
            if (value.IsAbrupt)
            {
                return value;
            }

            Current.Scopes[Current.Scopes.Count - 1][node.Name] = value.Value;
            return Completion.Normal(UnitValue.Instance);
        }

        public Completion VisitAssign(AssignNode node)
        {
            var value = Eval(node.Value, "value");
            if (value.IsAbrupt)
            {
                return value;
            }

            var scope = FindScope(node.Name);
            if (scope == null)
            {
                return Fault(EvaluationError.InvalidValue, $"Переменная «{node.Name}» не объявлена");
            }

            scope[node.Name] = value.Value;
            return Completion.Normal(UnitValue.Instance);
        }

        public Completion VisitOperation(OperationNode node)
        {
            var operands = EvalMany(node.Operands, "operands", out var abrupt);
            return abrupt ?? Apply(_interpreter.Operations, node.OperationId, operands);
        }

        public Completion VisitAnd(AndNode node)
        {
            var left = EvalBoolean(node.Left, "left", out var leftValue);
            if (left.IsAbrupt || !leftValue)
            {
                return left;
            }

            return EvalBoolean(node.Right, "right", out _);
        }

        public Completion VisitOr(OrNode node)
        {
            var left = EvalBoolean(node.Left, "left", out var leftValue);
            if (left.IsAbrupt || leftValue)
            {
                return left;
            }

            return EvalBoolean(node.Right, "right", out _);
        }

        public Completion VisitNot(NotNode node)
        {
            var operand = EvalBoolean(node.Operand, "operand", out var value);
            return operand.IsAbrupt ? operand : Completion.Normal(BooleanValue.Of(!value));
        }

        public Completion VisitBuiltinCall(BuiltinCallNode node)
        {
            var arguments = EvalMany(node.Arguments, "arguments", out var abrupt);
            return abrupt ?? Apply(_interpreter.Functions, node.FunctionId, arguments);
        }

        public Completion VisitBlock(BlockNode node)
        {
            PushScope();
            LawValue result = UnitValue.Instance;
            for (var i = 0; i < node.Expressions.Count; i++)
            {
                var completion = Eval(node.Expressions[i], "expressions", i);
                if (completion.IsAbrupt)
                {
                    PopScope();
                    return completion;
                }

                result = completion.Value;
            }

            PopScope();
            return Completion.Normal(result);
        }

        public Completion VisitIf(IfNode node)
        {
            var condition = EvalBoolean(node.Condition, "condition", out var value);
            if (condition.IsAbrupt)
            {
                return condition;
            }

            if (value)
            {
                return EvalScoped(node.Then, "then");
            }

            return node.Otherwise == null ? Completion.Normal(UnitValue.Instance) : EvalScoped(node.Otherwise, "otherwise");
        }

        public Completion VisitWhile(WhileNode node)
        {
            while (true)
            {
                var condition = EvalBoolean(node.Condition, "condition", out var value);
                if (condition.IsAbrupt)
                {
                    return condition;
                }

                if (!value)
                {
                    return Completion.Normal(UnitValue.Instance);
                }

                var body = EvalScoped(node.Body, "body");
                if (body.Kind == CompletionKind.Break)
                {
                    return Completion.Normal(UnitValue.Instance);
                }

                if (body.IsAbrupt && body.Kind != CompletionKind.Continue)
                {
                    return body;
                }
            }
        }

        public Completion VisitForEach(ForEachNode node)
        {
            var source = EvalList(node.Source, "source", out var list);
            if (source.IsAbrupt)
            {
                return source;
            }

            foreach (var item in list!.Items)
            {
                if (!Charge(_interpreter.Prices.ElementCost))
                {
                    return Completion.Abort(AbortReason.GasExhausted);
                }

                PushScope();
                Current.Scopes[Current.Scopes.Count - 1][node.Variable] = item;
                var body = Eval(node.Body, "body");
                PopScope();

                if (body.Kind == CompletionKind.Break)
                {
                    break;
                }

                if (body.IsAbrupt && body.Kind != CompletionKind.Continue)
                {
                    return body;
                }
            }

            return Completion.Normal(UnitValue.Instance);
        }

        public Completion VisitCall(CallNode node)
        {
            var arguments = EvalMany(node.Arguments, "arguments", out var abrupt);
            if (abrupt != null)
            {
                return abrupt.Value;
            }

            if (!Charge(_interpreter.Prices.CallCost))
            {
                return Completion.Abort(AbortReason.GasExhausted);
            }

            if (!_interpreter.Registry.TryGetDefinition(node.TargetId, out var definition) || !(definition is NormDefinition norm))
            {
                return Fault(EvaluationError.MissingDefinition, $"Норма «{node.TargetId}» больше не существует");
            }

            if (norm.Parameters.Count != arguments.Length)
            {
                return Fault(EvaluationError.InvalidValue, $"Норма «{norm.Name}» ожидает аргументов: {norm.Parameters.Count}, передано: {arguments.Length}");
            }

            var parameters = new Dictionary<string, LawValue>(StringComparer.Ordinal);
            for (var i = 0; i < arguments.Length; i++)
            {
                parameters[norm.Parameters[i].Name] = arguments[i];
            }

            return Invoke(norm, parameters);
        }

        public Completion VisitReturn(ReturnNode node)
        {
            var value = Eval(node.Value, "value");
            return value.IsAbrupt ? value : Completion.Return(value.Value);
        }

        public Completion VisitBreak(BreakNode node) => Completion.Break();

        public Completion VisitContinue(ContinueNode node) => Completion.Continue();

        public Completion VisitField(FieldNode node)
        {
            var record = Eval(node.Record, "record");
            if (record.IsAbrupt)
            {
                return record;
            }

            if (record.Value is RecordValue value && value.TryGetField(node.FieldName, out var field))
            {
                return Completion.Normal(field);
            }

            return Fault(EvaluationError.InvalidValue, $"У значения {record.Value} нет поля «{node.FieldName}»");
        }

        public Completion VisitHasStatus(HasStatusNode node)
        {
            var subject = EvalSnapshot(node.Subject, out var snapshot);
            if (subject.IsAbrupt)
            {
                return subject;
            }

            foreach (var assignment in snapshot!.Assignments)
            {
                if (!Charge(_interpreter.Prices.ElementCost))
                {
                    return Completion.Abort(AbortReason.GasExhausted);
                }

                if (IsKindOf(assignment.StatusId, node.StatusId))
                {
                    return Completion.Normal(BooleanValue.True);
                }
            }

            return Completion.Normal(BooleanValue.False);
        }

        public Completion VisitStatusRecords(StatusRecordsNode node)
        {
            var subject = EvalSnapshot(node.Subject, out var snapshot);
            if (subject.IsAbrupt)
            {
                return subject;
            }

            var records = new List<LawValue>();
            foreach (var assignment in snapshot!.Assignments)
            {
                if (!Charge(_interpreter.Prices.ElementCost))
                {
                    return Completion.Abort(AbortReason.GasExhausted);
                }

                if (IsKindOf(assignment.StatusId, node.StatusId))
                {
                    records.Add(assignment.ToRecord());
                }
            }

            return Completion.Normal(new ListValue(StatusAssignment.RecordType, records));
        }

        public Completion VisitState(StateNode node)
        {
            var subject = EvalSnapshot(node.Subject, out var snapshot);
            return subject.IsAbrupt ? subject : Completion.Normal(BooleanValue.Of(snapshot!.HasState(node.StateId)));
        }

        public Completion VisitCount(CountNode node) => Query(node, QueryKind.Count);

        public Completion VisitExists(ExistsNode node) => Query(node, QueryKind.Exists);

        public Completion VisitAll(AllNode node) => Query(node, QueryKind.All);

        public Completion VisitFilter(FilterNode node) => Query(node, QueryKind.Filter);

        public Completion VisitSanction(SanctionNode node)
        {
            var arguments = EvalMany(node.Arguments, "arguments", out var abrupt);
            return abrupt ?? Completion.Normal(new SanctionValue(node.SanctionId, arguments));
        }

        public Completion VisitVerdict(VerdictNode node)
        {
            var sanctions = new List<SanctionValue>();
            for (var i = 0; i < node.Sanctions.Count; i++)
            {
                var sanction = Eval(node.Sanctions[i], "sanctions", i);
                if (sanction.IsAbrupt)
                {
                    return sanction;
                }

                if (!(sanction.Value is SanctionValue value))
                {
                    return Fault(EvaluationError.InvalidValue, $"Ожидалась санкция, получено {sanction.Value}");
                }

                sanctions.Add(value);
            }

            LawValue? reason = null;
            if (node.Reason != null)
            {
                var completion = Eval(node.Reason, "reason");
                if (completion.IsAbrupt)
                {
                    return completion;
                }

                reason = completion.Value;
            }

            return Completion.Normal(new VerdictValue(node.Modality, sanctions, Current.Norm.Id, reason));
        }

        public Completion VisitNotApplicable(NotApplicableNode node) => Completion.Normal(NotApplicableValue.Instance);

        private enum QueryKind
        {
            Count,
            Exists,
            All,
            Filter
        }

        private Completion Query(ListQueryNode node, QueryKind kind)
        {
            var source = EvalList(node.Source, "source", out var list);
            if (source.IsAbrupt)
            {
                return source;
            }

            var count = 0L;
            var kept = new List<LawValue>();
            foreach (var item in list!.Items)
            {
                if (!Charge(_interpreter.Prices.ElementCost))
                {
                    return Completion.Abort(AbortReason.GasExhausted);
                }

                var matched = true;
                if (node.Predicate != null)
                {
                    PushScope();
                    Current.Scopes[Current.Scopes.Count - 1][node.Variable] = item;
                    var predicate = EvalBoolean(node.Predicate, "predicate", out matched);
                    PopScope();
                    if (predicate.IsAbrupt)
                    {
                        return predicate;
                    }
                }

                switch (kind)
                {
                    case QueryKind.Count when matched:
                        count++;
                        break;
                    case QueryKind.Exists when matched:
                        return Completion.Normal(BooleanValue.True);
                    case QueryKind.All when !matched:
                        return Completion.Normal(BooleanValue.False);
                    case QueryKind.Filter when matched:
                        kept.Add(item);
                        break;
                }
            }

            switch (kind)
            {
                case QueryKind.Count:
                    return Completion.Normal(new IntegerValue(count));
                case QueryKind.Exists:
                    return Completion.Normal(BooleanValue.False);
                case QueryKind.All:
                    return Completion.Normal(BooleanValue.True);
                default:
                    return Completion.Normal(new ListValue(list.ElementType, kept));
            }
        }

        private Completion Invoke(NormDefinition norm, IReadOnlyDictionary<string, LawValue> parameters)
        {
            if (_frames.Count >= _interpreter.MaxCallDepth)
            {
                return Completion.Abort(AbortReason.CallDepthExceeded);
            }

            _frames.Push(new Frame(norm, parameters));
            PushScope();
            var completion = EvalHere(norm.Body);
            _frames.Pop();

            switch (completion.Kind)
            {
                case CompletionKind.Return:
                    return Completion.Normal(completion.Value);
                case CompletionKind.Break:
                case CompletionKind.Continue:
                    return Fault(EvaluationError.InvalidResult, "«Прервать» или «Продолжить» вне цикла", norm.Id, "/");
                default:
                    return completion;
            }
        }

        private Completion Eval(LawNode node, string role, int index = -1)
        {
            var path = Current.Path;
            path.Add((role, index));
            var completion = EvalHere(node);
            path.RemoveAt(path.Count - 1);
            return completion;
        }

        private Completion EvalHere(LawNode node) =>
            Charge(_interpreter.Prices.NodeCost(node)) ? node.Accept(this) : Completion.Abort(AbortReason.GasExhausted);

        private Completion EvalScoped(LawNode node, string role)
        {
            PushScope();
            var completion = Eval(node, role);
            PopScope();
            return completion;
        }

        private LawValue[] EvalMany(IReadOnlyList<LawNode> nodes, string role, out Completion? abrupt)
        {
            var values = new LawValue[nodes.Count];
            for (var i = 0; i < nodes.Count; i++)
            {
                var completion = Eval(nodes[i], role, i);
                if (completion.IsAbrupt)
                {
                    abrupt = completion;
                    return values;
                }

                values[i] = completion.Value;
            }

            abrupt = null;
            return values;
        }

        private Completion EvalBoolean(LawNode node, string role, out bool value)
        {
            var completion = Eval(node, role);
            value = false;
            if (completion.IsAbrupt)
            {
                return completion;
            }

            if (completion.Value is BooleanValue boolean)
            {
                value = boolean.Value;
                return completion;
            }

            return Fault(EvaluationError.InvalidValue, $"Ожидалось логическое значение, получено {completion.Value}");
        }

        private Completion EvalList(LawNode node, string role, out ListValue? list)
        {
            var completion = Eval(node, role);
            list = completion.Value as ListValue;
            if (completion.IsAbrupt || list != null)
            {
                return completion;
            }

            return Fault(EvaluationError.InvalidValue, $"Ожидался список, получено {completion.Value}");
        }

        private Completion EvalSnapshot(LawNode node, out SnapshotValue? snapshot)
        {
            var completion = Eval(node, "subject");
            snapshot = completion.Value as SnapshotValue;
            if (completion.IsAbrupt || snapshot != null)
            {
                return completion;
            }

            return Fault(EvaluationError.InvalidValue, $"Ожидался снимок субъекта, получено {completion.Value}");
        }

        private Completion Apply(OperationTable table, string id, LawValue[] operands)
        {
            var resolution = table.Resolve(id, operands.Select(o => o.Type).ToArray());
            if (resolution.Status != OperationResolutionStatus.Resolved || resolution.Overload == null)
            {
                return Fault(EvaluationError.InvalidValue, $"«{id}» не применима к значениям ({string.Join(", ", operands.Select(o => o.ToString()))})");
            }

            var result = resolution.Overload.Evaluate(operands);
            return result.IsSuccess ? Completion.Normal(result.Value!) : Fault(result.Failure!.Code, result.Failure.Message);
        }

        private bool IsKindOf(string statusId, string targetId)
        {
            var visited = new HashSet<string>(StringComparer.Ordinal);
            for (string? current = statusId; current != null && visited.Add(current);)
            {
                if (string.Equals(current, targetId, StringComparison.Ordinal))
                {
                    return true;
                }

                current = _interpreter.Registry.TryGetDefinition(current, out var definition) && definition is StatusDefinition status
                    ? status.ParentId
                    : null;
            }

            return false;
        }

        private bool Charge(long cost)
        {
            if (cost > _gasLimit - _gasUsed)
            {
                _gasUsed = _gasLimit;
                return false;
            }

            _gasUsed += cost;
            return true;
        }

        private Dictionary<string, LawValue>? FindScope(string name)
        {
            var scopes = Current.Scopes;
            for (var i = scopes.Count - 1; i >= 0; i--)
            {
                if (scopes[i].ContainsKey(name))
                {
                    return scopes[i];
                }
            }

            return null;
        }

        private void PushScope() => Current.Scopes.Add(new Dictionary<string, LawValue>(StringComparer.Ordinal));

        private void PopScope() => Current.Scopes.RemoveAt(Current.Scopes.Count - 1);

        private Completion Fault(string code, string message) => Fault(code, message, Current.Norm.Id, Current.PathText);

        private static Completion Fault(string code, string message, string normId, string path) =>
            Completion.Fault(new EvaluationError(code, message, normId, path));
    }
}
