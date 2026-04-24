using Dictator.Domain.Laws.Bill_;

namespace Dictator.Domain.Laws.Law_
{
    public class LawStatement
    {
        public SubjectGroupNode Subject { get; }
        public ActionNode Action { get; }
        public RestrictionNode Restriction { get; }
        public CircumstanceGroupNode? Circumstance { get; }

        public LawStatement(BillStatement billStatement)
        {
            Subject = (SubjectGroupNode)billStatement.Subject.Clone();
            Action = (ActionNode)billStatement.Action.Clone();
            Restriction = (RestrictionNode)billStatement.Restriction.Clone();
            Circumstance = billStatement.Circumstance != null
                ? (CircumstanceGroupNode)billStatement.Circumstance.Clone()
                : null;
        }

        public LawStatement(
            SubjectGroupNode subject,
            ActionNode action,
            RestrictionNode restriction,
            CircumstanceGroupNode? circumstance = null)
        {
            Subject = subject;
            Action = action;
            Restriction = restriction;
            Circumstance = circumstance;
        }
    }
}