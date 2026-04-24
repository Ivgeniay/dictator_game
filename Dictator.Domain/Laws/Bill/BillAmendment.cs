namespace Dictator.Domain.Laws.Bill_
{
    /// <summary>
    /// Поправка к законопроекту
    /// </summary>
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
 