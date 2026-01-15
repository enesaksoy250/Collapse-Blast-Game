using System.Collections.Generic;
using UnityEngine;
using CollapseBlast.Managers;
using CollapseBlast.Config;
using CollapseBlast.Block;

namespace CollapseBlast.Handlers
{
    /// <summary>
    /// Handles filling empty cells with new blocks from the top.
    /// </summary>
    public class FillHandler
    {
        private GridManager gridManager;
        private GameConfig gameConfig;

        // Event fired when fill is complete
        public event System.Action OnFillComplete;

        public FillHandler(GridManager gridManager, GameConfig gameConfig)
        {
            this.gridManager = gridManager;
            this.gameConfig = gameConfig;
        }

        /// <summary>
        /// Fills all empty cells in the grid with new blocks.
        /// New blocks spawn above the grid and fall down.
        /// </summary>
        /// <returns>Number of blocks spawned</returns>
        public int FillEmptyCells()
        {
            int spawnedCount = 0;
            int rowCount = gridManager.RowCount;
            int columnCount = gridManager.ColumnCount;

            // Process each column
            for (int col = 0; col < columnCount; col++)
            {
                spawnedCount += FillColumn(col, rowCount);
            }

            OnFillComplete?.Invoke();
            return spawnedCount;
        }

        /// <summary>
        /// Fills empty cells in a specific column.
        /// </summary>
        private int FillColumn(int col, int rowCount)
        {
            int spawnedInColumn = 0;

            // Find empty cells from top to bottom
            for (int row = rowCount - 1; row >= 0; row--)
            {
                if (gridManager.GridData.IsEmpty(row, col))
                {
                    // Spawn new block
                    int colorIndex = Random.Range(0, gridManager.ColorCount);
                    Vector3 spawnPosition = gridManager.GetSpawnPosition(col, spawnedInColumn + 1);
                    Vector3 targetPosition = gridManager.GridToWorldPosition(row, col);

                    BlockView block = gridManager.SpawnBlock(row, col, colorIndex, spawnPosition);
                    if (block != null)
                    {
                        block.MoveTo(targetPosition, gameConfig.fallDuration);
                        spawnedInColumn++;
                    }
                }
            }

            return spawnedInColumn;
        }

        /// <summary>
        /// Fills specific columns with new blocks.
        /// Optimized version that only processes affected columns.
        /// </summary>
        public int FillColumns(HashSet<int> columns)
        {
            int spawnedCount = 0;
            int rowCount = gridManager.RowCount;

            foreach (int col in columns)
            {
                spawnedCount += FillColumn(col, rowCount);
            }

            if (spawnedCount > 0)
            {
                OnFillComplete?.Invoke();
            }

            return spawnedCount;
        }

        /// <summary>
        /// Fills empty cells immediately without animation.
        /// </summary>
        public void FillEmptyCellsImmediate()
        {
            int rowCount = gridManager.RowCount;
            int columnCount = gridManager.ColumnCount;

            for (int col = 0; col < columnCount; col++)
            {
                for (int row = 0; row < rowCount; row++)
                {
                    if (gridManager.GridData.IsEmpty(row, col))
                    {
                        int colorIndex = Random.Range(0, gridManager.ColorCount);
                        gridManager.CreateBlock(row, col, colorIndex);
                    }
                }
            }
        }
    }
}
