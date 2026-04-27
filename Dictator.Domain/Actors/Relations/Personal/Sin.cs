using System;
using System.Collections.Generic;
using System.Linq;
using Dictator.Domain.Utils;

namespace Dictator.Domain.Actors.Relations.Personal
{
    public class Sin : StringType
    {
        public Sin() : base() { }

        public Sin(string value) : base(value) { }

        /// <summary>
        /// (Финансовые) взяточничество
        /// </summary>
        public static readonly Sin Bribery = new Sin("bribery");

        /// <summary>
        /// (Финансовые) растрата государственных средств
        /// </summary>
        public static readonly Sin Embezzlement = new Sin("embezzlement");

        /// <summary>
        /// (Финансовые) отмывание денег
        /// </summary>
        public static readonly Sin MoneyLaundering = new Sin("money_laundering");

        /// <summary>
        /// (Финансовые) уклонение от уплаты налогов
        /// </summary>
        public static readonly Sin TaxEvasion = new Sin("tax_evasion");

        /// <summary>
        /// (Финансовые) системная коррупция
        /// </summary>
        public static readonly Sin Corruption = new Sin("corruption");



        /// <summary>
        /// (Личные) измена супругу
        /// </summary>
        public static readonly Sin Adultery = new Sin("adultery");

        /// <summary>
        /// (Личные) тайная семья на стороне
        /// </summary>
        public static readonly Sin SecretFamily = new Sin("secret_family");

        /// <summary>
        /// (Личные) незаконная любовная связь
        /// </summary>
        public static readonly Sin IllegalAffair = new Sin("illegal_affair");

        /// <summary>
        /// (Личные) употребление наркотиков
        /// </summary>
        public static readonly Sin DrugUse = new Sin("drug_use");

        /// <summary>
        /// (Личные) алкоголизм
        /// </summary>
        public static readonly Sin Alcoholism = new Sin("alcoholism");

        /// <summary>
        /// (Личные) пристрастие к азартным играм
        /// </summary>
        public static readonly Sin Gambling = new Sin("gambling");




        /// <summary>
        /// (Личные/социальные) гомосексуальная связь
        /// </summary>
        public static readonly Sin HomosexualAffair = new Sin("homosexual_affair");

        /// <summary>
        /// (Личные/социальные) использование услуг проституток
        /// </summary>
        public static readonly Sin Prostitution = new Sin("prostitution");

        /// <summary>
        /// (Личные/социальные) садизм
        /// </summary>
        public static readonly Sin Sadism = new Sin("sadism");

        /// <summary>
        /// (Личные/социальные) участие в тайных культах
        /// </summary>
        public static readonly Sin SecretCult = new Sin("secret_cult");

        /// <summary>
        /// (Личные/социальные) оккультизм
        /// </summary>
        public static readonly Sin Occultism = new Sin("occultism");




        /// <summary>
        /// (Криминальные) убийство
        /// </summary>
        public static readonly Sin Murder = new Sin("murder");

        /// <summary>
        /// (Криминальные) организация заказного убийства
        /// </summary>
        public static readonly Sin OrderedKilling = new Sin("ordered_killing");

        /// <summary>
        /// (Криминальные) физическое или психологическое насилие
        /// </summary>
        public static readonly Sin Abuse = new Sin("abuse");

        /// <summary>
        /// (Криминальные) педофилия
        /// </summary>
        public static readonly Sin Pedophilia = new Sin("pedophilia");

        /// <summary>
        /// (Криминальные) шантаж и вымогательство
        /// </summary>
        public static readonly Sin Blackmail = new Sin("blackmail");

        /// <summary>
        /// (Криминальные) незаконное хранение оружия
        /// </summary>
        public static readonly Sin IllegalWeapons = new Sin("illegal_weapons");




        /// <summary>
        /// (Торговля и эксплуатация) торговля людьми
        /// </summary>
        public static readonly Sin HumanTrafficking = new Sin("human_trafficking");

        /// <summary>
        /// (Торговля и эксплуатация) рабовладение
        /// </summary>
        public static readonly Sin Slavery = new Sin("slavery");

        /// <summary>
        /// (Торговля и эксплуатация) принудительный труд
        /// </summary>
        public static readonly Sin ForcedLabor = new Sin("forced_labor");

        /// <summary>
        /// (Торговля и эксплуатация) торговля органами
        /// </summary>
        public static readonly Sin OrganTrafficking = new Sin("organ_trafficking");





        /// <summary>
        /// (Экономические преступления) разворовывание бюджета через подставные фирмы
        /// </summary>
        public static readonly Sin BudgetEmbezzlement = new Sin("budget_embezzlement");

        /// <summary>
        /// (Экономические преступления) производство контрафактной продукции
        /// </summary>
        public static readonly Sin CounterfeitGoods = new Sin("counterfeit_goods");

        /// <summary>
        /// (Экономические преступления) производство наркотиков
        /// </summary>
        public static readonly Sin DrugProduction = new Sin("drug_production");

        /// <summary>
        /// (Экономические преступления) незаконная монополия
        /// </summary>
        public static readonly Sin IllegalMonopoly = new Sin("illegal_monopoly");

        /// <summary>
        /// (Экономические преступления) мошенничество с государственными контрактами
        /// </summary>
        public static readonly Sin StateContractFraud = new Sin("state_contract_fraud");




        /// <summary>
        /// (Политические) контакты с иностранными агентами
        /// </summary>
        public static readonly Sin TreasonContact = new Sin("treason_contact");

        /// <summary>
        /// (Политические) фальсификация выборов
        /// </summary>
        public static readonly Sin ElectionFraud = new Sin("election_fraud");

        /// <summary>
        /// (Политические) незаконная слежка за гражданами
        /// </summary>
        public static readonly Sin IllegalSurveillance = new Sin("illegal_surveillance");

        /// <summary>
        /// (Политические) злоупотребление служебным положением
        /// </summary>
        public static readonly Sin PowerAbuse = new Sin("power_abuse");




        /// <summary>
        /// (Особо тяжкие) военные преступления
        /// </summary>
        public static readonly Sin WarCrimes = new Sin("war_crimes");

        /// <summary>
        /// (Особо тяжкие) применение химического оружия
        /// </summary>
        public static readonly Sin ChemicalWeaponsUse = new Sin("chemical_weapons_use");

        /// <summary>
        /// (Особо тяжкие) финансирование терроризма
        /// </summary>
        public static readonly Sin TerrorismFunding = new Sin("terrorism_funding");

        /// <summary>
        /// (Особо тяжкие) насилие над детьми
        /// </summary>
        public static readonly Sin ChildAbuse = new Sin("child_abuse");

        /// <summary>
        /// (Особо тяжкие) каннибализм
        /// </summary>
        public static readonly Sin Cannibalism = new Sin("cannibalism");

        public static readonly IReadOnlyList<Sin> AllSins = new List<Sin>
        {
            Sin.Bribery, 
            Sin.Embezzlement, 
            Sin.MoneyLaundering, 
            Sin.TaxEvasion, 
            Sin.Corruption,
            Sin.Adultery, 
            Sin.SecretFamily, 
            Sin.IllegalAffair, 
            Sin.DrugUse, 
            Sin.Alcoholism, 
            Sin.Gambling,
            Sin.HomosexualAffair, 
            Sin.Prostitution, 
            Sin.Sadism, 
            Sin.SecretCult, 
            Sin.Occultism,
            Sin.Murder, 
            Sin.OrderedKilling, 
            Sin.Abuse, 
            Sin.Pedophilia, 
            Sin.Blackmail, 
            Sin.IllegalWeapons,
            Sin.HumanTrafficking, 
            Sin.Slavery, 
            Sin.ForcedLabor, 
            Sin.OrganTrafficking,
            Sin.BudgetEmbezzlement, 
            Sin.CounterfeitGoods, 
            Sin.DrugProduction, 
            Sin.IllegalMonopoly, 
            Sin.StateContractFraud,
            Sin.TreasonContact, 
            Sin.ElectionFraud, 
            Sin.IllegalSurveillance, 
            Sin.PowerAbuse,
            Sin.WarCrimes, 
            Sin.ChemicalWeaponsUse, 
            Sin.TerrorismFunding, 
            Sin.ChildAbuse, 
            Sin.Cannibalism
        };
    }


    public static class SinFactory
    {
        

        public static IReadOnlyList<Sin> All() => Sin.AllSins;

        public static Sin Random() => Sin.AllSins[RandomS.Next(Sin.AllSins.Count)];
        private static readonly List<Sin> _shuffleBuffer = new List<Sin>(Sin.AllSins);
        public static IReadOnlyList<Sin> Random(int count)
        {
            if (count >= Sin.AllSins.Count)
                return new List<Sin>(Sin.AllSins);

            lock (_shuffleBuffer) {
                for (int i = _shuffleBuffer.Count - 1; i > 0; i--)
                {
                    int j = RandomS.Next(i + 1);
                    var tmp = _shuffleBuffer[i];
                    _shuffleBuffer[i] = _shuffleBuffer[j];
                    _shuffleBuffer[j] = tmp;
                }
                return _shuffleBuffer.Take(count).ToList();
                
            }
        }

        public static IReadOnlyList<Sin> RandomUpTo(int maxCount)
        {
            int count = RandomS.Next(maxCount + 1);
            return Random(count);
        }
    }


}