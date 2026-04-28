namespace Dictator.Domain.Shared.Metrics
{
    /// <summary>
    /// Качественная градация уровня коррупции - от полного отсутствия до крайней степени.
    /// Используется для принятия решений в игровых механиках без привязки к конкретным числам.
    /// </summary>
    public enum CorruptionGrade
    {
        None,
        Low,
        Moderate,
        High,
        Extreme
    }

    /// <summary>
    /// Числовой уровень коррупции организации с динамически настраиваемым диапазоном.
    /// Влияет на эффективность расследований и вероятность саботажа действий диктатора.
    /// </summary>
    public class CorruptionLevel
    {
        private byte _maxValue;
        private byte _minValue; 
        private byte _value;
        public byte Value => _value;

        public CorruptionLevel(byte initialValue = 0, byte maxValue = 100, byte minValue = 0)
        {
            if (maxValue == byte.MinValue) maxValue = byte.MinValue + (byte)1;
            if (minValue >= maxValue)
            {
                minValue = (byte)(maxValue - (byte)1);
            }
            _maxValue = maxValue;
            _minValue = minValue;
            _value = Clamp(initialValue);
        }

        public void Increase(byte amount) => _value = Clamp((byte)(_value + amount));
        public void Decrease(byte amount) => _value = Clamp((byte)(_value - amount));
        public void Set(byte value) => _value = Clamp(value);
        public CorruptionGrade GetGrade()
        {
            if (_value == _minValue)   return CorruptionGrade.None;
            if (_value <= (_maxValue - _minValue) / 4)  return CorruptionGrade.Low;
            if (_value <= (_maxValue - _minValue) / 2)  return CorruptionGrade.Moderate;
            if (_value <= (_maxValue - _minValue) / 4 * 3)  return CorruptionGrade.High;
            return CorruptionGrade.Extreme;
        }

        private byte Clamp(byte value)
        {
            if (value > _maxValue) return _maxValue;
            if (value < _minValue) return _minValue;
            return value;
        }
    }
}