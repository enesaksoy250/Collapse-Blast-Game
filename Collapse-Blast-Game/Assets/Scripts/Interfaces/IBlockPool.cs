using CollapseBlast.Block;

namespace CollapseBlast.Interfaces
{
    /// <summary>
    /// Interface for block object pooling.
    /// </summary>
    public interface IBlockPool
    {
        /// <summary>
        /// Gets a block from the pool or creates a new one.
        /// </summary>
        BlockView Get();

        /// <summary>
        /// Returns a block to the pool.
        /// </summary>
        void Return(BlockView block);

        /// <summary>
        /// Pre-warms the pool with the specified number of blocks.
        /// </summary>
        void Prewarm(int count);

        /// <summary>
        /// Clears all blocks in the pool.
        /// </summary>
        void Clear();
    }
}
