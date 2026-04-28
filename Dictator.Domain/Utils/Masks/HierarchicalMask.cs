using System.Collections.Generic;
using System;

namespace Dictator.Domain.Utils.Masks
{
    /// <summary>
    /// Направление вхождения в иерархическую маску.
    /// Down - закон или эффект распространяется на всех кто ниже или равен целевому уровню.
    /// Up - распространяется на всех кто выше или равен.
    /// </summary>
    public enum DirectionHierarchicalMask
    {
        Up,
        Down
    }


    /// <summary>
    /// Иерархическая маска для типизированных строковых значений.
    /// Определяет вхождение субъекта в действие закона на основе его позиции в иерархии.
    /// Направление вхождения задаётся при создании: Down - закон распространяется на всех кто ниже или равен целевому уровню, Up - на всех кто выше или равен.
    /// Иерархия может изменяться динамически в процессе игры.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HierarchicalMask<T> where T : StringType
    {
        private readonly List<T> _hierarchy;
        private readonly DirectionHierarchicalMask _direction;

        public HierarchicalMask(
            IEnumerable<T> allValues, 
            DirectionHierarchicalMask direction = DirectionHierarchicalMask.Up
            )
        {
            _hierarchy = new List<T>(allValues);
            _direction = direction;
        }

        public void Insert(T value, int index)
        {
            if (_hierarchy.Contains(value)) return;
            index = Math.Max(0, Math.Min(index, _hierarchy.Count));
            _hierarchy.Insert(index, value);
        }

        public void Remove(T value)
        {
            _hierarchy.Remove(value);
        }

        public void MoveUp(T value)
        {
            int index = _hierarchy.IndexOf(value);
            if (index <= 0) return;
            _hierarchy.RemoveAt(index);
            _hierarchy.Insert(index - 1, value);
        }

        public void MoveDown(T value)
        {
            int index = _hierarchy.IndexOf(value);
            if (index < 0 || index >= _hierarchy.Count - 1) return;
            _hierarchy.RemoveAt(index);
            _hierarchy.Insert(index + 1, value);
        }

        public void MoveTo(T value, int index)
        {
            int current = _hierarchy.IndexOf(value);
            if (current < 0) return;
            _hierarchy.RemoveAt(current);
            index = Math.Max(0, Math.Min(index, _hierarchy.Count));
            _hierarchy.Insert(index, value);
        }

        public int GetLevel(T value)
        {
            return _hierarchy.IndexOf(value);
        }

        public bool Matches(T subject, T target)
        {
            int targetLevel  = _hierarchy.IndexOf(target);
            if (targetLevel < 0) return false;

            return Matches(subject, targetLevel);
        }

        public bool Matches(T subject, int target)
        {
            int subjectLevel = _hierarchy.IndexOf(subject);
            if (subjectLevel < 0) return false;

            return _direction == DirectionHierarchicalMask.Down
                ? subjectLevel <= target
                : subjectLevel >= target;
        }

        public IReadOnlyList<T> GetAll()
        {
            return _hierarchy;
        }
    }
}