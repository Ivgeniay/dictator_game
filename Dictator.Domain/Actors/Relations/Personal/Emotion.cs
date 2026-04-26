using Dictator.Domain.Utils;
using Dictator.Domain.Shared;

namespace Dictator.Domain.Actors.Relations.Personal
{
    public class Emotion : StringType
    {
        public Emotion() : base() { }

        public Emotion(string value) : base(value) { }

        /// <summary>
        /// (Позитивная) восхищение
        /// </summary>
        public static readonly Emotion Admiration    = new Emotion("admiration");
        /// <summary>
        /// (Позитивная) уважение
        /// </summary>
        public static readonly Emotion Respect       = new Emotion("respect");
        /// <summary>
        /// (Позитивная) доверие
        /// </summary>
        public static readonly Emotion Trust         = new Emotion("trust");
        /// <summary>
        /// (Позитивная) благодарность
        /// </summary>
        public static readonly Emotion Gratitude     = new Emotion("gratitude");
        /// <summary>
        /// (Позитивная) надежда
        /// </summary>
        public static readonly Emotion Hope          = new Emotion("hope");
        /// <summary>
        /// (Позитивная) преданность
        /// </summary>
        public static readonly Emotion Loyalty       = new Emotion("loyalty");
        /// <summary>
        /// (Негативная) страх
        /// </summary>
        public static readonly Emotion Fear          = new Emotion("fear");
        /// <summary>
        /// (Негативная) ненависть
        /// </summary>
        public static readonly Emotion Hatred        = new Emotion("hatred");
        /// <summary>
        /// (Негативная) презрение
        /// </summary>
        public static readonly Emotion Contempt      = new Emotion("contempt");
        /// <summary>
        /// (Негативная) недоверие
        /// </summary>
        public static readonly Emotion Distrust      = new Emotion("distrust");
        /// <summary>
        /// (Негативная) злость
        /// </summary>
        public static readonly Emotion Anger         = new Emotion("anger");
        /// <summary>
        /// (Негативная) обида
        /// </summary>
        public static readonly Emotion Resentment    = new Emotion("resentment");
        /// <summary>
        /// (Нейтральная/смешанная) безразличие
        /// </summary>
        public static readonly Emotion Indifference  = new Emotion("indifference");
        /// <summary>
        /// (Нейтральная/смешанная) зависть
        /// </summary>
        public static readonly Emotion Envy          = new Emotion("envy");
        /// <summary>
        /// (Нейтральная/смешанная) жалость
        /// </summary>
        public static readonly Emotion Pity          = new Emotion("pity");
        /// <summary>
        /// (Нейтральная/смешанная) разочарование
        /// </summary>
        public static readonly Emotion Disappointment = new Emotion("disappointment");
    }
}