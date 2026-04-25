namespace Dictator.Domain.Shared
{
    public static class Constants
    {
        public static class Subjects
        {
            public const string People         = "people";
            public const string Officials      = "officials";
            public const string Media          = "media";
            public const string Oligarchs      = "oligarchs";
            public const string Military       = "military";
            public const string Police         = "police";
            public const string Citizens       = "citizens";
            public const string Business       = "business";
        }

        public static class Restrictions
        {
            public const string FullFreedom        = "full_freedom";
            public const string LiftRestriction    = "lift_restriction";
            public const string PartialRestriction = "partial_restriction";
            public const string FullBan            = "full_ban";
        }

        public static class Circumstances
        {
            public const string Everywhere       = "everywhere";
            public const string InPublicPlaces   = "in_public_places";
            public const string InTransport      = "in_transport";
            public const string InSchools        = "in_schools";
            public const string InChurches       = "in_churches";
            public const string OnInternet       = "on_internet";
            public const string InStateBuildings = "in_state_buildings";
        }

        public static class Actions
        {
            public const string SpeakOnInternet  = "speak_on_internet";
            public const string AssemblePublicly = "assemble_publicly";
            public const string PublishInMedia   = "publish_in_media";
            public const string OrganizeProtest  = "organize_protest";
            public const string UseMobileDevices = "use_mobile_devices";
            public const string TravelAbroad     = "travel_abroad";
            public const string OwnProperty      = "own_property";
            public const string OperateBusiness  = "operate_business";
        }

        public static class Empty
        {
            public const string Value = "empty";
        }

        public static class All
        {
            public const string Value = "all";
        }
    }
}