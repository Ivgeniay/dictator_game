using System.Collections.Generic;
using Dictator.Domain.Utils;
using Dictator.Domain.Utils.Masks;

namespace Dictator.Domain.Actors.Relations.Group
{
    /// <summary>
    /// Типобезопасная строка представляющая основание лояльности группы населения к диктатору.
    /// Объясняет по какой причине группа поддерживает режим - от страха до идеологического согласия.
    /// Разные основания реагируют на разные события и законы что позволяет моделировать
    /// сложную и неоднородную поддержку режима внутри одной группы.
    /// </summary>
    public class LoyaltyBasis : StringType
    {
        public LoyaltyBasis() : base() { }
        public LoyaltyBasis(string value) : base(value) { }

        /// <summary>(Принуждение) страх репрессий и наказания</summary>
        public static readonly LoyaltyBasis Fear = new LoyaltyBasis("fear");

        /// <summary>(Убеждение) идеологическое согласие с курсом режима</summary>
        public static readonly LoyaltyBasis Ideology = new LoyaltyBasis("ideology");

        /// <summary>(Материальное) экономическое благополучие при текущем режиме</summary>
        public static readonly LoyaltyBasis EconomicWelfare = new LoyaltyBasis("economic_welfare");

        /// <summary>(Культурное) традиционализм и консерватизм</summary>
        public static readonly LoyaltyBasis Tradition = new LoyaltyBasis("tradition");

        /// <summary>(Культурное) националистические настроения</summary>
        public static readonly LoyaltyBasis Nationalism = new LoyaltyBasis("nationalism");

        /// <summary>(Культурное) религиозное одобрение режима</summary>
        public static readonly LoyaltyBasis Religion = new LoyaltyBasis("religion");

        /// <summary>(Материальное) личная выгода от режима</summary>
        public static readonly LoyaltyBasis PersonalGain = new LoyaltyBasis("personal_gain");

        /// <summary>(Информационное) влияние государственной пропаганды и СМИ</summary>
        public static readonly LoyaltyBasis Propaganda = new LoyaltyBasis("propaganda");

        /// <summary>(Личностное) личная харизма и образ диктатора</summary>
        public static readonly LoyaltyBasis Charisma = new LoyaltyBasis("charisma");

        /// <summary>(Идеологическое) антизападные и антилиберальные настроения</summary>
        public static readonly LoyaltyBasis AntiWestern = new LoyaltyBasis("anti_western");

        /// <summary>(Социальное) запрос на порядок стабильность и предсказуемость</summary>
        public static readonly LoyaltyBasis LawAndOrder = new LoyaltyBasis("law_and_order");

        /// <summary>(Социальное) зависть к элитам которых диктатор якобы усмиряет</summary>
        public static readonly LoyaltyBasis SocialEnvy = new LoyaltyBasis("social_envy");

        /// <summary>(Культурное) ностальгия по великому историческому прошлому</summary>
        public static readonly LoyaltyBasis HistoricalNostalgia = new LoyaltyBasis("historical_nostalgia");

        /// <summary>(Культурное) гордость за военную мощь и достижения армии</summary>
        public static readonly LoyaltyBasis MilitaryPride = new LoyaltyBasis("military_pride");

        /// <summary>(Культурное) консервативные семейные ценности</summary>
        public static readonly LoyaltyBasis FamilyValues = new LoyaltyBasis("family_values");

        /// <summary>(Идеологическое) вера что диктатор борется с коррупцией</summary>
        public static readonly LoyaltyBasis AntiCorruption = new LoyaltyBasis("anti_corruption");

        /// <summary>(Социальное) потребность в защите от внешних угроз</summary>
        public static readonly LoyaltyBasis SecurityNeeds = new LoyaltyBasis("security_needs");

        /// <summary>(Материальное) благодарность за конкретные социальные блага</summary>
        public static readonly LoyaltyBasis Gratitude = new LoyaltyBasis("gratitude");

        /// <summary>(Социальное) пассивное следование большинству без собственной позиции</summary>
        public static readonly LoyaltyBasis Conformism = new LoyaltyBasis("conformism");

        /// <summary>(Социальное) цинизм - все одинаковые этот хотя бы знакомый</summary>
        public static readonly LoyaltyBasis Cynicism = new LoyaltyBasis("cynicism");

        public static readonly IReadOnlyList<LoyaltyBasis> AllBases = new List<LoyaltyBasis>
        {
            Fear, Ideology, EconomicWelfare, Tradition, Nationalism,
            Religion, PersonalGain, Propaganda, Charisma, AntiWestern,
            LawAndOrder, SocialEnvy, HistoricalNostalgia, MilitaryPride, FamilyValues,
            AntiCorruption, SecurityNeeds, Gratitude, Conformism, Cynicism
        };
    }

    public static class LoyaltyBasisMask
    {
        public static DynamicMask<LoyaltyBasis> CreateDefault()
        {
            return new DynamicMask<LoyaltyBasis>(LoyaltyBasis.AllBases);
        }
    }
}