using Dictator.Domain.Utils;

namespace Dictator.Domain.Shared.LegalTerms
{
    /// <summary>
    /// Типобезопасная строка представляющая действие которое закон регулирует.
    /// Определяет что именно запрещается или разрешается - высказываться в интернете,
    /// собираться публично, владеть имуществом и т.д.
    /// </summary>
    public class ActionType : StringType
    {
        public ActionType() : base() { }
        public ActionType(string value) : base(value) { }
        public static readonly ActionType SpeakOnInternet     = new ActionType(Constants.Actions.SpeakOnInternet);
        public static readonly ActionType AssemblePublicly    = new ActionType(Constants.Actions.AssemblePublicly);
        public static readonly ActionType PublishInMedia      = new ActionType(Constants.Actions.PublishInMedia);
        public static readonly ActionType OrganizeProtest     = new ActionType(Constants.Actions.OrganizeProtest);
        public static readonly ActionType UseMobileDevices    = new ActionType(Constants.Actions.UseMobileDevices);
        public static readonly ActionType TravelAbroad        = new ActionType(Constants.Actions.TravelAbroad);
        public static readonly ActionType OwnProperty         = new ActionType(Constants.Actions.OwnProperty);
        public static readonly ActionType OperateBusiness     = new ActionType(Constants.Actions.OperateBusiness);
    }
}
