using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dictator.Engine.Core.Primitives;
using Dictator.Engine.Core.Types;

namespace Dictator.Engine.Core.Values
{
    public abstract class LawValue : IEquatable<LawValue>
    {
        public abstract LawType Type { get; }

        public abstract bool Equals(LawValue? other);

        public override bool Equals(object? obj) => obj is LawValue other && Equals(other);

        public abstract override int GetHashCode();
    }

    public sealed class UnitValue : LawValue
    {
        public static readonly UnitValue Instance = new UnitValue();

        private UnitValue()
        {
        }

        public override LawType Type => LawTypes.Unit;

        public override bool Equals(LawValue? other) => other is UnitValue;

        public override int GetHashCode() => typeof(UnitValue).GetHashCode();

        public override string ToString() => "()";
    }

    public sealed class BooleanValue : LawValue
    {
        public static readonly BooleanValue True = new BooleanValue(true);
        public static readonly BooleanValue False = new BooleanValue(false);

        private BooleanValue(bool value)
        {
            Value = value;
        }

        public bool Value { get; }

        public override LawType Type => LawTypes.Boolean;

        public static BooleanValue Of(bool value) => value ? True : False;

        public override bool Equals(LawValue? other) => other is BooleanValue boolean && Value == boolean.Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value ? "true" : "false";
    }

    public sealed class IntegerValue : LawValue
    {
        public IntegerValue(long value)
        {
            Value = value;
        }

        public long Value { get; }

        public override LawType Type => LawTypes.Integer;

        public override bool Equals(LawValue? other) => other is IntegerValue integer && Value == integer.Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    }

    public sealed class DecimalValue : LawValue
    {
        public DecimalValue(decimal value)
        {
            Value = value;
        }

        public decimal Value { get; }

        public override LawType Type => LawTypes.Decimal;

        public override bool Equals(LawValue? other) => other is DecimalValue number && Value == number.Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    }

    public sealed class StringValue : LawValue
    {
        public StringValue(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public override LawType Type => LawTypes.String;

        public override bool Equals(LawValue? other) => other is StringValue text && string.Equals(Value, text.Value, StringComparison.Ordinal);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

        public override string ToString() => "\"" + Value + "\"";
    }

    public sealed class InstantValue : LawValue
    {
        public InstantValue(decimal seconds)
        {
            Seconds = seconds;
        }

        public decimal Seconds { get; }

        public override LawType Type => LawTypes.Instant;

        public override bool Equals(LawValue? other) => other is InstantValue instant && Seconds == instant.Seconds;

        public override int GetHashCode() => Seconds.GetHashCode();

        public override string ToString() => "@" + Seconds.ToString(CultureInfo.InvariantCulture) + "s";
    }

    public sealed class DurationValue : LawValue
    {
        public DurationValue(decimal seconds)
        {
            Seconds = seconds;
        }

        public decimal Seconds { get; }

        public override LawType Type => LawTypes.Duration;

        public override bool Equals(LawValue? other) => other is DurationValue duration && Seconds == duration.Seconds;

        public override int GetHashCode() => Seconds.GetHashCode();

        public override string ToString() => Seconds.ToString(CultureInfo.InvariantCulture) + "s";
    }

    public sealed class TimeOfDayValue : LawValue
    {
        public const decimal SecondsPerDay = 86400m;

        public TimeOfDayValue(decimal seconds)
        {
            Seconds = Normalize(seconds);
        }

        public decimal Seconds { get; }

        public override LawType Type => LawTypes.TimeOfDay;

        public static TimeOfDayValue FromClock(int hours, int minutes, int seconds = 0) =>
            new TimeOfDayValue(hours * 3600m + minutes * 60m + seconds);

        public TimeOfDayValue Shift(decimal seconds) => new TimeOfDayValue(Seconds + seconds);

        public override bool Equals(LawValue? other) => other is TimeOfDayValue time && Seconds == time.Seconds;

        public override int GetHashCode() => Seconds.GetHashCode();

        public override string ToString()
        {
            var total = (long)decimal.Floor(Seconds);
            return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}", total / 3600, total / 60 % 60, total % 60);
        }

        private static decimal Normalize(decimal seconds)
        {
            var remainder = seconds % SecondsPerDay;
            return remainder < 0 ? remainder + SecondsPerDay : remainder;
        }
    }

    public sealed class TimeOfDayIntervalValue : LawValue
    {
        public TimeOfDayIntervalValue(TimeOfDayValue start, TimeOfDayValue end)
        {
            Start = start;
            End = end;
        }

        public TimeOfDayValue Start { get; }

        public TimeOfDayValue End { get; }

        public bool IsEmpty => Start.Seconds == End.Seconds;

        public bool CrossesMidnight => Start.Seconds > End.Seconds;

        public override LawType Type => LawTypes.TimeOfDayInterval;

        public bool Contains(TimeOfDayValue time) =>
            Segments().Any(segment => segment.From <= time.Seconds && time.Seconds < segment.To);

        public bool Overlaps(TimeOfDayIntervalValue other) =>
            Segments().Any(left => other.Segments().Any(right => left.From < right.To && right.From < left.To));

        public override bool Equals(LawValue? other) => other is TimeOfDayIntervalValue interval && Start.Equals(interval.Start) && End.Equals(interval.End);

        public override int GetHashCode() => HashCode.Combine(Start, End);

        public override string ToString() => Start + "–" + End;

        private IEnumerable<(decimal From, decimal To)> Segments()
        {
            if (IsEmpty)
            {
                yield break;
            }

            if (CrossesMidnight)
            {
                yield return (Start.Seconds, TimeOfDayValue.SecondsPerDay);
                yield return (0m, End.Seconds);
            }
            else
            {
                yield return (Start.Seconds, End.Seconds);
            }
        }
    }

    public sealed class DistanceValue : LawValue
    {
        public DistanceValue(Meter value)
        {
            Value = value;
        }

        public Meter Value { get; }

        public override LawType Type => LawTypes.Distance;

        public override bool Equals(LawValue? other) => other is DistanceValue distance && Value == distance.Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();
    }

    public sealed class MoneyValue : LawValue
    {
        private readonly MoneyType _type;

        public MoneyValue(decimal amount, string currencyId)
        {
            Amount = amount;
            _type = LawTypes.MoneyIn(currencyId);
        }

        public decimal Amount { get; }

        public string CurrencyId => _type.CurrencyId;

        public override LawType Type => _type;

        public MoneyValue WithAmount(decimal amount) => new MoneyValue(amount, CurrencyId);

        public override bool Equals(LawValue? other) => other is MoneyValue money && Amount == money.Amount && _type.Equals(money._type);

        public override int GetHashCode() => HashCode.Combine(Amount, _type);

        public override string ToString() => Amount.ToString(CultureInfo.InvariantCulture) + " " + CurrencyId;
    }

    public sealed class ListValue : LawValue
    {
        private readonly LawValue[] _items;
        private readonly ListType _type;

        public ListValue(LawType elementType, IEnumerable<LawValue> items)
        {
            _type = LawTypes.ListOf(elementType);
            _items = items.ToArray();
        }

        public LawType ElementType => _type.Element;

        public IReadOnlyList<LawValue> Items => _items;

        public override LawType Type => _type;

        public override bool Equals(LawValue? other) => other is ListValue list && _type.Equals(list._type) && _items.SequenceEqual(list._items);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(_type);
            foreach (var item in _items)
            {
                hash.Add(item);
            }

            return hash.ToHashCode();
        }

        public override string ToString() => "[" + string.Join(", ", _items.Select(i => i.ToString())) + "]";
    }

    public sealed class RecordValue : LawValue
    {
        private readonly Dictionary<string, LawValue> _fields;
        private readonly RecordType _type;

        public RecordValue(IEnumerable<KeyValuePair<string, LawValue>> fields)
        {
            _fields = fields.ToDictionary(f => f.Key, f => f.Value, StringComparer.Ordinal);
            _type = new RecordType(_fields.Select(f => new RecordField(f.Key, f.Value.Type)));
        }

        public IReadOnlyDictionary<string, LawValue> Fields => _fields;

        public override LawType Type => _type;

        public bool TryGetField(string name, out LawValue value)
        {
            if (_fields.TryGetValue(name, out var found))
            {
                value = found;
                return true;
            }

            value = BooleanValue.False;
            return false;
        }

        public override bool Equals(LawValue? other) =>
            other is RecordValue record
            && _fields.Count == record._fields.Count
            && _fields.All(f => record._fields.TryGetValue(f.Key, out var value) && f.Value.Equals(value));

        public override int GetHashCode() => _type.GetHashCode();

        public override string ToString() =>
            "{" + string.Join(", ", _type.Fields.Select(f => f.Name + ": " + _fields[f.Name])) + "}";
    }

    public sealed class DefinitionReferenceValue : LawValue
    {
        private readonly DefinitionReferenceType _type;

        public DefinitionReferenceValue(string definitionKind, string definitionId)
        {
            _type = LawTypes.ReferenceTo(definitionKind);
            DefinitionId = definitionId;
        }

        public string DefinitionKind => _type.DefinitionKind;

        public string DefinitionId { get; }

        public override LawType Type => _type;

        public override bool Equals(LawValue? other) =>
            other is DefinitionReferenceValue reference && _type.Equals(reference._type) && string.Equals(DefinitionId, reference.DefinitionId, StringComparison.Ordinal);

        public override int GetHashCode() => HashCode.Combine(_type, StringComparer.Ordinal.GetHashCode(DefinitionId));

        public override string ToString() => "&" + DefinitionId;
    }

    public sealed class EvaluationFailure
    {
        public const string DivisionByZero = "core.division_by_zero";
        public const string Overflow = "core.overflow";

        public EvaluationFailure(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public string Code { get; }

        public string Message { get; }

        public override string ToString() => Code + ": " + Message;
    }

    public readonly struct OperationResult
    {
        private OperationResult(LawValue? value, EvaluationFailure? failure)
        {
            Value = value;
            Failure = failure;
        }

        public LawValue? Value { get; }

        public EvaluationFailure? Failure { get; }

        public bool IsSuccess => Failure == null;

        public static OperationResult Success(LawValue value) => new OperationResult(value, null);

        public static OperationResult Fail(string code, string message) => new OperationResult(null, new EvaluationFailure(code, message));
    }
}
