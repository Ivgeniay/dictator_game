namespace Dictator.Domain.Shared.Metrics
{
    /// <summary>
    /// Качественная градация уровня лояльности — от открытой враждебности до безоговорочной преданности.
    /// Используется для определения поведения акторов без привязки к конкретным числам.
    /// </summary>
    public enum LoyaltyGrade
    {
        Hostile,
        Neutral,
        Loyal,
        Devoted
    }

    /// <summary>
    /// Числовой уровень лояльности актора к диктатору.
    /// Динамически настраиваемый диапазон позволяет использовать класс для разных контекстов -
    /// личной лояльности персонажа, лояльности организации или группы населения.
    /// </summary>
    public class Loyalty
    {
        private sbyte _maxValue;
        private sbyte _minValue; 
        private sbyte _value;

        public sbyte Value => _value;

        public Loyalty(sbyte initialValue = 0, sbyte maxValue = 100, sbyte minValue = -100)
        {
            if (maxValue == sbyte.MinValue) maxValue = sbyte.MinValue + (sbyte)1;
            if (minValue >= maxValue)
            {
                minValue = (sbyte)(maxValue - (sbyte)1);
            }

            _maxValue = maxValue;
            _minValue = minValue;
            _value = Clamp(initialValue);
        }

        public void Increase(sbyte amount) => _value = Clamp((sbyte)(_value + amount));
        public void Decrease(sbyte amount) => _value = Clamp((sbyte)(_value - amount));
        public void Set(sbyte value)       => _value = Clamp(value);
        public LoyaltyGrade GetGrade()
        {
            int range = _maxValue - _minValue;
            int relative = _value - _minValue;

            if (relative <= 0)               return LoyaltyGrade.Hostile;
            if (relative <= range / 4)       return LoyaltyGrade.Hostile;
            if (relative <= range / 2)       return LoyaltyGrade.Neutral;
            if (relative <= range / 4 * 3)   return LoyaltyGrade.Loyal;
            return LoyaltyGrade.Devoted;
        }

        private sbyte Clamp(sbyte value)
        {
            if (value > _maxValue) return _maxValue;
            if (value < _minValue) return _minValue;
            return value;
        }
    }
}