using System;
using System.Collections.Generic;
using Dictator.Engine.Core.Primitives;
using Dictator.Engine.Core.Values;

namespace Dictator.Engine.Core.Types
{
    public static class CoreOperations
    {
        private static readonly (string Id, Func<int, bool> Test)[] Comparisons =
        {
            (OperationIds.Less, c => c < 0),
            (OperationIds.LessOrEqual, c => c <= 0),
            (OperationIds.Greater, c => c > 0),
            (OperationIds.GreaterOrEqual, c => c >= 0)
        };

        private static readonly ScalarType[] Numbers = { LawTypes.Integer, LawTypes.Decimal };

        public static OperationTable CreateDefault()
        {
            var table = new OperationTable();
            RegisterNumbers(table);
            RegisterQuantity(table, LawTypes.Duration, v => ((DurationValue)v).Seconds, s => new DurationValue(s));
            RegisterQuantity(table, LawTypes.Distance, v => ((DistanceValue)v).Value.Value, m => new DistanceValue(new Meter(m)));
            RegisterTime(table);
            RegisterMoney(table);
            RegisterEquality(table);
            RegisterOrdering(table);
            return table;
        }

        private static void RegisterNumbers(OperationTable table)
        {
            var integer = LawTypes.Integer;
            var number = LawTypes.Decimal;

            Fixed(table, OperationIds.Add, integer, v => Compute(() => new IntegerValue(checked(Int(v[0]) + Int(v[1])))), integer, integer);
            Fixed(table, OperationIds.Subtract, integer, v => Compute(() => new IntegerValue(checked(Int(v[0]) - Int(v[1])))), integer, integer);
            Fixed(table, OperationIds.Multiply, integer, v => Compute(() => new IntegerValue(checked(Int(v[0]) * Int(v[1])))), integer, integer);
            Fixed(table, OperationIds.Divide, number, v => Compute(() => new DecimalValue((decimal)Int(v[0]) / Int(v[1]))), integer, integer);
            Fixed(table, OperationIds.Remainder, integer, v => Compute(() => new IntegerValue(checked(Int(v[0]) % Int(v[1])))), integer, integer);
            Fixed(table, OperationIds.Negate, integer, v => Compute(() => new IntegerValue(checked(-Int(v[0])))), integer);

            Fixed(table, OperationIds.Add, number, v => Compute(() => new DecimalValue(Dec(v[0]) + Dec(v[1]))), number, number);
            Fixed(table, OperationIds.Subtract, number, v => Compute(() => new DecimalValue(Dec(v[0]) - Dec(v[1]))), number, number);
            Fixed(table, OperationIds.Multiply, number, v => Compute(() => new DecimalValue(Dec(v[0]) * Dec(v[1]))), number, number);
            Fixed(table, OperationIds.Divide, number, v => Compute(() => new DecimalValue(Dec(v[0]) / Dec(v[1]))), number, number);
            Fixed(table, OperationIds.Negate, number, v => Compute(() => new DecimalValue(-Dec(v[0]))), number);

            Fixed(table, OperationIds.Add, LawTypes.String, v => Compute(() => new StringValue(((StringValue)v[0]).Value + ((StringValue)v[1]).Value)), LawTypes.String, LawTypes.String);
        }

        private static void RegisterQuantity(OperationTable table, ScalarType quantity, Func<LawValue, decimal> get, Func<decimal, LawValue> make)
        {
            Fixed(table, OperationIds.Add, quantity, v => Compute(() => make(get(v[0]) + get(v[1]))), quantity, quantity);
            Fixed(table, OperationIds.Subtract, quantity, v => Compute(() => make(get(v[0]) - get(v[1]))), quantity, quantity);
            Fixed(table, OperationIds.Negate, quantity, v => Compute(() => make(-get(v[0]))), quantity);
            Fixed(table, OperationIds.Divide, LawTypes.Decimal, v => Compute(() => new DecimalValue(get(v[0]) / get(v[1]))), quantity, quantity);

            foreach (var factor in Numbers)
            {
                Fixed(table, OperationIds.Multiply, quantity, v => Compute(() => make(get(v[0]) * Number(v[1]))), quantity, factor);
                Fixed(table, OperationIds.Multiply, quantity, v => Compute(() => make(Number(v[0]) * get(v[1]))), factor, quantity);
                Fixed(table, OperationIds.Divide, quantity, v => Compute(() => make(get(v[0]) / Number(v[1]))), quantity, factor);
            }
        }

        private static void RegisterTime(OperationTable table)
        {
            Fixed(table, OperationIds.Subtract, LawTypes.Duration,
                v => Compute(() => new DurationValue(Instant(v[0]) - Instant(v[1]))), LawTypes.Instant, LawTypes.Instant);
            Fixed(table, OperationIds.Add, LawTypes.Instant,
                v => Compute(() => new InstantValue(Instant(v[0]) + Duration(v[1]))), LawTypes.Instant, LawTypes.Duration);
            Fixed(table, OperationIds.Add, LawTypes.Instant,
                v => Compute(() => new InstantValue(Duration(v[0]) + Instant(v[1]))), LawTypes.Duration, LawTypes.Instant);
            Fixed(table, OperationIds.Subtract, LawTypes.Instant,
                v => Compute(() => new InstantValue(Instant(v[0]) - Duration(v[1]))), LawTypes.Instant, LawTypes.Duration);

            Fixed(table, OperationIds.Add, LawTypes.TimeOfDay,
                v => Compute(() => ((TimeOfDayValue)v[0]).Shift(Duration(v[1]))), LawTypes.TimeOfDay, LawTypes.Duration);
            Fixed(table, OperationIds.Subtract, LawTypes.TimeOfDay,
                v => Compute(() => ((TimeOfDayValue)v[0]).Shift(-Duration(v[1]))), LawTypes.TimeOfDay, LawTypes.Duration);

            Fixed(table, OperationIds.Contains, LawTypes.Boolean,
                v => Success(BooleanValue.Of(((TimeOfDayIntervalValue)v[0]).Contains((TimeOfDayValue)v[1]))), LawTypes.TimeOfDayInterval, LawTypes.TimeOfDay);
            Fixed(table, OperationIds.Overlaps, LawTypes.Boolean,
                v => Success(BooleanValue.Of(((TimeOfDayIntervalValue)v[0]).Overlaps((TimeOfDayIntervalValue)v[1]))), LawTypes.TimeOfDayInterval, LawTypes.TimeOfDayInterval);
        }

        private static void RegisterMoney(OperationTable table)
        {
            Rule(table, OperationIds.Add, 2, SameCurrency, v => Compute(() => Money(v[0]).WithAmount(Money(v[0]).Amount + Money(v[1]).Amount)));
            Rule(table, OperationIds.Subtract, 2, SameCurrency, v => Compute(() => Money(v[0]).WithAmount(Money(v[0]).Amount - Money(v[1]).Amount)));
            Rule(table, OperationIds.Negate, 1, t => t[0] as MoneyType, v => Compute(() => Money(v[0]).WithAmount(-Money(v[0]).Amount)));
            Rule(table, OperationIds.Divide, 2, t => SameCurrency(t) != null ? LawTypes.Decimal : null,
                v => Compute(() => new DecimalValue(Money(v[0]).Amount / Money(v[1]).Amount)));
            Rule(table, OperationIds.Multiply, 2, t => IsNumber(t[1]) ? t[0] as MoneyType : null,
                v => Compute(() => Money(v[0]).WithAmount(Money(v[0]).Amount * Number(v[1]))));
            Rule(table, OperationIds.Multiply, 2, t => IsNumber(t[0]) ? t[1] as MoneyType : null,
                v => Compute(() => Money(v[1]).WithAmount(Number(v[0]) * Money(v[1]).Amount)));
            Rule(table, OperationIds.Divide, 2, t => IsNumber(t[1]) ? t[0] as MoneyType : null,
                v => Compute(() => Money(v[0]).WithAmount(Money(v[0]).Amount / Number(v[1]))));
        }

        private static void RegisterEquality(OperationTable table)
        {
            var equatable = new[]
            {
                LawTypes.Boolean, LawTypes.Integer, LawTypes.Decimal, LawTypes.String, LawTypes.Instant,
                LawTypes.Duration, LawTypes.TimeOfDay, LawTypes.TimeOfDayInterval, LawTypes.Distance
            };

            foreach (var (id, expected) in new[] { (OperationIds.Equal, true), (OperationIds.NotEqual, false) })
            {
                OperationEvaluator evaluate = v => Success(BooleanValue.Of(v[0].Equals(v[1]) == expected));

                foreach (var type in equatable)
                {
                    Fixed(table, id, LawTypes.Boolean, evaluate, type, type);
                }

                Rule(table, id, 2, t => SameCurrency(t) != null ? LawTypes.Boolean : null, evaluate);
                Rule(table, id, 2, t => SameReferenceKind(t) ? LawTypes.Boolean : null, evaluate);
            }
        }

        private static void RegisterOrdering(OperationTable table)
        {
            var ordered = new (ScalarType Type, Func<LawValue, decimal> Get)[]
            {
                (LawTypes.Integer, v => Int(v)),
                (LawTypes.Decimal, Dec),
                (LawTypes.Instant, Instant),
                (LawTypes.Duration, Duration),
                (LawTypes.Distance, v => ((DistanceValue)v).Value.Value)
            };

            foreach (var (id, test) in Comparisons)
            {
                foreach (var (type, get) in ordered)
                {
                    Fixed(table, id, LawTypes.Boolean, v => Success(BooleanValue.Of(test(get(v[0]).CompareTo(get(v[1]))))), type, type);
                }

                Rule(table, id, 2, t => SameCurrency(t) != null ? LawTypes.Boolean : null,
                    v => Success(BooleanValue.Of(test(Money(v[0]).Amount.CompareTo(Money(v[1]).Amount)))));
            }
        }

        private static void Fixed(OperationTable table, string operationId, LawType result, OperationEvaluator evaluate, params LawType[] parameters) =>
            table.Register(operationId, new FixedOverload(result, evaluate, parameters));

        private static void Rule(OperationTable table, string operationId, int arity, Func<IReadOnlyList<LawType>, LawType?> rule, OperationEvaluator evaluate) =>
            table.Register(operationId, new RuleOverload(arity, rule, evaluate));

        private static OperationResult Success(LawValue value) => OperationResult.Success(value);

        internal static OperationResult Compute(Func<LawValue> compute)
        {
            try
            {
                return OperationResult.Success(compute());
            }
            catch (DivideByZeroException)
            {
                return OperationResult.Fail(EvaluationFailure.DivisionByZero, "Деление на ноль");
            }
            catch (OverflowException)
            {
                return OperationResult.Fail(EvaluationFailure.Overflow, "Результат вычисления слишком велик");
            }
        }

        private static long Int(LawValue value) => ((IntegerValue)value).Value;

        private static decimal Dec(LawValue value) => ((DecimalValue)value).Value;

        private static decimal Number(LawValue value) => value is IntegerValue integer ? integer.Value : Dec(value);

        private static decimal Instant(LawValue value) => ((InstantValue)value).Seconds;

        private static decimal Duration(LawValue value) => ((DurationValue)value).Seconds;

        private static MoneyValue Money(LawValue value) => (MoneyValue)value;

        private static bool IsNumber(LawType type) => type.Equals(LawTypes.Integer) || type.Equals(LawTypes.Decimal);

        private static LawType? SameCurrency(IReadOnlyList<LawType> operands) =>
            operands[0] is MoneyType left && left.Equals(operands[1]) ? left : null;

        private static bool SameReferenceKind(IReadOnlyList<LawType> operands) =>
            operands[0] is DefinitionReferenceType left && left.Equals(operands[1]);
    }
}
