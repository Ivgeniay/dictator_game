using Dictator.Domain.Actors.Relations.Personal;
using Dictator.Domain.Shared.LegalTerms;
using Dictator.Domain.Shared.Metrics;
using Dictator.Domain.State;
using System.Linq;
using System;

namespace Dictator.Domain.Actors.Types.Organizations
{
    /// <summary>
    /// Медиаорганизация  - телеканал, газета, интернет-издание и т.д.
    /// Может проводить журналистские расследования и публиковать компрометирующие материалы.
    /// </summary>
    public class MediaOrganization : Organization, IInvestigator
    {
        public MediaOrganization(Guid id, string name, IOwner owner) : base(id, name, SubjectType.Media, owner) { }

        public MediaOrganization(
            Guid id,
            string name,
            IOwner owner,
            Loyalty loyalty,
            CorruptionLevel corruptionLevel) : base(id, name,SubjectType.Media, owner, loyalty, corruptionLevel) {}

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