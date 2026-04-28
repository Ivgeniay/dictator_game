using Dictator.Domain.Shared.LegalTerms;
using Dictator.Domain.Shared.Metrics;
using System;

namespace Dictator.Domain.Actors.Types.Organizations
{
    /// <summary>
    /// Военная организация - вооружённые силы государства.
    /// Источник угрозы военного переворота при низкой лояльности.
    /// </summary>
    public class MilitaryOrganization : Organization
    {
        public MilitaryOrganization(Guid id, string name, IOwner owner)
            : base(id, name, SubjectType.Military, owner) { }

        public MilitaryOrganization(
            Guid id,
            string name,
            IOwner owner,
            Loyalty loyalty,
            CorruptionLevel corruptionLevel) : base(id, name, SubjectType.Military, owner, loyalty, corruptionLevel) {}
            
    }


}