using System;

namespace Dictator.Domain.Actors.Types
{
    public class Person
    {
        public Guid Id { get; }
        public string Name { get; }
        public Actor Actor { get; }

        public Person(Guid id, string name, Actor actor)
        {
            Id = id;
            Name = name;
            Actor = actor;
        }
    }
}