using System.Collections.Generic;
using System.Linq;
using System;

namespace Dictator.Domain.Laws.Registry
{
    public class RegistryType
    {
        public string Value { get; }
        public RegistryType(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Конституция
        /// </summary>
        public static readonly RegistryType Constitution = new RegistryType("constitution");
        /// <summary>
        /// Кодекс
        /// </summary>
        public static readonly RegistryType Codex        = new RegistryType("codex");
        /// <summary>
        /// Указ
        /// </summary>
        public static readonly RegistryType Decree       = new RegistryType("decree");
        public override bool Equals(object obj)
        {
            if (obj is RegistryType other)
                return Value == other.Value;
            return false;
        }
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;
    }

    public class LawRegistry
    {
        public Guid Id { get; }
        public RegistryType Type { get; }
        public IReadOnlyList<LawRecord> Records => _records;
        private readonly List<LawRecord> _records;
        public LawRegistry(Guid id, RegistryType type)
        {
            Id = id;
            Type = type;
            _records = new List<LawRecord>();
        }
        public void AddRecord(LawRecord record)
        {
            _records.Add(record);
        }
        public IReadOnlyList<LawRecord> GetActive()
        {
            return _records
                .Where(r => r.Status == LawRecordStatus.Active)
                .ToList();
        }
        public LawRecord GetById(Guid id)
        {
            return _records.FirstOrDefault(r => r.Id == id);
        }
        public IReadOnlyList<LawRecord> GetBySubject(SubjectType subject)
        {
            return GetActive()
                .Where(r => r.Current.Statement.Subject.Children
                    .OfType<SubjectNode>()
                    .Any(s => s.Subject.Equals(subject) || s.Subject.Equals(SubjectType.All)))
                .ToList();
        }
        public IReadOnlyList<LawRecord> GetByAction(ActionType action)
        {
            return GetActive()
                .Where(r => r.Current.Statement.Action.Action.Equals(action))
                .ToList();
        }
        public IReadOnlyList<LawRecord> GetByRestriction(RestrictionType restriction)
        {
            return GetActive()
                .Where(r => r.Current.Statement.Restriction.Restriction.Equals(restriction))
                .ToList();
        }
        public IReadOnlyList<LawRecord> GetBySubjectAndAction(SubjectType subject, ActionType action)
        {
            return GetBySubject(subject)
                .Where(r => r.Current.Statement.Action.Action.Equals(action))
                .ToList();
        }
    }

}