namespace Dictator.Domain.Laws.Bill
{
    public class BillAmendment
    {
        public long CreatedAtTick { get; }
        public BillStatement PreviousStatement { get; }
        public string Description { get; }
        public BillAmendment(long createdAtTick, BillStatement previousStatement, string description)
        {
            CreatedAtTick = createdAtTick;
            PreviousStatement = previousStatement;
            Description = description;
        }
    }
}
 