using Dictator.Domain.Shared.LegalTerms;
using Dictator.Domain.Shared.Metrics;
using System;

namespace Dictator.Domain.Actors.Types.Organizations
{
    /// <summary>
    /// Бизнес-организация - компания олигарха или крупного предпринимателя.
    /// Источник финансирования и экономического влияния.
    /// </summary>
    public class BusinessOrganization : Organization
    {
        public BusinessOrganization(Guid id, string name, IOwner owner)
            : base(id, name, SubjectType.Business, owner) { }

        public BusinessOrganization(
            Guid id,
            string name,
            IOwner owner,
            Loyalty loyalty,
            CorruptionLevel corruptionLevel) : base(id, name, SubjectType.Business, owner, loyalty, corruptionLevel)
        {
            
        }

    }

}