using System;

namespace Dictator.Domain.Actors.Types
{
    public class Dictator
    {
        public Guid Id { get; }
        public string Name { get; }

        public Dictator(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}