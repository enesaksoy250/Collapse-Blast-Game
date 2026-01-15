using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CollapseBlast.Managers;
using CollapseBlast.Config;

namespace CollapseBlast.Handlers
{
    /// <summary>
    /// Handles gravity mechanics - making blocks fall down to fill empty spaces.
    /// </summary>
    public class GravityHandler
    {
        private GridManager gridManager;
        private GameConfig gameConfig;

        // Event fired when gravity animation is complete
        public event System.Action OnGravityComplete;

        // Tracks falling blocks for animation completion
        private int fallingBlockCount;

        public GravityHandler(GridManager gridManager, GameConfig gameConfig)
        {
            this.gridManager = gridManager;
            this.gameConfig = gameConfig;
        }

        /// <summary>
        /// Applies gravity to make blocks fall down and fill empty spaces.
        /// </summary>
        /// <param name="affectedColumns">Optional: specific columns to process</param>
        /// <returns>Number of blocks that moved</returns>
        public int ApplyGravity(HashSet<int> affectedColumns = null)
        {
            int movedBlocks = 0;
            fallingBlockCount = 0;

            int columnCount = gridManager.ColumnCount;
            int rowCount = gridManager.RowCount;

            // Process each column (or only affected columns if specified)
            for (int col = 0; col < columnCount; col++)
            {
                if (affectedColumns != null && !affectedColumns.Contains(col))
                    continue;

                movedBlocks += ProcessColumn(col, rowCount);
            }

            return movedBlocks;
        }

        /// <summary>
        /// Processes a single column to apply gravity.
        /// Blocks fall down to fill empty spaces.
        /// </summary>
        private int ProcessColumn(int col, int rowCount)
        {
            int movedBlocks = 0;
            int writeRow = 0; // Next row to write a block to

            // Scan from bottom to top
            for (int readRow = 0; readRow < rowCount; readRow++)
            {
                if (!gridManager.GridData.IsEmpty(readRow, col))
                {
                    if (readRow != writeRow)
                    {
                        // Move block down
                        gridManager.MoveBlock(readRow, col, writeRow, col, true);
                        movedBlocks++;
                        fallingBlockCount++;
                    }
                    writeRow++;
                }
            }

            return movedBlocks;
        }

        /// <summary>
        /// Applies gravity synchronously without animation.
        /// Useful for initial grid setup or immediate processing.
        /// </summary>
        public void ApplyGravityImmediate()
        {
            int columnCount = gridManager.ColumnCount;
            int rowCount = gridManager.RowCount;

            for (int col = 0; col < columnCount; col++)
            {
                int writeRow = 0;

                for (int readRow = 0; readRow < rowCount; readRow++)
                {
                    if (!gridManager.GridData.IsEmpty(readRow, col))
                    {
                        if (readRow != writeRow)
                        {
                            gridManager.MoveBlock(readRow, col, writeRow, col, false);
                        }
                        writeRow++;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the number of empty cells in a column.
        /// </summary>
        public int GetEmptyCellCount(int column)
        {
            int count = 0;
            int rowCount = gridManager.RowCount;

            for (int row = 0; row < rowCount; row++)
            {
                if (gridManager.GridData.IsEmpty(row, column))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Gets all empty cells in the grid.
        /// </summary>
        public List<Vector2Int> GetEmptyCells()
        {
            List<Vector2Int> emptyCells = new List<Vector2Int>();
            int rowCount = gridManager.RowCount;
            int columnCount = gridManager.ColumnCount;

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < columnCount; col++)
                {
                    if (gridManager.GridData.IsEmpty(row, col))
                    {
                        emptyCells.Add(new Vector2Int(col, row));
                    }
                }
            }

            return emptyCells;
        }
    }
}
