using Dictator.Domain.Actors.Types;
using System.Collections.Generic;
using System;

namespace Dictator.Domain.Actors.Relations.Personal
{
    /// <summary>
    /// Маркер сущности над которой можно проводить расследование.
    /// Реализуется теми акторами чьи скрытые грехи могут быть раскрыты - депутатами, министрами, олигархами.
    /// Возвращает полный список грехов - логика вероятности раскрытия конкретных грехов определяется на уровне UseCase.
    /// </summary>
    public interface IInvestigatable
    {
        IReadOnlyList<Sin> GetExposableSins();
    }

    /// <summary>
    /// Маркер организации способной проводить расследования.
    /// Реализуется институтами имеющими следственные полномочия - полицией, прокуратурой, СМИ, НКО.
    /// </summary>
    public interface IInvestigator
    {
        Investigation Investigate(IInvestigatable target, InvestigationContext context);
    }

    /// <summary>
    /// Результат расследования проведённого конкретной организацией над конкретной сущностью.
    /// Фиксирует кто расследовал, кого, какие грехи были раскрыты и в какой момент симуляции.
    /// Является материалом для дальнейших действий - шантажа, публикации в СМИ, уголовного преследования.
    /// </summary>
    public class Investigation
    {
        public Guid Id { get; }
        /// <summary>
        /// Кто проводит расследование
        /// </summary>
        public Organization Actor { get; }
        /// <summary>
        /// Над кем расследование
        /// </summary>
        public IInvestigatable Target { get; }
        /// <summary>
        /// Выявленые правонарушения
        /// </summary>
        public IReadOnlyList<Sin> ExposedSins { get; }
        /// <summary>
        /// Временная метка
        /// </summary>
        public long ConductedAtTick { get; }

        public Investigation(
            Guid id,
            Organization actor,
            IInvestigatable target,
            IReadOnlyList<Sin> exposedSins,
            long conductedAtTick)
        {
            Id = id;
            Actor = actor;
            Target = target;
            ExposedSins = exposedSins;
            ConductedAtTick = conductedAtTick;
        }
    }

    /// <summary>
    /// Контекст расследования определяющий вероятность раскрытия каждого греха.
    /// Вычисляется внешней системой на основе параметров организации - лояльности и уровня коррупции.
    /// Значение 0.0 означает гарантированный провал, 1.0 - полное раскрытие всех грехов.
    /// </summary>
    public class InvestigationContext
    {
        public double SuccessProbability { get; }

        public InvestigationContext(double successProbability)
        {
            SuccessProbability = Math.Max(0.0, Math.Min(1.0, successProbability));
        }
    }
}