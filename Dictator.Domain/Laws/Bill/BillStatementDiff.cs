using System.Collections.Generic;
using System.Linq;

namespace Dictator.Domain.Laws.Bill
{
    public static class BillStatementDiff
    {
        public static BillStatementDiffResult Compare(
            BillStatement previous,
            BillStatement current,
            BillDiffWeights? weights = null)
        {
            weights = weights ?? BillDiffWeights.Default;

            var restrictionChange = CompareRestriction(previous.Restriction, current.Restriction);
            bool isRestrictiveContext = current.Restriction.Restriction.IsRestrictive;

            var subjectChanges = CompareSubjects(previous.Subject, current.Subject, isRestrictiveContext);
            var circumstanceChanges = CompareCircumstances(previous.Circumstance, current.Circumstance, isRestrictiveContext);
            var actionChange = CompareAction(previous.Action, current.Action);

            double subjectDelta = CalculateGroupDelta(subjectChanges.Select(c => c.Direction).ToList());
            double restrictionDelta = restrictionChange.Delta;
            double circumstanceDelta = CalculateGroupDelta(circumstanceChanges.Select(c => c.Direction).ToList());

            double totalDelta =
                subjectDelta      * weights.SubjectWeight +
                restrictionDelta  * weights.RestrictionWeight +
                circumstanceDelta * weights.CircumstanceWeight;

            var overallDirection = totalDelta > 0
                ? BillChangeDirection.Stricter
                : totalDelta < 0
                    ? BillChangeDirection.Softer
                    : BillChangeDirection.Unchanged;

            return new BillStatementDiffResult(
                subjectChanges,
                actionChange,
                restrictionChange,
                circumstanceChanges,
                overallDirection,
                totalDelta);
        }

        private static BillRestrictionChange CompareRestriction(
            BillRestrictionNode previous,
            BillRestrictionNode current)
        {
            double delta = current.Restriction.Severity - previous.Restriction.Severity;

            var direction = delta > 0
                ? BillChangeDirection.Stricter
                : delta < 0
                    ? BillChangeDirection.Softer
                    : BillChangeDirection.Unchanged;

            return BillRestrictionChange.Create(previous, current, direction, delta);
        }

        private static IReadOnlyList<BillSubjectChange> CompareSubjects(
            BillSubjectGroupNode previous,
            BillSubjectGroupNode current,
            bool isRestrictiveContext)
        {
            var changes = new List<BillSubjectChange>();

            var previousSubjects = ExtractSubjects(previous);
            var currentSubjects = ExtractSubjects(current);

            bool previousHasAll = previousSubjects.Any(s => s.Subject.Equals(SubjectType.All));
            bool currentHasAll = currentSubjects.Any(s => s.Subject.Equals(SubjectType.All));

            if (!previousHasAll && currentHasAll)
            {
                var allNode = currentSubjects.First(s => s.Subject.Equals(SubjectType.All));
                var direction = isRestrictiveContext
                    ? BillChangeDirection.Stricter
                    : BillChangeDirection.Softer;
                changes.Add(BillSubjectChange.Added(allNode, direction));
                return changes;
            }

            if (previousHasAll && !currentHasAll)
            {
                var allNode = previousSubjects.First(s => s.Subject.Equals(SubjectType.All));
                var direction = isRestrictiveContext
                    ? BillChangeDirection.Softer
                    : BillChangeDirection.Stricter;
                changes.Add(BillSubjectChange.Removed(allNode, direction));
                return changes;
            }

            foreach (var subject in currentSubjects.Where(s => !s.Subject.Equals(SubjectType.All)))
            {
                var match = previousSubjects.FirstOrDefault(s => s.Subject.Equals(subject.Subject));
                if (match == null)
                {
                    var direction = isRestrictiveContext
                        ? BillChangeDirection.Stricter
                        : BillChangeDirection.Softer;
                    changes.Add(BillSubjectChange.Added(subject, direction));
                }
            }

            foreach (var subject in previousSubjects.Where(s => !s.Subject.Equals(SubjectType.All)))
            {
                var match = currentSubjects.FirstOrDefault(s => s.Subject.Equals(subject.Subject));
                if (match == null)
                {
                    var direction = isRestrictiveContext
                        ? BillChangeDirection.Softer
                        : BillChangeDirection.Stricter;
                    changes.Add(BillSubjectChange.Removed(subject, direction));
                }
            }

            return changes;
        }

        private static IReadOnlyList<BillCircumstanceChange> CompareCircumstances(
            BillCircumstanceGroupNode? previous,
            BillCircumstanceGroupNode? current,
            bool isRestrictiveContext)
        {
            var changes = new List<BillCircumstanceChange>();

            if (previous == null && current == null)
                return changes;

            var previousCircumstances = previous != null
                ? ExtractCircumstances(previous)
                : new List<BillCircumstanceNode>();

            var currentCircumstances = current != null
                ? ExtractCircumstances(current)
                : new List<BillCircumstanceNode>();

            foreach (var circumstance in currentCircumstances.Where(c => !c.IsEmpty))
            {
                var match = previousCircumstances.FirstOrDefault(c => c.Circumstance.Equals(circumstance.Circumstance));
                if (match == null)
                {
                    var direction = isRestrictiveContext
                        ? BillChangeDirection.Softer
                        : BillChangeDirection.Stricter;
                    changes.Add(BillCircumstanceChange.Added(circumstance, direction));
                }
            }

            foreach (var circumstance in previousCircumstances.Where(c => !c.IsEmpty))
            {
                var match = currentCircumstances.FirstOrDefault(c => c.Circumstance.Equals(circumstance.Circumstance));
                if (match == null)
                {
                    var direction = isRestrictiveContext
                        ? BillChangeDirection.Stricter
                        : BillChangeDirection.Softer;
                    changes.Add(BillCircumstanceChange.Removed(circumstance, direction));
                }
            }

            return changes;
        }

        private static BillActionChange CompareAction(
            BillActionNode previous,
            BillActionNode current)
        {
            if (previous.Action.Equals(current.Action))
                return BillActionChange.NotModified;

            return BillActionChange.Modified(previous, current);
}

        private static List<BillSubjectNode> ExtractSubjects(BillSubjectGroupNode group)
        {
            return group.Children
                .OfType<BillSubjectNode>()
                .ToList();
        }

        private static List<BillCircumstanceNode> ExtractCircumstances(BillCircumstanceGroupNode group)
        {
            return group.Children
                .OfType<BillCircumstanceNode>()
                .ToList();
        }

        private static double CalculateGroupDelta(IList<BillChangeDirection> directions)
        {
            double delta = 0;
            foreach (var direction in directions)
            {
                if (direction == BillChangeDirection.Stricter) delta += 1;
                else if (direction == BillChangeDirection.Softer) delta -= 1;
            }
            return delta;
        }
    }
}