using Dictator.Domain.Actors.Relations.Personal;
using Dictator.Domain.Shared.LegalTerms;
using Dictator.Domain.Shared.Metrics;
using Dictator.Domain.State;
using System;
using System.Linq;

namespace Dictator.Domain.Actors.Types.Organizations
{
    /// <summary>
    /// Некоммерческая организация - правозащитники, общественные объединения.
    /// Может проводить независимые расследования и создавать международное давление.
    /// </summary>
    public class NgoOrganization : Organization, IInvestigator
    {
        public NgoOrganization(Guid id, string name, IOwner owner)
            : base(id, name, SubjectType.Citizens, owner) { }

        public NgoOrganization(
            Guid id,
            string name,
            IOwner owner,
            Loyalty loyalty,
            CorruptionLevel corruptionLevel) : base(id, name, SubjectType.Citizens, owner, loyalty, corruptionLevel) {}
            
        public Investigation Investigate(IInvestigatable target, InvestigationContext context)
        {
            var exposedSins = target.GetExposableSins()
                .Where(_ => RandomS.NextDouble() <= context.SuccessProbability)
                .ToList();

            return new Investigation(
                Guid.NewGuid(),
                this,
                target,
                exposedSins,
                GameState.Instance.CurrentTick);
        }


    }

}