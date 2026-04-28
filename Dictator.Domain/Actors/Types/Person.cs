using System;
using System.Collections.Generic;
using Dictator.Domain.Actors.Relations.Personal;

namespace Dictator.Domain.Actors.Types
{
    /// <summary>
    /// Конкретный именованный представитель группы населения.
    /// Моделирует индивида с которым игрок может взаимодействовать напрямую - подкупать, шантажировать, запугивать.
    /// Хранит личные грехи которые могут быть раскрыты в ходе расследования и личное отношение к диктатору.
    /// </summary>
    public class Person : IInvestigatable
    {
        public Guid Id { get; }
        public string Name { get; }
        public Actor Actor { get; }
        private PersonRelation Relation { get; }

        private readonly List<Sin> _sins;
        public IReadOnlyList<Sin> Sins => _sins;

        public Person(Guid id, string name, Actor actor)
        {
            Id = id;
            Name = name;
            Actor = actor;
            Relation = new PersonRelation();
            _sins = new List<Sin>();
        }

        public Person(Guid id, string name, Actor actor, PersonRelation relation, List<Sin> sins)
        {
            Id = id;
            Name = name;
            Actor = actor;
            Relation = relation;
            _sins = sins ?? new List<Sin>();
        }

        public void AddSin(Sin sin)
        {
            if (!_sins.Contains(sin))
                _sins.Add(sin);
        }

        public void AddSins(IEnumerable<Sin> sins)
        {
            foreach (var sin in sins)
                AddSin(sin);
        }

        public void RemoveSin(Sin sin)
        {
            _sins.Remove(sin);
        }

        public bool HasSin(Sin sin)
        {
            return _sins.Contains(sin);
        }

        public IReadOnlyList<Sin> GetExposableSins()
        {
            return _sins;
        }

        public void ApplyEmotion(Emotion emotion) => Relation.ApplyEmotion(emotion);
        public void ApplyRandomEmotion() => Relation.ApplyRandomEmotion();
        public void RemoveEmotion(Emotion emotion) => Relation.RemoveEmotion(emotion);
        public bool HasEmotion(Emotion emotion) => Relation.HasEmotion(emotion);
        public IReadOnlyCollection<Emotion> GetActiveEmotions() => Relation.GetActiveEmotions();
        public void IncreaseLoyalty(sbyte amount) => Relation.IncreaseLoyalty(amount);
        public void DecreaseLoyalty(sbyte amount) => Relation.DecreaseLoyalty(amount);
        public void SetLoyalty(sbyte value) => Relation.SetLoyalty(value);

    }
}