using Dictator.Domain.Utils;

namespace Dictator.Domain.Shared.LegalTerms
{
    /// <summary>
    /// Тип обстоятельств
    /// </summary>
    public class CircumstanceType : StringType
    {
        public CircumstanceType() : base() { }
        public CircumstanceType(string value) : base(value) { }
        public static readonly CircumstanceType Everywhere          = new CircumstanceType(Constants.Circumstances.Everywhere);
        public static readonly CircumstanceType InPublicPlaces      = new CircumstanceType(Constants.Circumstances.InPublicPlaces);
        public static readonly CircumstanceType InTransport         = new CircumstanceType(Constants.Circumstances.InTransport);
        public static readonly CircumstanceType InSchools           = new CircumstanceType(Constants.Circumstances.InSchools);
        public static readonly CircumstanceType InChurches          = new CircumstanceType(Constants.Circumstances.InChurches);
        public static readonly CircumstanceType OnInternet          = new CircumstanceType(Constants.Circumstances.OnInternet);
        public static readonly CircumstanceType InStateBuildings    = new CircumstanceType(Constants.Circumstances.InStateBuildings);
    }
    
}
