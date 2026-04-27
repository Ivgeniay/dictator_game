using System.Collections.Generic;
using System.Linq;
using Dictator.Domain.Utils.Masks;

namespace Dictator.Domain.Actors.Relations.Personal
{
    public class PersonRelation
    {
        private readonly DynamicMask<Emotion> _emotions;
        private readonly Loyalty _loyalty;
        private readonly List<Sin> _sins;

        public Loyalty Loyalty => _loyalty;
        public IReadOnlyList<Sin> Sins => _sins;

        public PersonRelation()
        {
            _emotions = EmotionMask.CreateDefault();
            _loyalty = new Loyalty();
            _sins = new List<Sin>();
        }

        public PersonRelation(DynamicMask<Emotion> emotion, Loyalty loyalty, List<Sin> sins)
        {
            _emotions = EmotionMask.CreateDefault();
            _emotions.Include(emotion.GetIncluded());
            _emotions.Exclude(emotion.GetExcluded());
            _loyalty = new Loyalty();
            _sins = new List<Sin>();
        }

        public void ApplyEmotion(Emotion emotion)
        {
            _emotions.Include(emotion);
        }

        public void ApplyRandomEmotion()
        {
            Emotion emotion = Emotion.AllEmotions[RandomS.Next(Emotion.AllEmotions.Count())];
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

        public IReadOnlyCollection<Emotion> GetActiveEmotions()
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

        public void AddSin(Sin sin)
        {
            if (!_sins.Contains(sin))
                _sins.Add(sin);
        }

        public void AddSins(IEnumerable<Sin> sins)
        {
            foreach (var sin in sins)
                AddSin(sin);
        }

        public void RemoveSin(Sin sin)
        {
            _sins.Remove(sin);
        }

        public bool HasSin(Sin sin)
        {
            return _sins.Contains(sin);
        }
    }
}