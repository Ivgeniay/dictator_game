namespace Dictator.Domain.State
{
    /// <summary>
    /// Синглтон хранящий текущее состояние игровой симуляции.
    /// Является единственным источником истины для игрового времени - текущего тика.
    /// </summary>
    public class GameState
    {
        public static readonly GameState Instance = new GameState();

        public long CurrentTick { get; private set; }

        private GameState()
        {
            CurrentTick = 0;
        }

        public void IncreaseTick(long value) => CurrentTick += value;
        public void AdvanceTick() => CurrentTick++;
        public void ResetTick() => CurrentTick = 0;
        
    }
}