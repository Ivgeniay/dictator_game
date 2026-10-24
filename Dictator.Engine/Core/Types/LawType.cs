using System;
using System.Collections.Generic;
using System.Linq;

namespace Dictator.Engine.Core.Types
{
    public abstract class LawType : IEquatable<LawType>
    {
        public virtual bool IsAssignableTo(LawType target) => Equals(target);

        public abstract bool Equals(LawType? other);

        public override bool Equals(object? obj) => obj is LawType other && Equals(other);

        public abstract override int GetHashCode();

        public static bool operator ==(LawType? left, LawType? right) => left is null ? right is null : left.Equals(right);

        public static bool operator !=(LawType? left, LawType? right) => !(left == right);
    }

    public sealed class ScalarType : LawType
    {
        public ScalarType(string id, ScalarType? parent = null)
        {
            Id = id;
            Parent = parent;
        }

        public string Id { get; }

        public ScalarType? Parent { get; }

        public override bool IsAssignableTo(LawType target)
        {
            for (var current = this; current != null; current = current.Parent)
            {
                if (current.Equals(target))
                {
                    return true;
                }
            }

            return false;
        }

        public override bool Equals(LawType? other) => other is ScalarType scalar && string.Equals(Id, scalar.Id, StringComparison.Ordinal);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Id);

        public override string ToString() => Id;
    }

    public sealed class NeverType : LawType
    {
        public static readonly NeverType Instance = new NeverType();

        private NeverType()
        {
        }

        public override bool IsAssignableTo(LawType target) => true;

        public override bool Equals(LawType? other) => other is NeverType;

        public override int GetHashCode() => typeof(NeverType).GetHashCode();

        public override string ToString() => "core.never";
    }

    public sealed class ListType : LawType
    {
        public ListType(LawType element)
        {
            Element = element;
        }

        public LawType Element { get; }

        public override bool IsAssignableTo(LawType target) => target is ListType list && Element.IsAssignableTo(list.Element);

        public override bool Equals(LawType? other) => other is ListType list && Element.Equals(list.Element);

        public override int GetHashCode() => HashCode.Combine(typeof(ListType), Element);

        public override string ToString() => "core.list<" + Element + ">";
    }

    public sealed class RecordField : IEquatable<RecordField>
    {
        public RecordField(string name, LawType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }

        public LawType Type { get; }

        public bool Equals(RecordField? other) => other != null && string.Equals(Name, other.Name, StringComparison.Ordinal) && Type.Equals(other.Type);

        public override bool Equals(object? obj) => obj is RecordField other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(StringComparer.Ordinal.GetHashCode(Name), Type);

        public override string ToString() => Name + ": " + Type;
    }

    public sealed class RecordType : LawType
    {
        private readonly RecordField[] _fields;

        public RecordType(IEnumerable<RecordField> fields)
        {
            _fields = fields.OrderBy(f => f.Name, StringComparer.Ordinal).ToArray();
        }

        public IReadOnlyList<RecordField> Fields => _fields;

        public bool TryGetField(string name, out LawType type)
        {
            foreach (var field in _fields)
            {
                if (string.Equals(field.Name, name, StringComparison.Ordinal))
                {
                    type = field.Type;
                    return true;
                }
            }

            type = NeverType.Instance;
            return false;
        }

        public override bool IsAssignableTo(LawType target)
        {
            if (!(target is RecordType record) || record._fields.Length != _fields.Length)
            {
                return false;
            }

            for (var i = 0; i < _fields.Length; i++)
            {
                if (!string.Equals(_fields[i].Name, record._fields[i].Name, StringComparison.Ordinal)
                    || !_fields[i].Type.IsAssignableTo(record._fields[i].Type))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(LawType? other) => other is RecordType record && _fields.SequenceEqual(record._fields);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(typeof(RecordType));
            foreach (var field in _fields)
            {
                hash.Add(field);
            }

            return hash.ToHashCode();
        }

        public override string ToString() => "core.record{" + string.Join(", ", _fields.Select(f => f.ToString())) + "}";
    }

    public sealed class DefinitionReferenceType : LawType
    {
        public DefinitionReferenceType(string definitionKind)
        {
            DefinitionKind = definitionKind;
        }

        public string DefinitionKind { get; }

        public override bool Equals(LawType? other) => other is DefinitionReferenceType reference && string.Equals(DefinitionKind, reference.DefinitionKind, StringComparison.Ordinal);

        public override int GetHashCode() => HashCode.Combine(typeof(DefinitionReferenceType), StringComparer.Ordinal.GetHashCode(DefinitionKind));

        public override string ToString() => "core.ref<" + DefinitionKind + ">";
    }

    public sealed class MoneyType : LawType
    {
        public MoneyType(string currencyId)
        {
            CurrencyId = currencyId;
        }

        public string CurrencyId { get; }

        public override bool Equals(LawType? other) => other is MoneyType money && string.Equals(CurrencyId, money.CurrencyId, StringComparison.Ordinal);

        public override int GetHashCode() => HashCode.Combine(typeof(MoneyType), StringComparer.Ordinal.GetHashCode(CurrencyId));

        public override string ToString() => "core.money<" + CurrencyId + ">";
    }

    public static class LawTypes
    {
        public static readonly ScalarType Unit = new ScalarType("core.unit");
        public static readonly ScalarType Boolean = new ScalarType("core.boolean");
        public static readonly ScalarType Integer = new ScalarType("core.integer");
        public static readonly ScalarType Decimal = new ScalarType("core.decimal");
        public static readonly ScalarType String = new ScalarType("core.string");
        public static readonly ScalarType Instant = new ScalarType("core.instant");
        public static readonly ScalarType Duration = new ScalarType("core.duration");
        public static readonly ScalarType TimeOfDay = new ScalarType("core.time_of_day");
        public static readonly ScalarType TimeOfDayInterval = new ScalarType("core.time_of_day_interval");
        public static readonly ScalarType Distance = new ScalarType("core.distance");
        public static readonly ScalarType Position = new ScalarType("core.position");
        public static readonly ScalarType Snapshot = new ScalarType("core.snapshot");
        public static readonly ScalarType Outcome = new ScalarType("core.outcome");
        public static readonly ScalarType Verdict = new ScalarType("core.verdict", Outcome);
        public static readonly ScalarType Sanction = new ScalarType("core.sanction");
        public static readonly NeverType Never = NeverType.Instance;

        public static MoneyType MoneyIn(string currencyId) => new MoneyType(currencyId);

        public static ListType ListOf(LawType element) => new ListType(element);

        public static RecordType RecordOf(params RecordField[] fields) => new RecordType(fields);

        public static DefinitionReferenceType ReferenceTo(string definitionKind) => new DefinitionReferenceType(definitionKind);
    }
}
