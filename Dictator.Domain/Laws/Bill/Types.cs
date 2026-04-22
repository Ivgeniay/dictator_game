namespace Dictator.Domain.Laws.Bill
{
    public enum BillLogicOperator
    {
        And,
        Or,
        Not,
    }

    /// <summary>
    /// Тип субъекта
    /// </summary>
    public class SubjectType
    {
        public string Value { get; }
        public SubjectType(string value)
        {
            Value = value;
        }

        public static readonly SubjectType All        = new SubjectType("all");
        public static readonly SubjectType Officials  = new SubjectType("officials");
        public static readonly SubjectType Media      = new SubjectType("media");
        public static readonly SubjectType Oligarchs  = new SubjectType("oligarchs");
        public static readonly SubjectType Military   = new SubjectType("military");
        public static readonly SubjectType Police     = new SubjectType("police");
        public static readonly SubjectType Citizens   = new SubjectType("citizens");
        public static readonly SubjectType Business   = new SubjectType("business");

        public override bool Equals(object obj)
        {
            if (obj is SubjectType other)
                return Value == other.Value;
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value;
        }
    }

    /// <summary>
    /// Тип дейсвий
    /// </summary>
    public class ActionType
    {
        public string Value { get; }
        public ActionType(string value)
        {
            Value = value;
        }
        public static readonly ActionType SpeakOnInternet     = new ActionType("speak_on_internet");
        public static readonly ActionType AssemblePublicly    = new ActionType("assemble_publicly");
        public static readonly ActionType PublishInMedia      = new ActionType("publish_in_media");
        public static readonly ActionType OrganizeProtest     = new ActionType("organize_protest");
        public static readonly ActionType UseMobileDevices    = new ActionType("use_mobile_devices");
        public static readonly ActionType TravelAbroad        = new ActionType("travel_abroad");
        public static readonly ActionType OwnProperty         = new ActionType("own_property");
        public static readonly ActionType OperateBusiness     = new ActionType("operate_business");
        public override bool Equals(object obj)
        {
            if (obj is ActionType other)
                return Value == other.Value;
            return false;
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        public override string ToString()
        {
            return Value;
        }
    }

    /// <summary>
    /// Тип ограничения
    /// </summary>
    public class RestrictionType
    {
        public string Value { get; }
        public int Severity { get; }
 
        public RestrictionType(string value, int severity)
        {
            Value = value;
            Severity = severity;
        }

        public static readonly RestrictionType FullFreedom        = new RestrictionType("full_freedom", 0);
        public static readonly RestrictionType LiftRestriction    = new RestrictionType("lift_restriction", 1);
        public static readonly RestrictionType PartialRestriction = new RestrictionType("partial_restriction", 2);
        public static readonly RestrictionType FullBan            = new RestrictionType("full_ban", 3);

        public bool IsRestrictive => Severity >= 2;

        public override bool Equals(object obj)
        {
            if (obj is RestrictionType other)
                return Value == other.Value;
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value;
        }
    }

    /// <summary>
    /// Тип обстоятельств
    /// </summary>
    public class CircumstanceType
    {
        public string Value { get; }
        public CircumstanceType(string value)
        {
            Value = value;
        }
        public static readonly CircumstanceType Everywhere          = new CircumstanceType("everywhere");
        public static readonly CircumstanceType InPublicPlaces      = new CircumstanceType("in_public_places");
        public static readonly CircumstanceType InTransport         = new CircumstanceType("in_transport");
        public static readonly CircumstanceType InSchools           = new CircumstanceType("in_schools");
        public static readonly CircumstanceType InChurches          = new CircumstanceType("in_churches");
        public static readonly CircumstanceType OnInternet          = new CircumstanceType("on_internet");
        public static readonly CircumstanceType InStateBuildings    = new CircumstanceType("in_state_buildings");
 
        public override bool Equals(object obj)
        {
            if (obj is CircumstanceType other)
                return Value == other.Value;
            return false;
        }
 
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
 
        public override string ToString()
        {
            return Value;
        }
    }


}