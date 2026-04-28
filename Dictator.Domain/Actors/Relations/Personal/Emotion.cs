using Dictator.Domain.Utils;
using Dictator.Domain.Shared;
using System.Collections.Generic;
using Dictator.Domain.Utils.Masks;

namespace Dictator.Domain.Actors.Relations.Personal
{
    /// <summary>
    /// Типобезопасная строка представляющая эмоцию которую личность испытывает к диктатору.
    /// Эмоции не исключают друг друга - страх и уважение могут существовать одновременно,
    /// что типично для условий авторитарного режима.
    /// </summary>
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

        public static readonly IReadOnlyList<Emotion> AllEmotions = new List<Emotion>
        {
            Emotion.Admiration,
            Emotion.Respect,
            Emotion.Trust,
            Emotion.Gratitude,
            Emotion.Hope,
            Emotion.Loyalty,
            Emotion.Fear,
            Emotion.Hatred,
            Emotion.Contempt,
            Emotion.Distrust,
            Emotion.Anger,
            Emotion.Resentment,
            Emotion.Indifference,
            Emotion.Envy,
            Emotion.Pity,
            Emotion.Disappointment
        };
    }

    public static class EmotionMask
    {
        public static DynamicMask<Emotion> CreateDefault()
        {
            return new DynamicMask<Emotion>(Emotion.AllEmotions);
        }
    }
}