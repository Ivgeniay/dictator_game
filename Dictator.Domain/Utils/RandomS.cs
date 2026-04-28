using System;

public static class RandomS
{
    private static readonly Random _rnd = new Random();
    private static readonly object _lock = new object();

    public static int Next(int maxValue)
    {
        lock (_lock) return _rnd.Next(maxValue);
    }

    public static int Next(int minValue, int maxValue)
    {
        lock (_lock) return _rnd.Next(minValue, maxValue);
    }

    internal static double NextDouble()
    {
        lock (_lock) return _rnd.NextDouble();
    }

}