using System.Collections.Generic;
using Dictator.Domain.Shared.Metrics;
using Dictator.Domain.Utils.Masks;

namespace Dictator.Domain.Actors.Relations.Group
{
    public class ActorRelation
    {
        private readonly Loyalty _loyalty;
        private readonly DynamicMask<LoyaltyBasis> _bases;

        public Loyalty Loyalty => _loyalty;

        public ActorRelation()
        {
            _loyalty = new Loyalty();
            _bases = LoyaltyBasisMask.CreateDefault();
        }

        public ActorRelation(Loyalty loyalty, DynamicMask<LoyaltyBasis> bases)
        {
            _loyalty = loyalty;
            _bases = bases;
        }

        public void IncreaseLoyalty(sbyte amount) => _loyalty.Increase(amount);
        public void DecreaseLoyalty(sbyte amount) => _loyalty.Decrease(amount);
        public void SetLoyalty(sbyte value)        => _loyalty.Set(value);
        public LoyaltyGrade GetLoyaltyGrade()      => _loyalty.GetGrade();

        public void AddBasis(LoyaltyBasis basis)    => _bases.Include(basis);
        public void RemoveBasis(LoyaltyBasis basis)  => _bases.Exclude(basis);
        public bool HasBasis(LoyaltyBasis basis)     => _bases.Matches(basis);
        public IReadOnlyCollection<LoyaltyBasis> GetActiveBases() => _bases.GetIncluded();
    }
}