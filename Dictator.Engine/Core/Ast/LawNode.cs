using System.Collections.Generic;
using System.Linq;
using Dictator.Engine.Core.Values;

namespace Dictator.Engine.Core.Ast
{
    public readonly struct NodeChild
    {
        public NodeChild(string role, LawNode node, int index = -1)
        {
            Role = role;
            Node = node;
            Index = index;
        }

        public string Role { get; }

        public LawNode Node { get; }

        public int Index { get; }

        public override string ToString() => Index < 0 ? Role : Role + "[" + Index + "]";
    }

    public abstract class LawNode
    {
        public abstract IEnumerable<NodeChild> Children { get; }

        public abstract TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor);

        protected static IEnumerable<NodeChild> Many(string role, IReadOnlyList<LawNode> nodes) =>
            nodes.Select((node, index) => new NodeChild(role, node, index));

        protected static IEnumerable<NodeChild> One(string role, LawNode? node) =>
            node == null ? Enumerable.Empty<NodeChild>() : new[] { new NodeChild(role, node) };

        protected static IEnumerable<NodeChild> None() => Enumerable.Empty<NodeChild>();
    }

    public interface ILawNodeVisitor<TResult>
    {
        TResult VisitLiteral(LiteralNode node);
        TResult VisitDefinitionReference(DefinitionReferenceNode node);
        TResult VisitParameter(ParameterNode node);
        TResult VisitLocal(LocalNode node);
        TResult VisitDeclare(DeclareNode node);
        TResult VisitAssign(AssignNode node);
        TResult VisitOperation(OperationNode node);
        TResult VisitAnd(AndNode node);
        TResult VisitOr(OrNode node);
        TResult VisitNot(NotNode node);
        TResult VisitBuiltinCall(BuiltinCallNode node);
        TResult VisitBlock(BlockNode node);
        TResult VisitIf(IfNode node);
        TResult VisitWhile(WhileNode node);
        TResult VisitForEach(ForEachNode node);
        TResult VisitCall(CallNode node);
        TResult VisitReturn(ReturnNode node);
        TResult VisitBreak(BreakNode node);
        TResult VisitContinue(ContinueNode node);
        TResult VisitField(FieldNode node);
        TResult VisitHasStatus(HasStatusNode node);
        TResult VisitStatusRecords(StatusRecordsNode node);
        TResult VisitState(StateNode node);
        TResult VisitCount(CountNode node);
        TResult VisitExists(ExistsNode node);
        TResult VisitAll(AllNode node);
        TResult VisitFilter(FilterNode node);
        TResult VisitSanction(SanctionNode node);
        TResult VisitVerdict(VerdictNode node);
        TResult VisitNotApplicable(NotApplicableNode node);
    }

    public sealed class LiteralNode : LawNode
    {
        public LiteralNode(LawValue value)
        {
            Value = value;
        }

        public LawValue Value { get; }

        public override IEnumerable<NodeChild> Children => None();

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitLiteral(this);
    }

    public sealed class DefinitionReferenceNode : LawNode
    {
        public DefinitionReferenceNode(string definitionId)
        {
            DefinitionId = definitionId;
        }

        public string DefinitionId { get; }

        public override IEnumerable<NodeChild> Children => None();

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitDefinitionReference(this);
    }

    public sealed class ParameterNode : LawNode
    {
        public ParameterNode(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override IEnumerable<NodeChild> Children => None();

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitParameter(this);
    }

    public sealed class LocalNode : LawNode
    {
        public LocalNode(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override IEnumerable<NodeChild> Children => None();

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitLocal(this);
    }

    public sealed class DeclareNode : LawNode
    {
        public DeclareNode(string name, LawNode value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public LawNode Value { get; }

        public override IEnumerable<NodeChild> Children => One("value", Value);

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitDeclare(this);
    }

    public sealed class AssignNode : LawNode
    {
        public AssignNode(string name, LawNode value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public LawNode Value { get; }

        public override IEnumerable<NodeChild> Children => One("value", Value);

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitAssign(this);
    }

    public sealed class OperationNode : LawNode
    {
        private readonly LawNode[] _operands;

        public OperationNode(string operationId, params LawNode[] operands)
        {
            OperationId = operationId;
            _operands = operands.ToArray();
        }

        public string OperationId { get; }

        public IReadOnlyList<LawNode> Operands => _operands;

        public override IEnumerable<NodeChild> Children => Many("operands", _operands);

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitOperation(this);
    }

    public sealed class AndNode : LawNode
    {
        public AndNode(LawNode left, LawNode right)
        {
            Left = left;
            Right = right;
        }

        public LawNode Left { get; }

        public LawNode Right { get; }

        public override IEnumerable<NodeChild> Children => One("left", Left).Concat(One("right", Right));

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitAnd(this);
    }

    public sealed class OrNode : LawNode
    {
        public OrNode(LawNode left, LawNode right)
        {
            Left = left;
            Right = right;
        }

        public LawNode Left { get; }

        public LawNode Right { get; }

        public override IEnumerable<NodeChild> Children => One("left", Left).Concat(One("right", Right));

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitOr(this);
    }

    public sealed class NotNode : LawNode
    {
        public NotNode(LawNode operand)
        {
            Operand = operand;
        }

        public LawNode Operand { get; }

        public override IEnumerable<NodeChild> Children => One("operand", Operand);

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitNot(this);
    }

    public sealed class BuiltinCallNode : LawNode
    {
        private readonly LawNode[] _arguments;

        public BuiltinCallNode(string functionId, params LawNode[] arguments)
        {
            FunctionId = functionId;
            _arguments = arguments.ToArray();
        }

        public string FunctionId { get; }

        public IReadOnlyList<LawNode> Arguments => _arguments;

        public override IEnumerable<NodeChild> Children => Many("arguments", _arguments);

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitBuiltinCall(this);
    }

    public sealed class BlockNode : LawNode
    {
        private readonly LawNode[] _expressions;

        public BlockNode(params LawNode[] expressions)
        {
            _expressions = expressions.ToArray();
        }

        public IReadOnlyList<LawNode> Expressions => _expressions;

        public override IEnumerable<NodeChild> Children => Many("expressions", _expressions);

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitBlock(this);
    }

    public sealed class IfNode : LawNode
    {
        public IfNode(LawNode condition, LawNode then, LawNode? otherwise = null)
        {
            Condition = condition;
            Then = then;
            Otherwise = otherwise;
        }

        public LawNode Condition { get; }

        public LawNode Then { get; }

        public LawNode? Otherwise { get; }

        public override IEnumerable<NodeChild> Children => One("condition", Condition).Concat(One("then", Then)).Concat(One("otherwise", Otherwise));

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitIf(this);
    }

    public sealed class WhileNode : LawNode
    {
        public WhileNode(LawNode condition, LawNode body)
        {
            Condition = condition;
            Body = body;
        }

        public LawNode Condition { get; }

        public LawNode Body { get; }

        public override IEnumerable<NodeChild> Children => One("condition", Condition).Concat(One("body", Body));

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitWhile(this);
    }

    public sealed class ForEachNode : LawNode
    {
        public ForEachNode(LawNode source, string variable, LawNode body)
        {
            Source = source;
            Variable = variable;
            Body = body;
        }

        public LawNode Source { get; }

        public string Variable { get; }

        public LawNode Body { get; }

        public override IEnumerable<NodeChild> Children => One("source", Source).Concat(One("body", Body));

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitForEach(this);
    }

    public sealed class CallNode : LawNode
    {
        private readonly LawNode[] _arguments;

        public CallNode(string targetId, params LawNode[] arguments)
        {
            TargetId = targetId;
            _arguments = arguments.ToArray();
        }

        public string TargetId { get; }

        public IReadOnlyList<LawNode> Arguments => _arguments;

        public override IEnumerable<NodeChild> Children => Many("arguments", _arguments);

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitCall(this);
    }

    public sealed class ReturnNode : LawNode
    {
        public ReturnNode(LawNode value)
        {
            Value = value;
        }

        public LawNode Value { get; }

        public override IEnumerable<NodeChild> Children => One("value", Value);

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitReturn(this);
    }

    public sealed class BreakNode : LawNode
    {
        public override IEnumerable<NodeChild> Children => None();

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitBreak(this);
    }

    public sealed class ContinueNode : LawNode
    {
        public override IEnumerable<NodeChild> Children => None();

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitContinue(this);
    }

    public sealed class FieldNode : LawNode
    {
        public FieldNode(LawNode record, string fieldName)
        {
            Record = record;
            FieldName = fieldName;
        }

        public LawNode Record { get; }

        public string FieldName { get; }

        public override IEnumerable<NodeChild> Children => One("record", Record);

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitField(this);
    }

    public sealed class HasStatusNode : LawNode
    {
        public HasStatusNode(LawNode subject, string statusId)
        {
            Subject = subject;
            StatusId = statusId;
        }

        public LawNode Subject { get; }

        public string StatusId { get; }

        public override IEnumerable<NodeChild> Children => One("subject", Subject);

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitHasStatus(this);
    }

    public sealed class StatusRecordsNode : LawNode
    {
        public StatusRecordsNode(LawNode subject, string statusId)
        {
            Subject = subject;
            StatusId = statusId;
        }

        public LawNode Subject { get; }

        public string StatusId { get; }

        public override IEnumerable<NodeChild> Children => One("subject", Subject);

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitStatusRecords(this);
    }

    public sealed class StateNode : LawNode
    {
        public StateNode(LawNode subject, string stateId)
        {
            Subject = subject;
            StateId = stateId;
        }

        public LawNode Subject { get; }

        public string StateId { get; }

        public override IEnumerable<NodeChild> Children => One("subject", Subject);

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitState(this);
    }

    public abstract class ListQueryNode : LawNode
    {
        protected ListQueryNode(LawNode source, string variable, LawNode? predicate)
        {
            Source = source;
            Variable = variable;
            Predicate = predicate;
        }

        public LawNode Source { get; }

        public string Variable { get; }

        public LawNode? Predicate { get; }

        public override IEnumerable<NodeChild> Children => One("source", Source).Concat(One("predicate", Predicate));
    }

    public sealed class CountNode : ListQueryNode
    {
        public CountNode(LawNode source, string variable, LawNode? predicate = null)
            : base(source, variable, predicate)
        {
        }

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitCount(this);
    }

    public sealed class ExistsNode : ListQueryNode
    {
        public ExistsNode(LawNode source, string variable, LawNode predicate)
            : base(source, variable, predicate)
        {
        }

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitExists(this);
    }

    public sealed class AllNode : ListQueryNode
    {
        public AllNode(LawNode source, string variable, LawNode predicate)
            : base(source, variable, predicate)
        {
        }

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitAll(this);
    }

    public sealed class FilterNode : ListQueryNode
    {
        public FilterNode(LawNode source, string variable, LawNode predicate)
            : base(source, variable, predicate)
        {
        }

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitFilter(this);
    }

    public sealed class SanctionNode : LawNode
    {
        private readonly LawNode[] _arguments;

        public SanctionNode(string sanctionId, params LawNode[] arguments)
        {
            SanctionId = sanctionId;
            _arguments = arguments.ToArray();
        }

        public string SanctionId { get; }

        public IReadOnlyList<LawNode> Arguments => _arguments;

        public override IEnumerable<NodeChild> Children => Many("arguments", _arguments);

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitSanction(this);
    }

    public sealed class VerdictNode : LawNode
    {
        private readonly LawNode[] _sanctions;

        public VerdictNode(Modality modality, IEnumerable<LawNode> sanctions, LawNode? reason = null)
        {
            Modality = modality;
            _sanctions = sanctions.ToArray();
            Reason = reason;
        }

        public Modality Modality { get; }

        public IReadOnlyList<LawNode> Sanctions => _sanctions;

        public LawNode? Reason { get; }

        public override IEnumerable<NodeChild> Children => Many("sanctions", _sanctions).Concat(One("reason", Reason));

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitVerdict(this);
    }

    public sealed class NotApplicableNode : LawNode
    {
        public static readonly NotApplicableNode Instance = new NotApplicableNode();

        private NotApplicableNode()
        {
        }

        public override IEnumerable<NodeChild> Children => None();

        public override TResult Accept<TResult>(ILawNodeVisitor<TResult> visitor) => visitor.VisitNotApplicable(this);
    }
}
