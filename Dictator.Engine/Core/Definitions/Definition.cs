using System;
using System.Collections.Generic;
using System.Linq;
using Dictator.Engine.Core.Ast;
using Dictator.Engine.Core.Types;
using Dictator.Engine.Core.Values;

namespace Dictator.Engine.Core.Definitions
{
    public static class DefinitionKinds
    {
        public const string Status = "status";
        public const string State = "state";
        public const string Alias = "alias";
        public const string Organ = "organ";
        public const string Currency = "currency";
        public const string Sanction = "sanction";
        public const string Norm = "norm";
    }

    [Flags]
    public enum SubjectKinds
    {
        None = 0,
        Person = 1,
        Organization = 2,
        Any = Person | Organization
    }

    public sealed class TypedParameter : IEquatable<TypedParameter>
    {
        public TypedParameter(string name, LawType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }

        public LawType Type { get; }

        public bool Equals(TypedParameter? other) => other != null && string.Equals(Name, other.Name, StringComparison.Ordinal) && Type.Equals(other.Type);

        public override bool Equals(object? obj) => obj is TypedParameter other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(StringComparer.Ordinal.GetHashCode(Name), Type);

        public override string ToString() => Name + ": " + Type;
    }

    public abstract class Definition
    {
        protected Definition(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }

        public string Name { get; }

        public abstract string Kind { get; }

        public IEnumerable<string> Dependencies => CollectDependencies().Distinct(StringComparer.Ordinal);

        public virtual bool HasSameSignature(Definition other) => string.Equals(Kind, other.Kind, StringComparison.Ordinal);

        protected abstract IEnumerable<string> CollectDependencies();

        public override string ToString() => Kind + ":" + Id;
    }

    public sealed class StatusDefinition : Definition
    {
        public StatusDefinition(string id, string name, SubjectKinds applicability, string? parentId = null)
            : base(id, name)
        {
            Applicability = applicability;
            ParentId = parentId;
        }

        public SubjectKinds Applicability { get; }

        public string? ParentId { get; }

        public override string Kind => DefinitionKinds.Status;

        public override bool HasSameSignature(Definition other) =>
            other is StatusDefinition status && Applicability == status.Applicability && string.Equals(ParentId, status.ParentId, StringComparison.Ordinal);

        protected override IEnumerable<string> CollectDependencies() =>
            ParentId == null ? Enumerable.Empty<string>() : new[] { ParentId };
    }

    public sealed class StateDefinition : Definition
    {
        public StateDefinition(string id, string name)
            : base(id, name)
        {
        }

        public override string Kind => DefinitionKinds.State;

        protected override IEnumerable<string> CollectDependencies() => Enumerable.Empty<string>();
    }

    public sealed class AliasDefinition : Definition
    {
        public AliasDefinition(string id, string name, LawValue value)
            : base(id, name)
        {
            Value = value;
        }

        public LawValue Value { get; }

        public LawType Type => Value.Type;

        public override string Kind => DefinitionKinds.Alias;

        public override bool HasSameSignature(Definition other) => other is AliasDefinition alias && Type.Equals(alias.Type);

        protected override IEnumerable<string> CollectDependencies() => DependencyCollector.FromValue(Value);
    }

    public sealed class OrganDefinition : Definition
    {
        public OrganDefinition(string id, string name)
            : base(id, name)
        {
        }

        public override string Kind => DefinitionKinds.Organ;

        protected override IEnumerable<string> CollectDependencies() => Enumerable.Empty<string>();
    }

    public sealed class CurrencyDefinition : Definition
    {
        public CurrencyDefinition(string id, string name)
            : base(id, name)
        {
        }

        public override string Kind => DefinitionKinds.Currency;

        protected override IEnumerable<string> CollectDependencies() => Enumerable.Empty<string>();
    }

    public sealed class SanctionDefinition : Definition
    {
        private readonly TypedParameter[] _parameters;

        public SanctionDefinition(string id, string name, IEnumerable<TypedParameter> parameters)
            : base(id, name)
        {
            _parameters = parameters.ToArray();
        }

        public IReadOnlyList<TypedParameter> Parameters => _parameters;

        public override string Kind => DefinitionKinds.Sanction;

        public override bool HasSameSignature(Definition other) => other is SanctionDefinition sanction && _parameters.SequenceEqual(sanction._parameters);

        protected override IEnumerable<string> CollectDependencies() => _parameters.SelectMany(p => DependencyCollector.FromType(p.Type));
    }

    public sealed class NormDefinition : Definition
    {
        private readonly TypedParameter[] _parameters;

        public NormDefinition(string id, string name, IEnumerable<TypedParameter> parameters, LawNode body)
            : base(id, name)
        {
            _parameters = parameters.ToArray();
            Body = body;
        }

        public IReadOnlyList<TypedParameter> Parameters => _parameters;

        public LawNode Body { get; }

        public LawType ResultType => LawTypes.Outcome;

        public override string Kind => DefinitionKinds.Norm;

        public override bool HasSameSignature(Definition other) => other is NormDefinition norm && _parameters.SequenceEqual(norm._parameters);

        protected override IEnumerable<string> CollectDependencies() =>
            _parameters.SelectMany(p => DependencyCollector.FromType(p.Type)).Concat(DependencyCollector.FromNode(Body));
    }

    public sealed class Act
    {
        private readonly Definition[] _definitions;

        public Act(string id, string name, string? parentId, IEnumerable<Definition> definitions)
        {
            Id = id;
            Name = name;
            ParentId = parentId;
            _definitions = definitions.ToArray();
        }

        public string Id { get; }

        public string Name { get; }

        public string? ParentId { get; }

        public bool IsRoot => ParentId == null;

        public IReadOnlyList<Definition> Definitions => _definitions;

        public override string ToString() => "act:" + Id;
    }

    public static class DependencyCollector
    {
        public static IEnumerable<string> FromType(LawType type)
        {
            switch (type)
            {
                case MoneyType money:
                    return new[] { money.CurrencyId };
                case ListType list:
                    return FromType(list.Element);
                case RecordType record:
                    return record.Fields.SelectMany(f => FromType(f.Type));
                default:
                    return Enumerable.Empty<string>();
            }
        }

        public static IEnumerable<string> FromValue(LawValue value)
        {
            switch (value)
            {
                case MoneyValue money:
                    return new[] { money.CurrencyId };
                case DefinitionReferenceValue reference:
                    return new[] { reference.DefinitionId };
                case ListValue list:
                    return FromType(list.ElementType).Concat(list.Items.SelectMany(FromValue));
                case RecordValue record:
                    return record.Fields.Values.SelectMany(FromValue);
                case SanctionValue sanction:
                    return new[] { sanction.SanctionId }.Concat(sanction.Arguments.SelectMany(FromValue));
                default:
                    return Enumerable.Empty<string>();
            }
        }

        public static IEnumerable<string> FromNode(LawNode node) =>
            Direct(node).Concat(node.Children.SelectMany(child => FromNode(child.Node)));

        private static IEnumerable<string> Direct(LawNode node)
        {
            switch (node)
            {
                case LiteralNode literal:
                    return FromValue(literal.Value);
                case DefinitionReferenceNode reference:
                    return new[] { reference.DefinitionId };
                case HasStatusNode hasStatus:
                    return new[] { hasStatus.StatusId };
                case StatusRecordsNode records:
                    return new[] { records.StatusId };
                case StateNode state:
                    return new[] { state.StateId };
                case SanctionNode sanction:
                    return new[] { sanction.SanctionId };
                case CallNode call:
                    return new[] { call.TargetId };
                default:
                    return Enumerable.Empty<string>();
            }
        }
    }
}
