namespace CollapseBlast.Interfaces
{
    /// <summary>
    /// Interface for deadlock detection and resolution.
    /// </summary>
    public interface IDeadlockHandler
    {
        /// <summary>
        /// Checks if the current grid state is a deadlock (no valid moves).
        /// </summary>
        bool IsDeadlock();

        /// <summary>
        /// Shuffles the grid to resolve deadlock.
        /// Guarantees at least one valid move after shuffle.
        /// </summary>
        void Shuffle();
    }
}
