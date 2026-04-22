using System.Collections.Generic;

namespace Dictator.Domain.Laws.Bill
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
        public BillSubjectNode Previous { get; }
        public BillSubjectNode Current { get; }
        public BillChangeDirection Direction { get; }
        public BillChangePresence Presence { get; }

        private BillSubjectChange(
            BillSubjectNode previous,
            BillSubjectNode current,
            BillChangeDirection direction,
            BillChangePresence presence)
        {
            Previous = previous;
            Current = current;
            Direction = direction;
            Presence = presence;
        }

        public static BillSubjectChange Added(BillSubjectNode current, BillChangeDirection direction)
        {
            return new BillSubjectChange(BillSubjectNode.Empty, current, direction, BillChangePresence.Added);
        }

        public static BillSubjectChange Removed(BillSubjectNode previous, BillChangeDirection direction)
        {
            return new BillSubjectChange(previous, BillSubjectNode.Empty, direction, BillChangePresence.Removed);
        }

        public static BillSubjectChange Modified(BillSubjectNode previous, BillSubjectNode current, BillChangeDirection direction)
        {
            return new BillSubjectChange(previous, current, direction, BillChangePresence.Modified);
        }

        public static BillSubjectChange NotModified = new BillSubjectChange(BillSubjectNode.Empty, BillSubjectNode.Empty, BillChangeDirection.Unchanged, BillChangePresence.NotModified);
    }

    public class BillActionChange
    {
        public BillActionNode Previous { get; }
        public BillActionNode Current { get; }
        public BillChangePresence Presence { get; }

        private BillActionChange(
            BillActionNode previous,
            BillActionNode current,
            BillChangePresence presence)
        {
            Previous = previous;
            Current = current;
            Presence = presence;
        }

        public static readonly BillActionChange NotModified = new BillActionChange(
            BillActionNode.Empty, 
            BillActionNode.Empty, 
            BillChangePresence.NotModified);

        public static BillActionChange Modified(BillActionNode previous, BillActionNode current)
        {
            return new BillActionChange(previous, current, BillChangePresence.Modified);
        }
    }

    public class BillRestrictionChange
    {
        public BillRestrictionNode Previous { get; }
        public BillRestrictionNode Current { get; }
        public BillChangeDirection Direction { get; }
        public double Delta { get; }

        private BillRestrictionChange(
            BillRestrictionNode previous,
            BillRestrictionNode current,
            BillChangeDirection direction,
            double delta)
        {
            Previous = previous;
            Current = current;
            Direction = direction;
            Delta = delta;
        }

        public static BillRestrictionChange Create(
            BillRestrictionNode previous,
            BillRestrictionNode current,
            BillChangeDirection direction,
            double delta)
        {
            return new BillRestrictionChange(previous, current, direction, delta);
        }
    }

    public class BillCircumstanceChange
    {
        public BillCircumstanceNode Previous { get; }
        public BillCircumstanceNode Current { get; }
        public BillChangeDirection Direction { get; }
        public BillChangePresence Presence { get; }

        private BillCircumstanceChange(
            BillCircumstanceNode previous,
            BillCircumstanceNode current,
            BillChangeDirection direction,
            BillChangePresence presence)
        {
            Previous = previous;
            Current = current;
            Direction = direction;
            Presence = presence;
        }

        public static BillCircumstanceChange Added(BillCircumstanceNode current, BillChangeDirection direction)
        {
            return new BillCircumstanceChange(BillCircumstanceNode.Empty, current, direction, BillChangePresence.Added);
        }

        public static BillCircumstanceChange Removed(BillCircumstanceNode previous, BillChangeDirection direction)
        {
            return new BillCircumstanceChange(previous, BillCircumstanceNode.Empty, direction, BillChangePresence.Removed);
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