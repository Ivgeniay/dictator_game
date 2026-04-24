using System;
using System.Collections.Generic;
using System.Linq;
using Dictator.Domain.Laws.Law_;

namespace Dictator.Domain.Laws.Registry
{
    public enum LawRecordStatus
    {
        Active,
        Repealed
    }

    public class LawRecord
    {
        public Guid Id { get; }
        public LawRecordStatus Status { get; private set; }
        public long? RepealedAtTick { get; private set; }
        public int ActiveVersionIndex { get; private set; }
        public int VersionCount => _history.Count;
        public IReadOnlyList<Law> History => _history;
        public Law Current => _history.Last();

        private readonly List<Law> _history;

        public LawRecord(Guid id, Law initialLaw)
        {
            Id = id;
            Status = LawRecordStatus.Active;
            RepealedAtTick = null;
            _history = new List<Law> { initialLaw };
            ActiveVersionIndex = 0;
        }

        public void AddVersion(Law law)
        {
            _history.Add(law);
            ActiveVersionIndex = _history.Count - 1;
        }

        public Law GetVersion(int index)
        {
            if (index < 0 || index >= _history.Count)
                throw new ArgumentOutOfRangeException(nameof(index), $"Версия {index} не существует. Доступно версий: {_history.Count}");
            return _history[index];
        }

        public void Repeal(long currentTick)
        {
            Status = LawRecordStatus.Repealed;
            RepealedAtTick = currentTick;
        }

        public void Restore(int versionIndex)
        {
            if (versionIndex < 0 || versionIndex >= _history.Count)
                throw new ArgumentOutOfRangeException(nameof(versionIndex), $"Версия {versionIndex} не существует. Доступно версий: {_history.Count}");
            ActiveVersionIndex = versionIndex;
            Status = LawRecordStatus.Active;
            RepealedAtTick = null;
        }
    }
}