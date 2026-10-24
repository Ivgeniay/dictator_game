using System;
using System.Collections.Generic;
using System.Linq;
using Dictator.Engine.Core.Definitions;
using Dictator.Engine.Core.Types;

namespace Dictator.Engine.Core.Values
{
    public enum Modality
    {
        Permitted,
        Forbidden,
        Obligatory
    }

    public sealed class StatusAssignment
    {
        public const string StatusField = "status";
        public const string AssignedByField = "assigned_by";
        public const string AssignedAtField = "assigned_at";
        public const string StatusKind = DefinitionKinds.Status;

        private readonly Dictionary<string, LawValue> _attributes;

        public StatusAssignment(
            string statusId,
            string assignedBy,
            InstantValue assignedAt,
            InstantValue? expiresAt = null,
            IEnumerable<KeyValuePair<string, LawValue>>? attributes = null)
        {
            StatusId = statusId;
            AssignedBy = assignedBy;
            AssignedAt = assignedAt;
            ExpiresAt = expiresAt;
            _attributes = (attributes ?? Enumerable.Empty<KeyValuePair<string, LawValue>>())
                .ToDictionary(a => a.Key, a => a.Value, StringComparer.Ordinal);
        }

        public static RecordType RecordType { get; } = LawTypes.RecordOf(
            new RecordField(StatusField, LawTypes.ReferenceTo(StatusKind)),
            new RecordField(AssignedByField, LawTypes.String),
            new RecordField(AssignedAtField, LawTypes.Instant));

        public string StatusId { get; }

        public string AssignedBy { get; }

        public InstantValue AssignedAt { get; }

        public InstantValue? ExpiresAt { get; }

        public IReadOnlyDictionary<string, LawValue> Attributes => _attributes;

        public RecordValue ToRecord() => new RecordValue(new[]
        {
            new KeyValuePair<string, LawValue>(StatusField, new DefinitionReferenceValue(StatusKind, StatusId)),
            new KeyValuePair<string, LawValue>(AssignedByField, new StringValue(AssignedBy)),
            new KeyValuePair<string, LawValue>(AssignedAtField, AssignedAt)
        });
    }

    public sealed class SnapshotValue : LawValue
    {
        private readonly StatusAssignment[] _assignments;
        private readonly HashSet<string> _states;

        public SnapshotValue(string subjectId, IEnumerable<StatusAssignment> assignments, IEnumerable<string> states)
        {
            SubjectId = subjectId;
            _assignments = assignments.ToArray();
            _states = new HashSet<string>(states, StringComparer.Ordinal);
        }

        public string SubjectId { get; }

        public IReadOnlyList<StatusAssignment> Assignments => _assignments;

        public IEnumerable<string> States => _states;

        public override LawType Type => LawTypes.Snapshot;

        public bool HasState(string stateId) => _states.Contains(stateId);

        public override bool Equals(LawValue? other) => ReferenceEquals(this, other);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(SubjectId);

        public override string ToString() => "snapshot(" + SubjectId + ")";
    }

    public sealed class SanctionValue : LawValue
    {
        private readonly LawValue[] _arguments;

        public SanctionValue(string sanctionId, IEnumerable<LawValue> arguments)
        {
            SanctionId = sanctionId;
            _arguments = arguments.ToArray();
        }

        public string SanctionId { get; }

        public IReadOnlyList<LawValue> Arguments => _arguments;

        public override LawType Type => LawTypes.Sanction;

        public override bool Equals(LawValue? other) =>
            other is SanctionValue sanction && string.Equals(SanctionId, sanction.SanctionId, StringComparison.Ordinal) && _arguments.SequenceEqual(sanction._arguments);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(SanctionId);

        public override string ToString() => SanctionId + "(" + string.Join(", ", _arguments.Select(a => a.ToString())) + ")";
    }

    public sealed class VerdictValue : LawValue
    {
        private readonly SanctionValue[] _sanctions;

        public VerdictValue(Modality modality, IEnumerable<SanctionValue> sanctions, string normId, LawValue? reason)
        {
            Modality = modality;
            _sanctions = sanctions.ToArray();
            NormId = normId;
            Reason = reason;
        }

        public Modality Modality { get; }

        public IReadOnlyList<SanctionValue> Sanctions => _sanctions;

        public string NormId { get; }

        public LawValue? Reason { get; }

        public override LawType Type => LawTypes.Verdict;

        public override bool Equals(LawValue? other) =>
            other is VerdictValue verdict
            && Modality == verdict.Modality
            && string.Equals(NormId, verdict.NormId, StringComparison.Ordinal)
            && _sanctions.SequenceEqual(verdict._sanctions)
            && Equals(Reason, verdict.Reason);

        public override int GetHashCode() => HashCode.Combine(Modality, StringComparer.Ordinal.GetHashCode(NormId));

        public override string ToString() => Modality + "[" + string.Join(", ", _sanctions.Select(s => s.ToString())) + "] by " + NormId;
    }

    public sealed class NotApplicableValue : LawValue
    {
        public static readonly NotApplicableValue Instance = new NotApplicableValue();

        private NotApplicableValue()
        {
        }

        public override LawType Type => LawTypes.Outcome;

        public override bool Equals(LawValue? other) => other is NotApplicableValue;

        public override int GetHashCode() => typeof(NotApplicableValue).GetHashCode();

        public override string ToString() => "not_applicable";
    }
}
