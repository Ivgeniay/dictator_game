using System.Collections.Generic;
using Dictator.Domain.Utils;

namespace Dictator.Domain.Shared.LegalTerms
{
    /// <summary>
    /// Типобезопасная строка представляющая институциональный контекст закона.
    /// Отвечает на вопрос "где" или "в рамках чего" применяется утверждение закона.
    /// В отличие от CircumstanceType который описывает физическое место,
    /// InstitutionType описывает государственный или общественный институт.
    /// Например: ограничение численности чиновников "в парламенте" - контекст парламент,
    /// а не просто место нахождения.
    /// </summary>
    public class InstitutionType : StringType
    {
        public InstitutionType() : base() { }
        public InstitutionType(string value) : base(value) { }

        /// <summary>(Законодательная власть) парламент - законодательный орган</summary>
        public static readonly InstitutionType Parliament = new InstitutionType("parliament");

        /// <summary>(Исполнительная власть) кабинет министров</summary>
        public static readonly InstitutionType Cabinet = new InstitutionType("cabinet");

        /// <summary>(Исполнительная власть) администрация главы государства</summary>
        public static readonly InstitutionType Presidency = new InstitutionType("presidency");

        /// <summary>(Судебная власть) суд общей юрисдикции</summary>
        public static readonly InstitutionType Court = new InstitutionType("court");

        /// <summary>(Судебная власть) конституционный суд</summary>
        public static readonly InstitutionType ConstitutionalCourt = new InstitutionType("constitutional_court");

        /// <summary>(Правоохранительные) полицейское ведомство</summary>
        public static readonly InstitutionType PoliceForce = new InstitutionType("police_force");

        /// <summary>(Правоохранительные) прокуратура</summary>
        public static readonly InstitutionType Prosecution = new InstitutionType("prosecution");

        /// <summary>(Военные) вооружённые силы</summary>
        public static readonly InstitutionType Military = new InstitutionType("military");

        /// <summary>(Гражданские) страна в целом - применяется ко всем институтам и гражданам</summary>
        public static readonly InstitutionType Country = new InstitutionType("country");

        /// <summary>(Гражданские) муниципальные органы власти</summary>
        public static readonly InstitutionType Municipality = new InstitutionType("municipality");

        /// <summary>(Экономические) государственные предприятия</summary>
        public static readonly InstitutionType StateEnterprise = new InstitutionType("state_enterprise");

        /// <summary>(Экономические) частный бизнес и предпринимательство</summary>
        public static readonly InstitutionType PrivateBusiness = new InstitutionType("private_business");

        /// <summary>(Образование) государственные образовательные учреждения</summary>
        public static readonly InstitutionType Education = new InstitutionType("education");

        /// <summary>(Здравоохранение) государственная система здравоохранения</summary>
        public static readonly InstitutionType Healthcare = new InstitutionType("healthcare");

        /// <summary>(Медиа) зарегистрированные средства массовой информации</summary>
        public static readonly InstitutionType MediaInstitution = new InstitutionType("media_institution");

        public static readonly IReadOnlyList<InstitutionType> AllTypes = new List<InstitutionType>
        {
            Parliament, Cabinet, Presidency, Court, ConstitutionalCourt,
            PoliceForce, Prosecution, Military, Country, Municipality,
            StateEnterprise, PrivateBusiness, Education, Healthcare, MediaInstitution
        };
    }
}