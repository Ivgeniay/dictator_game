using System;
using System.Collections.Generic;
using System.Linq;
using Dictator.Domain.Actors.Types;
using Newtonsoft.Json;

namespace Dictator.Domain.Statistics
{
    /// <summary>
    /// Статистика населения страны.
    /// Хранит абсолютную численность населения и процентное распределение типов граждан.
    /// Проценты типов граждан независимы друг от друга - процент водителей не влияет
    /// на процент курильщиков. Используется для вычисления реакции населения на законы.
    /// </summary>
    public class PopulationStatistics
    {
        private long _totalPopulation;
        private readonly Dictionary<string, double> _distribution;

        public long TotalPopulation => _totalPopulation;

        [JsonConstructor]
        public PopulationStatistics(long totalPopulation, Dictionary<string, double> distribution)
        {
            _totalPopulation = totalPopulation;
            _distribution = distribution ?? new Dictionary<string, double>();
        }

        public PopulationStatistics(long totalPopulation)
        {
            _totalPopulation = totalPopulation;
            _distribution = new Dictionary<string, double>();
        }

        public void IncreasePopulation(long amount)
        {
            if (amount <= 0) return;
            _totalPopulation += amount;
        }

        public void DecreasePopulation(long amount)
        {
            if (amount <= 0) return;
            _totalPopulation = Math.Max(0, _totalPopulation - amount);
        }

        public void SetPopulation(long amount)
        {
            if (amount < 0) return;
            _totalPopulation = amount;
        }

        public void SetPercentage(CitizenType type, double percentage)
        {
            if (percentage < 0.0 || percentage > 1.0) return;
            _distribution[type.Value] = percentage;
        }

        public double GetPercentage(CitizenType type)
        {
            if (_distribution.TryGetValue(type.Value, out var percentage))
                return percentage;
            return 0.0;
        }

        public long GetAbsoluteCount(CitizenType type)
        {
            return (long)(_totalPopulation * GetPercentage(type));
        }

        public IReadOnlyDictionary<string, double> GetDistribution()
        {
            return _distribution;
        }

        public IReadOnlyList<CitizenType> GetTypesAboveThreshold(double threshold)
        {
            return _distribution
                .Where(kv => kv.Value >= threshold)
                .Select(kv => new CitizenType(kv.Key))
                .ToList();
        }

        public void Randomize(IReadOnlyList<CitizenType> types, double minPercent = 0.01, double maxPercent = 0.80)
        {
            foreach (var type in types)
            {
                var percentage = minPercent + RandomS.NextDouble() * (maxPercent - minPercent);
                _distribution[type.Value] = Math.Round(percentage, 2);
            }
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static PopulationStatistics Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<PopulationStatistics>(json);
        }
    }
}