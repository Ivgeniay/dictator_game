using System.Collections.Generic;

namespace Dictator.Domain.Laws.Bill
{
    public abstract class BillNode
    {
        public string Type { get; }

        public BillNode(string nodeType)
        {
            Type = nodeType;
        }
    }

    public class BillLogicOperatorNode : BillNode
    {
        public BillLogicOperator Operator { get; }

        public BillLogicOperatorNode(BillLogicOperator @operator) : base(nameof(BillLogicOperatorNode))
        {
            Operator = @operator;
        }
    }

    public class BillSubjectNode : BillNode
    {
        public SubjectType Subject { get; }
        public bool IsEmpty => Subject.Value == "empty";
        public static readonly BillSubjectNode Empty = new BillSubjectNode(new SubjectType("empty"));

        public BillSubjectNode(SubjectType subject) : base(nameof(BillSubjectNode))
        {
            Subject = subject;
        }
    }

    public class BillActionNode : BillNode
    {
        public ActionType Action { get; }
        public bool IsEmpty => Action.Value == "empty";
        public static readonly BillActionNode Empty = new BillActionNode(new ActionType("empty"));
        public BillActionNode(ActionType action) : base(nameof(BillActionNode))
        {
            Action = action;
        }
    }

    public class BillSubjectGroupNode : BillNode
    {
        public IReadOnlyList<BillNode> Children { get; }
        public BillSubjectGroupNode(IReadOnlyList<BillNode> children) : base(nameof(BillSubjectGroupNode))
        {
            Children = children ?? new List<BillNode>();
        }
    }

    public class BillRestrictionNode : BillNode
    {
        public RestrictionType Restriction { get; }
        public bool IsEmpty => Restriction.Value == "empty";
        public static readonly BillRestrictionNode Empty = new BillRestrictionNode(new RestrictionType("empty", -1));

        public BillRestrictionNode(RestrictionType restriction) : base(nameof(BillRestrictionNode))
        {
            Restriction = restriction;
        }
    }

    public class BillCircumstanceGroupNode : BillNode
    {
        public IReadOnlyList<BillNode> Children { get; }
        public BillCircumstanceGroupNode(IReadOnlyList<BillNode> children) : base(nameof(BillCircumstanceGroupNode))
        {
            Children = children ?? new List<BillNode>();
        }
    }

    public class BillCircumstanceNode : BillNode
    {
        public CircumstanceType Circumstance { get; }
        public bool IsEmpty => Circumstance.Value == "empty";
        public static readonly BillCircumstanceNode Empty = new BillCircumstanceNode(new CircumstanceType("empty"));

        public BillCircumstanceNode(CircumstanceType circumstance) : base(nameof(BillCircumstanceNode))
        {
            Circumstance = circumstance;
        }
    }
}