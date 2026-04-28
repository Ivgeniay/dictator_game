using System;
using Dictator.Domain.Shared.LegalTerms;
using Dictator.Domain.Shared.Metrics;

namespace Dictator.Domain.Actors.Types
{
    /// <summary>
    /// Кто может владеть организацией
    /// </summary>
    public interface IOwner { }

    /// <summary>
    /// Что-то, что может принадлежать IOwner
    /// </summary>
    public interface IOwnable
    {
        IOwner Owner { get; }
        void TransferOwnership(IOwner newOwner);
    }


    /// <summary>
    /// Любая институциональная структура способная совершать действия в симуляции.
    /// Моделирует организации разного типа - от СМИ и бизнеса до силовых структур и НКО.
    /// Владелец организации может меняться в процессе игры - национализация и приватизация
    /// являются ключевыми инструментами диктатора.
    /// </summary>
    public abstract class Organization : IOwnable
    {
        public class OrganizationModerator
        {
            private static OrganizationModerator? _instance = null;
            public static OrganizationModerator? CreateSingle()
            {
                if (_instance != null) return null;
                _instance = new OrganizationModerator();
                return _instance;
            }

            private OrganizationModerator() { }

            public void IncreaseLoyalty(Organization org, sbyte amount) => org.loyalty.Increase(amount);
            public void DecreaseLoyalty(Organization org, sbyte amount) => org.loyalty.Decrease(amount);
            public void SetLoyalty(Organization org, sbyte value) => org.loyalty.Set(value);

            public void IncreaseCorruption(Organization org, byte amount) => org.corruptionLevel.Increase(amount);
            public void DecreaseCorruption(Organization org, byte amount) => org.corruptionLevel.Decrease(amount);
            public void SetCorruption(Organization org, byte value) => org.corruptionLevel.Set(value);
        }

        public string Type { get; protected set; } = string.Empty;
        public Guid Id { get; }
        public string Name { get; }
        public SubjectType Subject { get; }
        public IOwner Owner { get; private set; }

        private Loyalty loyalty { get; }
        private CorruptionLevel corruptionLevel { get; }

        protected Organization(Guid id, string name, SubjectType subject, IOwner owner)
            : this(id, name, subject, owner, new Loyalty(), new CorruptionLevel()) { }

        protected Organization(
            Guid id,
            string name,
            SubjectType subject,
            IOwner owner,
            Loyalty loyalty,
            CorruptionLevel corruptionLevel)
        {
            Id = id;
            Name = name;
            Subject = subject;
            Owner = owner;
            Type = GetType().FullName;
            this.loyalty = loyalty;
            this.corruptionLevel = corruptionLevel;
        }

        public void TransferOwnership(IOwner newOwner) => Owner = newOwner;
        public sbyte GetLoyaltyLevel() => loyalty.Value;
        public byte GetCorruptionLevel() => corruptionLevel.Value;
        public LoyaltyGrade GetLoyaltyGrade() => loyalty.GetGrade();
        public CorruptionGrade GetCorruptionGrade() => corruptionLevel.GetGrade();
    }
}