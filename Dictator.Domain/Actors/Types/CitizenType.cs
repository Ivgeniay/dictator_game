using System.Collections.Generic;
using Dictator.Domain.Utils;
using Dictator.Domain.Utils.Masks;

namespace Dictator.Domain.Actors.Types
{
    /// <summary>
    /// Типобезопасная строка представляющая социальный тип или образ жизни гражданина.
    /// Используется для моделирования того как конкретный закон затрагивает разные
    /// группы населения - запрет курения плох для курильщиков но хорош для некурящих.
    /// На уровне Person хранится как DynamicMask, на уровне населения - как статистика.
    /// </summary>
    public class CitizenType : StringType
    {
        public CitizenType() : base() { }
        public CitizenType(string value) : base(value) { }

        /// <summary>(Вредные привычки) курильщик</summary>
        public static readonly CitizenType Smoker = new CitizenType("smoker");

        /// <summary>(Вредные привычки) употребляющий алкоголь</summary>
        public static readonly CitizenType Drinker = new CitizenType("drinker");

        /// <summary>(Цифровые) активный пользователь интернета</summary>
        public static readonly CitizenType InternetUser = new CitizenType("internet_user");

        /// <summary>(Цифровые) активный пользователь социальных сетей</summary>
        public static readonly CitizenType SocialMediaUser = new CitizenType("social_media_user");

        /// <summary>(Транспорт) владелец автомобиля</summary>
        public static readonly CitizenType Driver = new CitizenType("driver");

        /// <summary>(Вера) религиозный человек</summary>
        public static readonly CitizenType ReligiousPerson = new CitizenType("religious_person");

        /// <summary>(Экономика) владелец малого или среднего бизнеса</summary>
        public static readonly CitizenType BusinessOwner = new CitizenType("business_owner");

        /// <summary>(Образование) студент</summary>
        public static readonly CitizenType Student = new CitizenType("student");

        /// <summary>(Социальные) пенсионер</summary>
        public static readonly CitizenType Pensioner = new CitizenType("pensioner");

        /// <summary>(Труд) наёмный рабочий или служащий</summary>
        public static readonly CitizenType Worker = new CitizenType("worker");

        /// <summary>(Образование) представитель интеллигенции</summary>
        public static readonly CitizenType Intellectual = new CitizenType("intellectual");

        /// <summary>(Политика) политически активный гражданин</summary>
        public static readonly CitizenType PoliticallyActive = new CitizenType("politically_active");

        /// <summary>(Политика) участник протестных акций</summary>
        public static readonly CitizenType Protester = new CitizenType("protester");

        /// <summary>(Труд) безработный</summary>
        public static readonly CitizenType Unemployed = new CitizenType("unemployed");

        /// <summary>(Семья) многодетный родитель</summary>
        public static readonly CitizenType LargeFamily = new CitizenType("large_family");

        /// <summary>(Здоровье) человек с хроническими заболеваниями</summary>
        public static readonly CitizenType ChronicallyIll = new CitizenType("chronically_ill");

        /// <summary>(Культура) представитель национального меньшинства</summary>
        public static readonly CitizenType MinorityGroup = new CitizenType("minority_group");

        /// <summary>(Цифровые) противник цифрового контроля</summary>
        public static readonly CitizenType PrivacyAdvocate = new CitizenType("privacy_advocate");

        /// <summary>(Экономика) получатель государственных субсидий</summary>
        public static readonly CitizenType SubsidyRecipient = new CitizenType("subsidy_recipient");

        /// <summary>(Культура) военнослужащий или ветеран</summary>
        public static readonly CitizenType Veteran = new CitizenType("veteran");

        public static readonly IReadOnlyList<CitizenType> AllTypes = new List<CitizenType>
        {
            Smoker, Drinker, InternetUser, SocialMediaUser, Driver,
            ReligiousPerson, BusinessOwner, Student, Pensioner, Worker,
            Intellectual, PoliticallyActive, Protester, Unemployed, LargeFamily,
            ChronicallyIll, MinorityGroup, PrivacyAdvocate, SubsidyRecipient, Veteran
        };
    }

    /// <summary>
    /// Фабрика для создания DynamicMask с полным набором типов граждан.
    /// </summary>
    public static class CitizenTypeMask
    {
        public static DynamicMask<CitizenType> CreateDefault()
        {
            return new DynamicMask<CitizenType>(CitizenType.AllTypes);
        }
    }
}