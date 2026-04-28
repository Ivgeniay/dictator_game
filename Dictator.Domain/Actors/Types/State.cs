using Dictator.Domain.Actors.Types;

namespace Dictator.Domain.Actors {

    /// <summary>
    /// Синглтон представляющий государство как абстрактного владельца имущества.
    /// Используется когда организация принадлежит государству а не конкретной личности.
    /// </summary>
    public class State : IOwner
    {
        public static readonly State Instance = new State();
        private State() { }
    }
    
}