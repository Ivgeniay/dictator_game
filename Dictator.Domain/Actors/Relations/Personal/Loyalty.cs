namespace Dictator.Domain.Actors.Relations.Personal
{
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

        public void Increase(sbyte amount)
        {
            _value = Clamp((sbyte)(_value + amount));
        }

        public void Decrease(sbyte amount)
        {
            _value = Clamp((sbyte)(_value - amount));
        }

        public void Set(sbyte value)
        {
            _value = Clamp(value);
        }

        public bool IsPositive => _value > 0;
        public bool IsNegative => _value < 0;
        public bool IsNeutral  => _value == 0;

        private sbyte Clamp(sbyte value)
        {
            if (value > _maxValue) return _maxValue;
            if (value < _minValue) return _minValue;
            return value;
        }
    }
}