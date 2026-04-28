using System;

namespace Dictator.Domain.Utils {

    /// <summary>
    /// Абстрактный базовый класс для типобезопасных строковых значений предметной области.
    /// Предотвращает случайное смешение строк разного смысла на уровне компилятора.
    /// Поле Type содержит полное имя конкретного класса и используется при сериализации.
    /// </summary>
    public abstract class StringType
    {
        public string Type { get; }
        public string Value { get; protected set; }

        public StringType()
        {
            Type = GetType().FullName;
            Value = string.Empty;
        }

        public StringType(string value) : this()
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            return Value == ((StringType)obj).Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        public override string ToString()
        {
            return Value;
        }
    } 
}