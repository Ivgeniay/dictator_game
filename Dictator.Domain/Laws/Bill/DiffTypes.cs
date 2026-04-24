using System.Collections.Generic;

namespace Dictator.Domain.Laws.Bill_
{
    public enum BillChangeDirection
    {
        Stricter,
        Softer,
        Unchanged
    }

    public enum BillChangePresence
    {
        Added,
        Removed,
        Modified,
        NotModified,
    }

    public class BillDiffWeights
    {
        public double SubjectWeight { get; }
        public double RestrictionWeight { get; }
        public double CircumstanceWeight { get; }

        public BillDiffWeights(
            double subjectWeight,
            double restrictionWeight,
            double circumstanceWeight)
        {
            SubjectWeight = subjectWeight;
            RestrictionWeight = restrictionWeight;
            CircumstanceWeight = circumstanceWeight;
        }

        public static readonly BillDiffWeights Default = new BillDiffWeights(
            subjectWeight:      1.0,
            restrictionWeight:  2.0,
            circumstanceWeight: 0.5
        );
    }

    public class BillSubjectChange
    {
        public SubjectNode Previous { get; }
        public SubjectNode Current { get; }
        public BillChangeDirection Direction { get; }
        public BillChangePresence Presence { get; }

        private BillSubjectChange(
            SubjectNode previous,
            SubjectNode current,
            BillChangeDirection direction,
            BillChangePresence presence)
        {
            Previous = previous;
            Current = current;
            Direction = direction;
            Presence = presence;
        }

        public static BillSubjectChange Added(SubjectNode current, BillChangeDirection direction)
        {
            return new BillSubjectChange(SubjectNode.Empty, current, direction, BillChangePresence.Added);
        }

        public static BillSubjectChange Removed(SubjectNode previous, BillChangeDirection direction)
        {
            return new BillSubjectChange(previous, SubjectNode.Empty, direction, BillChangePresence.Removed);
        }

        public static BillSubjectChange Modified(SubjectNode previous, SubjectNode current, BillChangeDirection direction)
        {
            return new BillSubjectChange(previous, current, direction, BillChangePresence.Modified);
        }

        public static BillSubjectChange NotModified = new BillSubjectChange(SubjectNode.Empty, SubjectNode.Empty, BillChangeDirection.Unchanged, BillChangePresence.NotModified);
    }

    public class BillActionChange
    {
        public ActionNode Previous { get; }
        public ActionNode Current { get; }
        public BillChangePresence Presence { get; }

        private BillActionChange(
            ActionNode previous,
            ActionNode current,
            BillChangePresence presence)
        {
            Previous = previous;
            Current = current;
            Presence = presence;
        }

        public static readonly BillActionChange NotModified = new BillActionChange(
            ActionNode.Empty, 
            ActionNode.Empty, 
            BillChangePresence.NotModified);

        public static BillActionChange Modified(ActionNode previous, ActionNode current)
        {
            return new BillActionChange(previous, current, BillChangePresence.Modified);
        }
    }

    public class BillRestrictionChange
    {
        public RestrictionNode Previous { get; }
        public RestrictionNode Current { get; }
        public BillChangeDirection Direction { get; }
        public double Delta { get; }

        private BillRestrictionChange(
            RestrictionNode previous,
            RestrictionNode current,
            BillChangeDirection direction,
            double delta)
        {
            Previous = previous;
            Current = current;
            Direction = direction;
            Delta = delta;
        }

        public static BillRestrictionChange Create(
            RestrictionNode previous,
            RestrictionNode current,
            BillChangeDirection direction,
            double delta)
        {
            return new BillRestrictionChange(previous, current, direction, delta);
        }
    }

    public class BillCircumstanceChange
    {
        public CircumstanceNode Previous { get; }
        public CircumstanceNode Current { get; }
        public BillChangeDirection Direction { get; }
        public BillChangePresence Presence { get; }

        private BillCircumstanceChange(
            CircumstanceNode previous,
            CircumstanceNode current,
            BillChangeDirection direction,
            BillChangePresence presence)
        {
            Previous = previous;
            Current = current;
            Direction = direction;
            Presence = presence;
        }

        public static BillCircumstanceChange Added(CircumstanceNode current, BillChangeDirection direction)
        {
            return new BillCircumstanceChange(CircumstanceNode.Empty, current, direction, BillChangePresence.Added);
        }

        public static BillCircumstanceChange Removed(CircumstanceNode previous, BillChangeDirection direction)
        {
            return new BillCircumstanceChange(previous, CircumstanceNode.Empty, direction, BillChangePresence.Removed);
        }
    }

    public class BillStatementDiffResult
    {
        public IReadOnlyList<BillSubjectChange> SubjectChanges { get; }
        public BillActionChange ActionChange { get; }
        public BillRestrictionChange RestrictionChange { get; }
        public IReadOnlyList<BillCircumstanceChange> CircumstanceChanges { get; }
        public BillChangeDirection OverallDirection { get; }
        public double TotalDelta { get; }

        public BillStatementDiffResult(
            IReadOnlyList<BillSubjectChange> subjectChanges,
            BillActionChange actionChange,
            BillRestrictionChange restrictionChange,
            IReadOnlyList<BillCircumstanceChange> circumstanceChanges,
            BillChangeDirection overallDirection,
            double totalDelta)
        {
            SubjectChanges = subjectChanges;
            ActionChange = actionChange;
            RestrictionChange = restrictionChange;
            CircumstanceChanges = circumstanceChanges;
            OverallDirection = overallDirection;
            TotalDelta = totalDelta;
        }
    }
}