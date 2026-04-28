using System;

namespace Dictator.Domain.Actors.Types
{
    /// <summary>
    /// Игровое представление игрока. Глава государства
    /// </summary>
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