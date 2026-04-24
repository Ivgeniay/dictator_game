using Dictator.Domain.Laws.Bill_;
using System;

namespace Dictator.Domain.Laws.Law_
{
    public static class LawFactory
    {
        public static Law FromBill(Bill bill, long currentTick)
        {
            return new Law(
                id:            Guid.NewGuid(),
                billId:        bill.Id,
                name:          bill.Name,
                description:   bill.Description,
                statement:     new LawStatement(bill.Statement),
                adoptedAtTick: currentTick);
        }
    }
}