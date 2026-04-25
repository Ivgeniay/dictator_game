using Dictator.Domain.Shared.LegalTerms;
using System.Collections.Generic;
using Dictator.Domain.Utils;
using System.Linq;
using System;
using Dictator.Domain.Shared;

namespace Dictator.Domain.Laws
{
    public enum LogicOperator
    {
        And,
        Or,
        Not,
    }


    public abstract class Node 
    {
        public string Type { get; protected set; } = string.Empty;
        public abstract Node Clone();
    }

    public class LogicOperatorNode : Node
    {
        public LogicOperator Op { get; }

        public LogicOperatorNode(LogicOperator op)
        {
            this.Op = op;
            Type = GetType().FullName;
        }

        public override Node Clone()
        {
            return new LogicOperatorNode(Op);
        }
    }

    public class SubjectNode : Node
    {
        public SubjectType Subject { get; }
        public bool IsEmpty => Subject.Value == Constants.Empty.Value;
        public static readonly SubjectNode Empty = new SubjectNode(new SubjectType(Constants.Empty.Value));

        public SubjectNode(SubjectType subject)
        {
            Subject = subject;
            Type = GetType().FullName;
        }

        public override Node Clone()
        {
            return new SubjectNode(new SubjectType(Subject.Value));
        }
    }

    public class ActionNode : Node
    {
        public ActionType Action { get; }
        public bool IsEmpty => Action.Value == Constants.Empty.Value;
        public static readonly ActionNode Empty = new ActionNode(new ActionType(Constants.Empty.Value));
        public ActionNode(ActionType action)
        {
            Action = action;
            Type = GetType().FullName;
        }

        public override Node Clone()
        {
            return new ActionNode(new ActionType(Action.Value));
        }
    }

    public class SubjectGroupNode : Node
    {
        public IReadOnlyList<Node> Children { get; }
        public SubjectGroupNode(List<Node>children)
        {
            Children = children ?? new List<Node>();
            Type = GetType().FullName;
        }

        public override Node Clone()
        {
            return new SubjectGroupNode(Children.Select(c => c.Clone()).ToList());
        }
    }

    public class RestrictionNode : Node
    {
        public RestrictionType Restriction { get; }
        public bool IsEmpty => Restriction.Value == Constants.Empty.Value;
        public static readonly RestrictionNode Empty = new RestrictionNode(new RestrictionType(Constants.Empty.Value, -1));

        public RestrictionNode(RestrictionType restriction)
        {
            Restriction = restriction;
            Type = GetType().FullName;
        }

        public override Node Clone()
        {
            return new RestrictionNode(new RestrictionType(Restriction.Value, Restriction.Severity));
        }
    }

    public class CircumstanceGroupNode : Node
    {
        public IReadOnlyList<Node>Children { get; }
        public CircumstanceGroupNode(List<Node> children)
        {
            Children = children ?? new List<Node>();
            Type = GetType().FullName;
        }

        public override Node Clone()
        {
            return new CircumstanceGroupNode(Children.Select(c => c.Clone()).ToList());
        }
    }

    public class CircumstanceNode : Node
    {
        public CircumstanceType Circumstance { get; }
        public bool IsEmpty => Circumstance.Value == Constants.Empty.Value;
        public static readonly CircumstanceNode Empty = new CircumstanceNode(new CircumstanceType(Constants.Empty.Value));

        public CircumstanceNode(CircumstanceType circumstance)
        {
            Circumstance = circumstance;
            Type = GetType().FullName;
        }

        public override Node Clone()
        {
            return new CircumstanceNode(new CircumstanceType(Circumstance.Value));
        }
    }
}