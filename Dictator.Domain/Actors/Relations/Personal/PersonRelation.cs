using Dictator.Domain.Shared.Metrics;
using Dictator.Domain.Utils.Masks;

namespace Dictator.Domain.Actors.Relations.Personal
{
    /// <summary>
    /// Личное отношение конкретного человека к диктатору.
    /// Выражается через набор активных эмоций и числовой уровень лояльности.
    /// Эмоции не исключают друг друга - человек может одновременно испытывать страх и уважение, что типично для условий диктатуры.
    /// </summary>
    public class PersonRelation
    {
        private readonly DynamicMask<Emotion> _emotions;
        private readonly Loyalty _loyalty;

        public Loyalty Loyalty => _loyalty;

        public PersonRelation()
        {
            _emotions = EmotionMask.CreateDefault();
            _loyalty = new Loyalty();
        }

        public PersonRelation(DynamicMask<Emotion> emotions, Loyalty loyalty)
        {
            _emotions = emotions;
            _loyalty = loyalty;
        }

        public void ApplyEmotion(Emotion emotion)
        {
            _emotions.Include(emotion);
        }

        public void ApplyRandomEmotion()
        {
            Emotion emotion = Emotion.AllEmotions[RandomS.Next(Emotion.AllEmotions.Count)];
            _emotions.ExcludeAll();
            _emotions.Include(emotion);
        }

        public void RemoveEmotion(Emotion emotion)
        {
            _emotions.Exclude(emotion);
        }

        public bool HasEmotion(Emotion emotion)
        {
            return _emotions.Matches(emotion);
        }

        public System.Collections.Generic.IReadOnlyCollection<Emotion> GetActiveEmotions()
        {
            return _emotions.GetIncluded();
        }

        public void IncreaseLoyalty(sbyte amount)
        {
            _loyalty.Increase(amount);
        }

        public void DecreaseLoyalty(sbyte amount)
        {
            _loyalty.Decrease(amount);
        }

        public void SetLoyalty(sbyte value)
        {
            _loyalty.Set(value);
        }
    }
}