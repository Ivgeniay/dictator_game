using System.Collections.Generic;
using System.Linq;
using Dictator.Domain.Shared;

namespace Dictator.Domain.Utils.Masks
{
    public class DynamicMask<T> where T : StringType
    {
        private readonly Dictionary<T, bool> _entries;
        private bool _includesAll;

        public DynamicMask(IEnumerable<T> allValues)
        {
            _entries = allValues.ToDictionary(v => v, v => false);
            _includesAll = false;
        }

        public void Push(T value, bool mask) => _entries[value] = mask;
        public void Push(T value) => _entries[value] = false;
        public bool Pop(T value)
        {
            if (_entries.TryGetValue(value, out bool mask))
            {
                _entries.Remove(value);
                return true;
            }
            return false;
        }


        public void Include(IEnumerable<T> values)
        {
            if (values.Any(e => e.Value == Constants.All.Value))
            {
                IncludeAll();
                return;
            }

            foreach(var e in values)
            {
                Include(e);
            }
        }

        public void Include(T value)
        {
            if (_includesAll) return;

            if (value.Value == Constants.All.Value)
            {
                IncludeAll();
                return;
            }

            if (_entries.ContainsKey(value))
                _entries[value] = true;
        }


        public void Exclude(IEnumerable<T> values)
        {
            if (values.Any(e => e.Value == Constants.All.Value))
            {
                ExcludeAll();
                return;
            }

            foreach(var e in values)
            {
                Exclude(e);
            }
        }
        
        public void Exclude(T value)
        {
            if (value.Value == Constants.All.Value)
            {
                ExcludeAll();
                return;
            }

            if (!_entries.ContainsKey(value)) return;

            if (_includesAll)
            {
                _includesAll = false;
                foreach (var key in _entries.Keys.ToList())
                    _entries[key] = true;
            }
            
            _entries[value] = false;
        }

        public void IncludeAll()
        {
            _includesAll = true;
            
            foreach (var key in _entries.Keys.ToList())
                _entries[key] = true;
        }

        public void ExcludeAll()
        {
            _includesAll = false;
            foreach (var key in _entries.Keys.ToList())
                _entries[key] = false;
        }

        public bool Matches(T value)
        {
            if (_includesAll) return true;

            if (_entries.TryGetValue(value, out bool included))
                return included;

            return false;
        }

        public bool IsEmpty()
        {
            return !_includesAll && _entries.Values.All(v => !v);
        }

        public void Clear()
        {
            ExcludeAll();
        }

        public IReadOnlyCollection<T> GetIncluded()
        {
            return _entries
                .Where(kv => kv.Value)
                .Select(kv => kv.Key)
                .ToList();
        }

        public IReadOnlyCollection<T> GetExcluded()
        {
            return _entries
                .Where(kv => !kv.Value)
                .Select(kv => kv.Key)
                .ToList();
        }
    }
}