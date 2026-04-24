namespace Dictator.Domain.Laws.Bill_
{
    public class BillStatement
    {
        public SubjectGroupNode Subject { get; }
        public ActionNode Action { get; }
        public RestrictionNode Restriction { get; }
        public CircumstanceGroupNode? Circumstance { get; }

        public BillStatement(
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