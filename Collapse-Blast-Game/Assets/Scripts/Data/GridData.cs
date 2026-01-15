using UnityEngine;

namespace CollapseBlast.Data
{
    /// <summary>
    /// Manages the logical representation of the game grid.
    /// Stores block data in a 2D array for efficient access.
    /// </summary>
    public class GridData
    {
        private BlockData[,] blocks;
        private int rowCount;
        private int columnCount;

        public int RowCount => rowCount;
        public int ColumnCount => columnCount;

        public GridData(int rowCount, int columnCount)
        {
            this.rowCount = rowCount;
            this.columnCount = columnCount;
            blocks = new BlockData[rowCount, columnCount];
        }

        /// <summary>
        /// Gets block data at the specified position.
        /// </summary>
        public BlockData GetBlock(int row, int column)
        {
            if (!IsValidPosition(row, column))
            {
                Debug.LogWarning($"Invalid position: ({row}, {column})");
                return BlockData.Empty(row, column);
            }
            return blocks[row, column];
        }

        /// <summary>
        /// Sets block data at the specified position.
        /// </summary>
        public void SetBlock(int row, int column, BlockData data)
        {
            if (!IsValidPosition(row, column))
            {
                Debug.LogWarning($"Invalid position: ({row}, {column})");
                return;
            }
            data.row = row;
            data.column = column;
            blocks[row, column] = data;
        }

        /// <summary>
        /// Marks a cell as empty.
        /// </summary>
        public void ClearBlock(int row, int column)
        {
            if (!IsValidPosition(row, column))
                return;
            blocks[row, column] = BlockData.Empty(row, column);
        }

        /// <summary>
        /// Checks if the position is within grid bounds.
        /// </summary>
        public bool IsValidPosition(int row, int column)
        {
            return row >= 0 && row < rowCount && column >= 0 && column < columnCount;
        }

        /// <summary>
        /// Checks if a cell is empty.
        /// </summary>
        public bool IsEmpty(int row, int column)
        {
            if (!IsValidPosition(row, column))
                return true;
            return blocks[row, column].isEmpty;
        }

        /// <summary>
        /// Gets the color index at the specified position.
        /// Returns -1 if empty or invalid.
        /// </summary>
        public int GetColorAt(int row, int column)
        {
            if (!IsValidPosition(row, column) || blocks[row, column].isEmpty)
                return -1;
            return blocks[row, column].colorIndex;
        }

        /// <summary>
        /// Swaps two blocks in the grid.
        /// </summary>
        public void SwapBlocks(int row1, int col1, int row2, int col2)
        {
            if (!IsValidPosition(row1, col1) || !IsValidPosition(row2, col2))
                return;

            BlockData temp = blocks[row1, col1];
            blocks[row1, col1] = blocks[row2, col2];
            blocks[row2, col2] = temp;

            // Update position references
            blocks[row1, col1].row = row1;
            blocks[row1, col1].column = col1;
            blocks[row2, col2].row = row2;
            blocks[row2, col2].column = col2;
        }

        /// <summary>
        /// Clears all blocks in the grid.
        /// </summary>
        public void Clear()
        {
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < columnCount; col++)
                {
                    blocks[row, col] = BlockData.Empty(row, col);
                }
            }
        }
    }
}
