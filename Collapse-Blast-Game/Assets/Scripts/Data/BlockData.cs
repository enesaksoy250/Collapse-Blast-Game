using UnityEngine;

namespace CollapseBlast.Data
{
    /// <summary>
    /// Struct representing the logical data of a block.
    /// Using struct instead of class for better memory performance (no GC allocations).
    /// </summary>
    public struct BlockData
    {
        public int colorIndex;
        public int row;
        public int column;
        public bool isEmpty;

        public BlockData(int colorIndex, int row, int column)
        {
            this.colorIndex = colorIndex;
            this.row = row;
            this.column = column;
            this.isEmpty = false;
        }

        /// <summary>
        /// Creates an empty block data (represents a vacant cell).
        /// </summary>
        public static BlockData Empty(int row, int column)
        {
            return new BlockData
            {
                colorIndex = -1,
                row = row,
                column = column,
                isEmpty = true
            };
        }

        /// <summary>
        /// Gets the position as Vector2Int.
        /// </summary>
        public Vector2Int Position => new Vector2Int(column, row);

        public override string ToString()
        {
            return isEmpty ? $"Empty({row},{column})" : $"Block({colorIndex},{row},{column})";
        }
    }
}
