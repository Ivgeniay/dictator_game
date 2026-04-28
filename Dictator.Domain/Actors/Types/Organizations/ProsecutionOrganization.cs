using Dictator.Domain.Actors.Relations.Personal;
using Dictator.Domain.Shared.LegalTerms;
using Dictator.Domain.Shared.Metrics;
using Dictator.Domain.State;
using System;
using System.Linq;

namespace Dictator.Domain.Actors.Types.Organizations
{
    /// <summary>
    /// Прокуратура - надзорный и обвинительный орган.
    /// Может возбуждать уголовные дела и при наличии политической возможности - против самого диктатора.
    /// </summary>
    public class ProsecutionOrganization : Organization, IInvestigator
    {
        public ProsecutionOrganization(Guid id, string name, IOwner owner)
            : base(id, name, SubjectType.Officials, owner) { }

        public ProsecutionOrganization(
            Guid id,
            string name,
            IOwner owner,
            Loyalty loyalty,
            CorruptionLevel corruptionLevel) : base(id, name, SubjectType.Officials, owner, loyalty, corruptionLevel) {}


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