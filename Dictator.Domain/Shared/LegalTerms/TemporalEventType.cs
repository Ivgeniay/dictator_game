using System.Collections.Generic;
using Dictator.Domain.Utils;

namespace Dictator.Domain.Shared.LegalTerms
{
    /// <summary>
    /// Типобезопасная строка представляющая периодическое событие регулируемое законом.
    /// Используется в TemporalBillStatement для описания того что именно происходит
    /// через заданный интервал тиков — выборы, заседания, отчёты и т.д.
    /// </summary>
    public class TemporalEventType : StringType
    {
        public TemporalEventType() : base() { }
        public TemporalEventType(string value) : base(value) { }

        /// <summary>(Демократические) всеобщие выборы</summary>
        public static readonly TemporalEventType Elections = new TemporalEventType("elections");

        /// <summary>(Демократические) референдум</summary>
        public static readonly TemporalEventType Referendum = new TemporalEventType("referendum");

        /// <summary>(Законодательные) заседание парламента или иного органа</summary>
        public static readonly TemporalEventType Session = new TemporalEventType("session");

        /// <summary>(Законодательные) роспуск законодательного органа</summary>
        public static readonly TemporalEventType Dissolution = new TemporalEventType("dissolution");

        /// <summary>(Отчётность) обязательный отчёт перед вышестоящим органом</summary>
        public static readonly TemporalEventType Report = new TemporalEventType("report");

        /// <summary>(Отчётность) плановая проверка деятельности органа или субъекта</summary>
        public static readonly TemporalEventType Review = new TemporalEventType("review");

        /// <summary>(Исполнительные) инаугурация главы государства или должностного лица</summary>
        public static readonly TemporalEventType Inauguration = new TemporalEventType("inauguration");

        /// <summary>(Исполнительные) ротация состава органа или комиссии</summary>
        public static readonly TemporalEventType Rotation = new TemporalEventType("rotation");

        /// <summary>(Финансовые) утверждение государственного бюджета</summary>
        public static readonly TemporalEventType BudgetApproval = new TemporalEventType("budget_approval");

        /// <summary>(Финансовые) финансовый аудит организации или ведомства</summary>
        public static readonly TemporalEventType Audit = new TemporalEventType("audit");

        public static readonly IReadOnlyList<TemporalEventType> AllTypes = new List<TemporalEventType>
        {
            Elections, Referendum, Session, Dissolution, Report,
            Review, Inauguration, Rotation, BudgetApproval, Audit
        };
    }
}