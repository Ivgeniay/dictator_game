using Dictator.Domain.Actors.Relations.Personal;
using Dictator.Domain.Shared.LegalTerms;
using Dictator.Domain.Shared.Metrics;
using Dictator.Domain.State;
using System;
using System.Linq;

namespace Dictator.Domain.Actors.Types.Organizations
{
    /// <summary>
    /// Полицейская организация - силовая структура внутреннего порядка.
    /// Может проводить официальные расследования и собирать компромат.
    /// </summary>
    public class PoliceOrganization : Organization, IInvestigator
    {
        public PoliceOrganization(Guid id, string name, IOwner owner)
            : base(id, name, SubjectType.Police, owner) { }

        public PoliceOrganization(
            Guid id,
            string name,
            IOwner owner,
            Loyalty loyalty,
            CorruptionLevel corruptionLevel) : base(id, name, SubjectType.Police, owner, loyalty, corruptionLevel) {}


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