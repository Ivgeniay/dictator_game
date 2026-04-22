using System;
using System.Collections.Generic;

namespace Dictator.Domain.Laws.Bill
{
    public enum BillStatus
    {
        Draft,
        UnderReview,
        FirstReading,
        SecondReading,
        Adopted,
        Rejected,
        Repealed
    }

    public class Bill
    {
        public Guid Id { get; }
        public string Name { get; }
        public string Description { get; }
        public BillStatus Status { get; private set; }
        public BillStatement Statement { get; private set; }
        public IReadOnlyList<BillAmendment> Amendments => _amendments;

        private readonly List<BillAmendment> _amendments;

        public Bill(Guid id, string name, string description, BillStatement statement)
        {
            Id = id;
            Name = name;
            Description = description;
            Status = BillStatus.Draft;
            Statement = statement;
            _amendments = new List<BillAmendment>();
        }

        public void Amend(BillStatement newStatement, string description, long currentTick)
        {
            _amendments.Add(new BillAmendment(currentTick, Statement, description));
            Statement = newStatement;
        }

        public void SetStatus(BillStatus status)
        {
            Status = status;
        }
    }
}