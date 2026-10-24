using System;
using System.Globalization;

namespace Dictator.Engine.Core.Primitives
{
    public readonly struct Meter : IEquatable<Meter>, IComparable<Meter>
    {
        private const decimal MetersPerKilometer = 1000m;
        private const decimal MetersPerCentimeter = 0.01m;
        private const decimal MetersPerMillimeter = 0.001m;

        public static readonly Meter Zero = new Meter(0m);

        public decimal Value { get; }

        public Meter(decimal value)
        {
            Value = value;
        }

        public int Sign => Math.Sign(Value);

        public static Meter FromKilometers(decimal kilometers) => new Meter(kilometers * MetersPerKilometer);

        public static Meter FromCentimeters(decimal centimeters) => new Meter(centimeters * MetersPerCentimeter);

        public static Meter FromMillimeters(decimal millimeters) => new Meter(millimeters * MetersPerMillimeter);

        public decimal ToKilometers() => Value / MetersPerKilometer;

        public decimal ToCentimeters() => Value / MetersPerCentimeter;

        public decimal ToMillimeters() => Value / MetersPerMillimeter;

        public Meter Abs() => new Meter(Math.Abs(Value));

        public bool Equals(Meter other) => Value == other.Value;

        public override bool Equals(object obj) => obj is Meter other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode();

        public int CompareTo(Meter other) => Value.CompareTo(other.Value);

        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture) + "m";

        public static Meter operator +(Meter left, Meter right) => new Meter(left.Value + right.Value);

        public static Meter operator -(Meter left, Meter right) => new Meter(left.Value - right.Value);

        public static Meter operator -(Meter value) => new Meter(-value.Value);

        public static Meter operator *(Meter value, decimal factor) => new Meter(value.Value * factor);

        public static Meter operator *(decimal factor, Meter value) => new Meter(factor * value.Value);

        public static Meter operator /(Meter value, decimal divisor) => new Meter(value.Value / divisor);

        public static decimal operator /(Meter left, Meter right) => left.Value / right.Value;

        public static bool operator ==(Meter left, Meter right) => left.Equals(right);

        public static bool operator !=(Meter left, Meter right) => !left.Equals(right);

        public static bool operator <(Meter left, Meter right) => left.Value < right.Value;

        public static bool operator >(Meter left, Meter right) => left.Value > right.Value;

        public static bool operator <=(Meter left, Meter right) => left.Value <= right.Value;

        public static bool operator >=(Meter left, Meter right) => left.Value >= right.Value;
    }
}