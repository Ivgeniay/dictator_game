using Dictator.Domain.Actors.Relations.Group;
using Dictator.Domain.Shared.LegalTerms;
using Dictator.Domain.Shared.Metrics;
using System;
using System.Collections.Generic;

namespace Dictator.Domain.Actors.Types
{
    /// <summary>
    /// Группа населения - анонимный коллектив объединённый общим субъектным типом.
    /// Моделирует давление на игрока со стороны социальных групп: народа, чиновников, бизнеса и других.
    /// Конкретные представители группы выражены через Person.
    /// </summary>
    public class Actor
    {
        public Guid Id { get; }
        public SubjectType Subject { get; }
        private ActorRelation Relation { get; }

        public Actor(Guid id, SubjectType subject)
        {
            Id = id;
            Subject = subject;
            Relation = new ActorRelation();
        }

        public Actor(Guid id, SubjectType subject, ActorRelation relation)
        {
            Id = id;
            Subject = subject;
            Relation = relation;
        }

        public void IncreaseLoyalty(sbyte amount) => Relation.IncreaseLoyalty(amount);
        public void DecreaseLoyalty(sbyte amount) => Relation.DecreaseLoyalty(amount);
        public void SetLoyalty(sbyte value) => Relation.SetLoyalty(value);
        public LoyaltyGrade GetLoyaltyGrade() => Relation.GetLoyaltyGrade();

        public void AddBasis(LoyaltyBasis basis) => Relation.AddBasis(basis);
        public void RemoveBasis(LoyaltyBasis basis) => Relation.RemoveBasis(basis);
        public bool HasBasis(LoyaltyBasis basis) => Relation.HasBasis(basis);
        public IReadOnlyCollection<LoyaltyBasis> GetActiveBases() => Relation.GetActiveBases();
    }
}