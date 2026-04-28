using Dictator.Domain.Utils;

namespace Dictator.Domain.Shared.LegalTerms
{
    /// <summary>
    /// Типобезопасная строка представляющая степень ограничения действия законом.
    /// Числовой уровень строгости Severity позволяет программно сравнивать законы
    /// и вычислять направление изменения при внесении поправок.
    /// </summary>
    public class RestrictionType : StringType
    {
        public int Severity { get; protected set; }

        public RestrictionType() : base()
        {
            Severity = 0;
        }
        
        public RestrictionType(string value, int severity) : base(value)
        {
            Severity = severity;
        }

        public static readonly RestrictionType FullFreedom        = new RestrictionType(Constants.Restrictions.FullFreedom, 0);
        public static readonly RestrictionType LiftRestriction    = new RestrictionType(Constants.Restrictions.LiftRestriction, 1);
        public static readonly RestrictionType PartialRestriction = new RestrictionType(Constants.Restrictions.PartialRestriction, 2);
        public static readonly RestrictionType FullBan            = new RestrictionType(Constants.Restrictions.FullBan, 3);

        public bool IsRestrictive => Severity >= 2;
    }
}
