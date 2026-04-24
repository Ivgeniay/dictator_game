using System;

namespace Dictator.Domain.Laws.Law_
{
    public class Law
    {
        public Guid Id { get; }
        public Guid BillId { get; }
        public string Name { get; }
        public string Description { get; }
        public LawStatement Statement { get; }
        public long AdoptedAtTick { get; }
        public Law(
            Guid id,
            Guid billId,
            string name,
            string description,
            LawStatement statement,
            long adoptedAtTick)
        {
            Id = id;
            BillId = billId;
            Name = name;
            Description = description;
            Statement = statement;
            AdoptedAtTick = adoptedAtTick;
        }
    }
}