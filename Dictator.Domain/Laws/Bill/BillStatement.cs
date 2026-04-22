namespace Dictator.Domain.Laws.Bill
{
    public class BillStatement
    {
        public BillSubjectGroupNode Subject { get; }
        public BillActionNode Action { get; }
        public BillRestrictionNode Restriction { get; }
        public BillCircumstanceGroupNode? Circumstance { get; }

        public BillStatement(
            BillSubjectGroupNode subject,
            BillActionNode action,
            BillRestrictionNode restriction,
            BillCircumstanceGroupNode? circumstance = null)
        {
            Subject = subject;
            Action = action;
            Restriction = restriction;
            Circumstance = circumstance;
        }
    }
}