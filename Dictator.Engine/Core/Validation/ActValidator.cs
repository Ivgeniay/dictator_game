using System;
using System.Collections.Generic;
using System.Linq;
using Dictator.Engine.Core.Ast;
using Dictator.Engine.Core.Definitions;
using Dictator.Engine.Core.Errors;
using Dictator.Engine.Core.Registry;
using Dictator.Engine.Core.Types;
using Dictator.Engine.Core.Values;

namespace Dictator.Engine.Core.Validation
{
    public sealed class ValidationError : LawError
    {
        public const string TypeMismatch = "validation.type_mismatch";
        public const string UnknownDefinition = "validation.unknown_definition";
        public const string InvisibleDefinition = "validation.invisible_definition";
        public const string WrongDefinitionKind = "validation.wrong_definition_kind";
        public const string UnknownParameter = "validation.unknown_parameter";
        public const string UnknownLocal = "validation.unknown_local";
        public const string DuplicateLocal = "validation.duplicate_local";
        public const string AssignToParameter = "validation.assign_to_parameter";
        public const string UnitValue = "validation.unit_value";
        public const string UnknownOperation = "validation.unknown_operation";
        public const string UnknownFunction = "validation.unknown_function";
        public const string NoMatchingOverload = "validation.no_matching_overload";
        public const string AmbiguousOverload = "validation.ambiguous_overload";
        public const string NotAList = "validation.not_a_list";
        public const string NotARecord = "validation.not_a_record";
        public const string UnknownField = "validation.unknown_field";
        public const string BreakOutsideLoop = "validation.break_outside_loop";
        public const string ContinueOutsideLoop = "validation.continue_outside_loop";
        public const string ArgumentCount = "validation.argument_count";
        public const string MissingPredicate = "validation.missing_predicate";
        public const string MissingSanctions = "validation.missing_sanctions";
        public const string InvalidReason = "validation.invalid_reason";
        public const string NotAllPathsReturn = "validation.not_all_paths_return";
        public const string WrongResultType = "validation.wrong_result_type";
        public const string DuplicateParameter = "validation.duplicate_parameter";
        public const string InvalidParameterType = "validation.invalid_parameter_type";
        public const string EmptyApplicability = "validation.empty_applicability";
        public const string ApplicabilityWiderThanParent = "validation.applicability_wider_than_parent";
        public const string StatusCycle = "validation.status_cycle";
        public const string InvalidAliasValue = "validation.invalid_alias_value";

        public ValidationError(string code, string message, string definitionId, string path)
            : base(code, message)
        {
            DefinitionId = definitionId;
            Path = path;
        }

        public string DefinitionId { get; }

        public string Path { get; }

        public override string ToString() => Code + " [" + DefinitionId + " " + Path + "]: " + Message;
    }

    public readonly struct ValidationResult
    {
        public ValidationResult(IReadOnlyList<ValidationError> errors)
        {
            Errors = errors;
        }

        public IReadOnlyList<ValidationError> Errors { get; }

        public bool IsSuccess => Errors.Count == 0;
    }

    public sealed class ActValidator
    {
        private readonly DefinitionRegistry _registry;
        private readonly OperationTable _operations;
        private readonly OperationTable _functions;

        public ActValidator(DefinitionRegistry registry, OperationTable operations, OperationTable functions)
        {
            _registry = registry;
            _operations = operations;
            _functions = functions;
        }

        public ValidationResult Validate(Act act)
        {
            var scope = new DefinitionScope(act, _registry);
            var errors = new List<ValidationError>();

            foreach (var definition in act.Definitions)
            {
                var context = new ValidationContext(scope, errors, definition.Id);
                switch (definition)
                {
                    case StatusDefinition status:
                        CheckStatus(status, context);
                        break;
                    case AliasDefinition alias:
                        CheckAlias(alias, context);
                        break;
                    case SanctionDefinition sanction:
                        CheckParameters(sanction.Parameters, context);
                        break;
                    case NormDefinition norm:
                        CheckParameters(norm.Parameters, context);
                        new NormChecker(context, norm, _operations, _functions).Check();
                        break;
                }
            }

            return new ValidationResult(errors.ToArray());
        }

        private static void CheckStatus(StatusDefinition status, ValidationContext context)
        {
            if (status.Applicability == SubjectKinds.None)
            {
                context.Report(ValidationError.EmptyApplicability, $"Статус «{status.Name}» ни к кому не применим");
            }

            if (status.ParentId == null)
            {
                return;
            }

            var parent = context.Resolve<StatusDefinition>(status.ParentId, "статусом");
            if (parent != null && (status.Applicability & ~parent.Applicability) != SubjectKinds.None)
            {
                context.Report(
                    ValidationError.ApplicabilityWiderThanParent,
                    $"Статус «{status.Name}» применим к более широкому кругу субъектов, чем «{parent.Name}»");
            }

            var visited = new HashSet<string>(StringComparer.Ordinal) { status.Id };
            for (var current = parent; current?.ParentId != null; current = context.Scope.Find(current.ParentId) as StatusDefinition)
            {
                if (!visited.Add(current.ParentId))
                {
                    if (string.Equals(current.ParentId, status.Id, StringComparison.Ordinal))
                    {
                        context.Report(ValidationError.StatusCycle, $"Цепочка родителей статуса «{status.Name}» замыкается на себя");
                    }

                    break;
                }
            }
        }

        private static void CheckAlias(AliasDefinition alias, ValidationContext context)
        {
            if (alias.Type is NeverType || alias.Type.Equals(LawTypes.Unit))
            {
                context.Report(ValidationError.InvalidAliasValue, $"Алиас «{alias.Name}» не содержит значения");
            }

            context.CheckValue(alias.Value);
        }

        private static void CheckParameters(IReadOnlyList<TypedParameter> parameters, ValidationContext context)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var parameter in parameters)
            {
                if (!names.Add(parameter.Name))
                {
                    context.Report(ValidationError.DuplicateParameter, $"Параметр «{parameter.Name}» объявлен дважды");
                }

                if (parameter.Type is NeverType || parameter.Type.Equals(LawTypes.Unit))
                {
                    context.Report(ValidationError.InvalidParameterType, $"Параметр «{parameter.Name}» не может иметь тип {parameter.Type}");
                }

                context.CheckType(parameter.Type);
            }
        }
    }

    internal sealed class DefinitionScope
    {
        private readonly Dictionary<string, Definition> _visible = new Dictionary<string, Definition>(StringComparer.Ordinal);

        public DefinitionScope(Act act, DefinitionRegistry registry)
        {
            Registry = registry;
            foreach (var definition in act.Definitions)
            {
                if (!_visible.ContainsKey(definition.Id))
                {
                    _visible.Add(definition.Id, definition);
                }
            }

            if (act.ParentId == null)
            {
                return;
            }

            foreach (var definition in registry.VisibleDefinitions(act.ParentId))
            {
                if (!_visible.ContainsKey(definition.Id))
                {
                    _visible.Add(definition.Id, definition);
                }
            }
        }

        public DefinitionRegistry Registry { get; }

        public Definition? Find(string id) => _visible.TryGetValue(id, out var definition) ? definition : null;
    }

    internal sealed class ValidationContext
    {
        private readonly List<ValidationError> _errors;
        private readonly List<string> _path = new List<string>();

        public ValidationContext(DefinitionScope scope, List<ValidationError> errors, string definitionId)
        {
            Scope = scope;
            _errors = errors;
            DefinitionId = definitionId;
        }

        public DefinitionScope Scope { get; }

        public string DefinitionId { get; }

        public string Path => "/" + string.Join("/", _path);

        public void Enter(string segment) => _path.Add(segment);

        public void Leave() => _path.RemoveAt(_path.Count - 1);

        public void Report(string code, string message) => _errors.Add(new ValidationError(code, message, DefinitionId, Path));

        public Definition? ResolveAny(string id)
        {
            var definition = Scope.Find(id);
            if (definition != null)
            {
                return definition;
            }

            if (Scope.Registry.TryGetDefinition(id, out var hidden))
            {
                Report(ValidationError.InvisibleDefinition, $"Термин «{hidden.Name}» недоступен в этом акте");
            }
            else
            {
                Report(ValidationError.UnknownDefinition, $"Определение «{id}» не найдено");
            }

            return null;
        }

        public T? Resolve<T>(string id, string expected) where T : Definition
        {
            var definition = ResolveAny(id);
            if (definition == null)
            {
                return null;
            }

            if (definition is T typed)
            {
                return typed;
            }

            Report(ValidationError.WrongDefinitionKind, $"«{definition.Name}» не является {expected}");
            return null;
        }

        public void CheckType(LawType type)
        {
            switch (type)
            {
                case MoneyType money:
                    Resolve<CurrencyDefinition>(money.CurrencyId, "валютой");
                    break;
                case ListType list:
                    CheckType(list.Element);
                    break;
                case RecordType record:
                    foreach (var field in record.Fields)
                    {
                        CheckType(field.Type);
                    }

                    break;
            }
        }

        public void CheckValue(LawValue value)
        {
            CheckType(value.Type);
            switch (value)
            {
                case DefinitionReferenceValue reference:
                    var target = ResolveAny(reference.DefinitionId);
                    if (target != null && !string.Equals(target.Kind, reference.DefinitionKind, StringComparison.Ordinal))
                    {
                        Report(ValidationError.WrongDefinitionKind, $"«{target.Name}» не является определением вида «{reference.DefinitionKind}»");
                    }

                    break;
                case ListValue list:
                    foreach (var item in list.Items)
                    {
                        CheckValue(item);
                    }

                    break;
                case RecordValue record:
                    foreach (var field in record.Fields.Values)
                    {
                        CheckValue(field);
                    }

                    break;
            }
        }
    }

    internal sealed class NormChecker : ILawNodeVisitor<LawType>
    {
        private readonly ValidationContext _context;
        private readonly NormDefinition _norm;
        private readonly OperationTable _operations;
        private readonly OperationTable _functions;
        private readonly Dictionary<string, LawType> _parameters;
        private readonly List<Dictionary<string, LawType>> _scopes = new List<Dictionary<string, LawType>>();
        private int _loopDepth;

        public NormChecker(ValidationContext context, NormDefinition norm, OperationTable operations, OperationTable functions)
        {
            _context = context;
            _norm = norm;
            _operations = operations;
            _functions = functions;
            _parameters = new Dictionary<string, LawType>(StringComparer.Ordinal);
            foreach (var parameter in norm.Parameters)
            {
                if (!_parameters.ContainsKey(parameter.Name))
                {
                    _parameters.Add(parameter.Name, parameter.Type);
                }
            }
        }

        public void Check()
        {
            PushScope();
            var type = _norm.Body.Accept(this);
            PopScope();

            if (type.IsAssignableTo(_norm.ResultType))
            {
                return;
            }

            if (type.Equals(LawTypes.Unit))
            {
                _context.Report(ValidationError.NotAllPathsReturn, $"Не все ветви нормы «{_norm.Name}» заканчиваются вердиктом или «не применима»");
            }
            else
            {
                _context.Report(ValidationError.WrongResultType, $"Норма «{_norm.Name}» должна возвращать вердикт или «не применима», а возвращает {type}");
            }
        }

        public LawType VisitLiteral(LiteralNode node)
        {
            _context.CheckValue(node.Value);
            return node.Value.Type;
        }

        public LawType VisitDefinitionReference(DefinitionReferenceNode node)
        {
            var definition = _context.ResolveAny(node.DefinitionId);
            switch (definition)
            {
                case null:
                    return LawTypes.Never;
                case AliasDefinition alias:
                    return alias.Type;
                default:
                    return LawTypes.ReferenceTo(definition.Kind);
            }
        }

        public LawType VisitParameter(ParameterNode node)
        {
            if (_parameters.TryGetValue(node.Name, out var type))
            {
                return type;
            }

            _context.Report(ValidationError.UnknownParameter, $"У нормы нет параметра «{node.Name}»");
            return LawTypes.Never;
        }

        public LawType VisitLocal(LocalNode node)
        {
            if (TryFindLocal(node.Name, out var type))
            {
                return type;
            }

            _context.Report(ValidationError.UnknownLocal, $"Переменная «{node.Name}» не объявлена");
            return LawTypes.Never;
        }

        public LawType VisitDeclare(DeclareNode node)
        {
            var type = Visit(node.Value, "value");
            if (type.Equals(LawTypes.Unit))
            {
                _context.Report(ValidationError.UnitValue, $"Выражение для «{node.Name}» не даёт значения");
                type = LawTypes.Never;
            }

            Declare(node.Name, type);
            return LawTypes.Unit;
        }

        public LawType VisitAssign(AssignNode node)
        {
            if (TryFindLocal(node.Name, out var target))
            {
                VisitExpecting(node.Value, "value", target, $"Значение для «{node.Name}»");
                return LawTypes.Unit;
            }

            Visit(node.Value, "value");
            if (_parameters.ContainsKey(node.Name))
            {
                _context.Report(ValidationError.AssignToParameter, $"Параметр «{node.Name}» нельзя изменить");
            }
            else
            {
                _context.Report(ValidationError.UnknownLocal, $"Переменная «{node.Name}» не объявлена");
            }

            return LawTypes.Unit;
        }

        public LawType VisitOperation(OperationNode node)
        {
            var operands = VisitMany(node.Operands, "operands");
            return ResolveIn(_operations, node.OperationId, operands, ValidationError.UnknownOperation, "Операция");
        }

        public LawType VisitAnd(AndNode node) => VisitLogical(node.Left, node.Right);

        public LawType VisitOr(OrNode node) => VisitLogical(node.Left, node.Right);

        public LawType VisitNot(NotNode node)
        {
            var operand = VisitExpecting(node.Operand, "operand", LawTypes.Boolean, "Операнд «Не»");
            return operand is NeverType ? operand : LawTypes.Boolean;
        }

        public LawType VisitBuiltinCall(BuiltinCallNode node)
        {
            var arguments = VisitMany(node.Arguments, "arguments");
            return ResolveIn(_functions, node.FunctionId, arguments, ValidationError.UnknownFunction, "Функция");
        }

        public LawType VisitBlock(BlockNode node)
        {
            PushScope();
            LawType result = LawTypes.Unit;
            var diverges = false;
            for (var i = 0; i < node.Expressions.Count; i++)
            {
                result = Visit(node.Expressions[i], "expressions", i);
                diverges |= result is NeverType;
            }

            PopScope();
            return diverges ? LawTypes.Never : result;
        }

        public LawType VisitIf(IfNode node)
        {
            var condition = VisitExpecting(node.Condition, "condition", LawTypes.Boolean, "Условие");

            var then = VisitScoped(node.Then, "then");
            if (node.Otherwise == null)
            {
                return condition is NeverType ? condition : LawTypes.Unit;
            }

            var otherwise = VisitScoped(node.Otherwise, "otherwise");
            return condition is NeverType ? condition : Join(then, otherwise);
        }

        public LawType VisitWhile(WhileNode node)
        {
            VisitExpecting(node.Condition, "condition", LawTypes.Boolean, "Условие цикла");

            _loopDepth++;
            VisitScoped(node.Body, "body");
            _loopDepth--;
            return LawTypes.Unit;
        }

        public LawType VisitForEach(ForEachNode node)
        {
            var element = ElementOf(Visit(node.Source, "source"));

            PushScope();
            Declare(node.Variable, element);
            _loopDepth++;
            Visit(node.Body, "body");
            _loopDepth--;
            PopScope();
            return LawTypes.Unit;
        }

        public LawType VisitCall(CallNode node)
        {
            var arguments = VisitMany(node.Arguments, "arguments");
            var target = _context.Resolve<NormDefinition>(node.TargetId, "нормой");
            if (target != null)
            {
                CheckArguments(target.Name, target.Parameters, arguments);
            }

            return LawTypes.Outcome;
        }

        public LawType VisitReturn(ReturnNode node)
        {
            VisitExpecting(node.Value, "value", _norm.ResultType, "Возвращаемое значение");
            return LawTypes.Never;
        }

        public LawType VisitBreak(BreakNode node)
        {
            if (_loopDepth == 0)
            {
                _context.Report(ValidationError.BreakOutsideLoop, "«Прервать» можно использовать только внутри цикла");
            }

            return LawTypes.Never;
        }

        public LawType VisitContinue(ContinueNode node)
        {
            if (_loopDepth == 0)
            {
                _context.Report(ValidationError.ContinueOutsideLoop, "«Продолжить» можно использовать только внутри цикла");
            }

            return LawTypes.Never;
        }

        public LawType VisitField(FieldNode node)
        {
            var record = Visit(node.Record, "record");
            switch (record)
            {
                case NeverType _:
                    return record;
                case RecordType recordType when recordType.TryGetField(node.FieldName, out var fieldType):
                    return fieldType;
                case RecordType _:
                    _context.Report(ValidationError.UnknownField, $"У записи нет поля «{node.FieldName}»");
                    return LawTypes.Never;
                default:
                    _context.Report(ValidationError.NotARecord, $"Поле «{node.FieldName}» можно взять только у записи, а не у {record}");
                    return LawTypes.Never;
            }
        }

        public LawType VisitHasStatus(HasStatusNode node)
        {
            VisitExpecting(node.Subject, "subject", LawTypes.Snapshot, "Субъект");
            _context.Resolve<StatusDefinition>(node.StatusId, "статусом");
            return LawTypes.Boolean;
        }

        public LawType VisitStatusRecords(StatusRecordsNode node)
        {
            VisitExpecting(node.Subject, "subject", LawTypes.Snapshot, "Субъект");
            _context.Resolve<StatusDefinition>(node.StatusId, "статусом");
            return LawTypes.ListOf(StatusAssignment.RecordType);
        }

        public LawType VisitState(StateNode node)
        {
            VisitExpecting(node.Subject, "subject", LawTypes.Snapshot, "Субъект");
            _context.Resolve<StateDefinition>(node.StateId, "состоянием");
            return LawTypes.Boolean;
        }

        public LawType VisitCount(CountNode node)
        {
            VisitQuery(node, false);
            return LawTypes.Integer;
        }

        public LawType VisitExists(ExistsNode node)
        {
            VisitQuery(node, true);
            return LawTypes.Boolean;
        }

        public LawType VisitAll(AllNode node)
        {
            VisitQuery(node, true);
            return LawTypes.Boolean;
        }

        public LawType VisitFilter(FilterNode node)
        {
            var element = VisitQuery(node, true);
            return LawTypes.ListOf(element);
        }

        public LawType VisitSanction(SanctionNode node)
        {
            var arguments = VisitMany(node.Arguments, "arguments");
            var sanction = _context.Resolve<SanctionDefinition>(node.SanctionId, "санкцией");
            if (sanction != null)
            {
                CheckArguments(sanction.Name, sanction.Parameters, arguments);
            }

            return LawTypes.Sanction;
        }

        public LawType VisitVerdict(VerdictNode node)
        {
            for (var i = 0; i < node.Sanctions.Count; i++)
            {
                VisitExpecting(node.Sanctions[i], "sanctions", LawTypes.Sanction, "Элемент списка санкций", i);
            }

            if (node.Modality != Modality.Permitted && node.Sanctions.Count == 0)
            {
                _context.Report(
                    ValidationError.MissingSanctions,
                    "Для «запрещено» и «обязательно» нужна хотя бы одна санкция. Если наказания нет, укажите санкцию «санкций не предусмотрено»");
            }

            if (node.Reason != null)
            {
                var reason = Visit(node.Reason, "reason");
                if (!(reason is NeverType) && !reason.Equals(LawTypes.String) && !(reason is DefinitionReferenceType))
                {
                    _context.Report(ValidationError.InvalidReason, $"Причина должна быть текстом или ссылкой на определение, а не {reason}");
                }
            }

            return LawTypes.Verdict;
        }

        public LawType VisitNotApplicable(NotApplicableNode node) => LawTypes.Outcome;

        private LawType Visit(LawNode node, string role, int index = -1)
        {
            _context.Enter(index < 0 ? role : role + "[" + index + "]");
            var type = node.Accept(this);
            _context.Leave();
            return type;
        }

        private LawType VisitExpecting(LawNode node, string role, LawType expected, string what, int index = -1)
        {
            _context.Enter(index < 0 ? role : role + "[" + index + "]");
            var type = node.Accept(this);
            Expect(type, expected, what);
            _context.Leave();
            return type;
        }

        private LawType VisitScoped(LawNode node, string role)
        {
            PushScope();
            var type = Visit(node, role);
            PopScope();
            return type;
        }

        private LawType[] VisitMany(IReadOnlyList<LawNode> nodes, string role)
        {
            var types = new LawType[nodes.Count];
            for (var i = 0; i < nodes.Count; i++)
            {
                types[i] = Visit(nodes[i], role, i);
            }

            return types;
        }

        private LawType VisitLogical(LawNode left, LawNode right)
        {
            var leftType = VisitExpecting(left, "left", LawTypes.Boolean, "Левый операнд");
            VisitExpecting(right, "right", LawTypes.Boolean, "Правый операнд");
            return leftType is NeverType ? leftType : LawTypes.Boolean;
        }

        private LawType VisitQuery(ListQueryNode node, bool predicateRequired)
        {
            var element = ElementOf(Visit(node.Source, "source"));

            if (node.Predicate == null)
            {
                if (predicateRequired)
                {
                    _context.Report(ValidationError.MissingPredicate, "Для этого запроса нужно условие");
                }

                return element;
            }

            PushScope();
            Declare(node.Variable, element);
            var outerLoopDepth = _loopDepth;
            _loopDepth = 0;
            VisitExpecting(node.Predicate, "predicate", LawTypes.Boolean, "Условие запроса");
            _loopDepth = outerLoopDepth;
            PopScope();
            return element;
        }

        private LawType ElementOf(LawType source)
        {
            switch (source)
            {
                case NeverType _:
                    return source;
                case ListType list:
                    return list.Element;
                default:
                    _context.Report(ValidationError.NotAList, $"Ожидается список, получено {source}");
                    return LawTypes.Never;
            }
        }

        private LawType ResolveIn(OperationTable table, string id, LawType[] operands, string unknownCode, string what)
        {
            var resolution = table.Resolve(id, operands);
            var types = string.Join(", ", operands.Select(t => t.ToString()));
            switch (resolution.Status)
            {
                case OperationResolutionStatus.Resolved:
                    return resolution.ResultType;
                case OperationResolutionStatus.UnknownOperation:
                    _context.Report(unknownCode, $"{what} «{id}» не существует");
                    break;
                case OperationResolutionStatus.NoMatchingOverload:
                    _context.Report(ValidationError.NoMatchingOverload, $"{what} «{id}» не определена для типов ({types})");
                    break;
                case OperationResolutionStatus.Ambiguous:
                    _context.Report(ValidationError.AmbiguousOverload, $"{what} «{id}» неоднозначна для типов ({types})");
                    break;
            }

            return LawTypes.Never;
        }

        private void CheckArguments(string targetName, IReadOnlyList<TypedParameter> parameters, LawType[] arguments)
        {
            if (parameters.Count != arguments.Length)
            {
                _context.Report(ValidationError.ArgumentCount, $"«{targetName}» ожидает аргументов: {parameters.Count}, передано: {arguments.Length}");
                return;
            }

            for (var i = 0; i < arguments.Length; i++)
            {
                _context.Enter("arguments[" + i + "]");
                Expect(arguments[i], parameters[i].Type, $"Аргумент «{parameters[i].Name}»");
                _context.Leave();
            }
        }

        private void Expect(LawType actual, LawType expected, string what)
        {
            if (!actual.IsAssignableTo(expected))
            {
                _context.Report(ValidationError.TypeMismatch, $"{what}: ожидается {expected}, получено {actual}");
            }
        }

        private static LawType Join(LawType left, LawType right)
        {
            if (left is NeverType || left.IsAssignableTo(right))
            {
                return right;
            }

            if (right is NeverType || right.IsAssignableTo(left))
            {
                return left;
            }

            return LawTypes.Unit;
        }

        private void PushScope() => _scopes.Add(new Dictionary<string, LawType>(StringComparer.Ordinal));

        private void PopScope() => _scopes.RemoveAt(_scopes.Count - 1);

        private void Declare(string name, LawType type)
        {
            if (_parameters.ContainsKey(name) || TryFindLocal(name, out _))
            {
                _context.Report(ValidationError.DuplicateLocal, $"Имя «{name}» уже занято");
                return;
            }

            _scopes[_scopes.Count - 1].Add(name, type);
        }

        private bool TryFindLocal(string name, out LawType type)
        {
            for (var i = _scopes.Count - 1; i >= 0; i--)
            {
                if (_scopes[i].TryGetValue(name, out var found))
                {
                    type = found;
                    return true;
                }
            }

            type = LawTypes.Never;
            return false;
        }
    }
}
