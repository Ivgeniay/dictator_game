using Dictator.Engine.Core.Values;

namespace Dictator.Engine.Core.Types
{
    public static class FunctionIds
    {
        public const string TimeOfDay = "core.time_of_day";
        public const string SubjectId = "core.subject_id";
        public const string ToDecimal = "core.to_decimal";
        public const string Truncate = "core.truncate";
    }

    public static class CoreFunctions
    {
        public static OperationTable CreateDefault()
        {
            var table = new OperationTable();

            table.Register(FunctionIds.TimeOfDay, new FixedOverload(LawTypes.TimeOfDay,
                v => OperationResult.Success(new TimeOfDayValue(((InstantValue)v[0]).Seconds)), LawTypes.Instant));

            table.Register(FunctionIds.SubjectId, new FixedOverload(LawTypes.String,
                v => OperationResult.Success(new StringValue(((SnapshotValue)v[0]).SubjectId)), LawTypes.Snapshot));

            table.Register(FunctionIds.ToDecimal, new FixedOverload(LawTypes.Decimal,
                v => OperationResult.Success(new DecimalValue(((IntegerValue)v[0]).Value)), LawTypes.Integer));

            table.Register(FunctionIds.Truncate, new FixedOverload(LawTypes.Integer,
                v => CoreOperations.Compute(() => new IntegerValue((long)decimal.Truncate(((DecimalValue)v[0]).Value))), LawTypes.Decimal));

            return table;
        }
    }
}
