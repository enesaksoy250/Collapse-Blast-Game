using System.Collections.Generic;
using UnityEngine;
using CollapseBlast.Data;
using CollapseBlast.Interfaces;

namespace CollapseBlast.Core
{
    /// <summary>
    /// Smart shuffle algorithm that guarantees at least one valid move after shuffling.
    /// DOES NOT use blind shuffle (random shuffle + check loop).
    /// Instead, uses Fisher-Yates shuffle + guaranteed move creation.
    /// </summary>
    public class SmartShuffler : IDeadlockHandler
    {
        private GridData gridData;
        private IGroupFinder groupFinder;
        private DeadlockDetector deadlockDetector;
        private int colorCount;

        // Reusable list for collecting all block data
        private List<BlockData> allBlocks;

        // Event for notifying when shuffle occurs
        public event System.Action OnShuffleCompleted;

        public SmartShuffler(GridData gridData, IGroupFinder groupFinder, DeadlockDetector deadlockDetector, int colorCount)
        {
            this.gridData = gridData;
            this.groupFinder = groupFinder;
            this.deadlockDetector = deadlockDetector;
            this.colorCount = colorCount;
            this.allBlocks = new List<BlockData>();
        }

        /// <summary>
        /// Updates references after configuration changes.
        /// </summary>
        public void UpdateConfiguration(GridData gridData, int colorCount)
        {
            this.gridData = gridData;
            this.colorCount = colorCount;
        }

        /// <summary>
        /// Checks if the current grid state is a deadlock.
        /// </summary>
        public bool IsDeadlock()
        {
            return deadlockDetector.QuickDeadlockCheck();
        }

        /// <summary>
        /// Shuffles the grid to resolve deadlock.
        /// Algorithm:
        /// 1. Collect all blocks
        /// 2. Fisher-Yates shuffle
        /// 3. Place blocks back on grid
        /// 4. Guarantee at least one valid move
        /// </summary>
        public void Shuffle()
        {
            CollectAllBlocks();

            if (allBlocks.Count < 2)
            {
                Debug.LogWarning("Not enough blocks to shuffle");
                return;
            }

            // Step 1: Fisher-Yates shuffle
            FisherYatesShuffle(allBlocks);

            // Step 2: Place shuffled blocks back on grid
            PlaceBlocksOnGrid();

            // Step 3: Guarantee at least one valid move
            GuaranteeValidMove();

            OnShuffleCompleted?.Invoke();
        }

        /// <summary>
        /// Collects all non-empty blocks from the grid.
        /// </summary>
        private void CollectAllBlocks()
        {
            allBlocks.Clear();

            for (int row = 0; row < gridData.RowCount; row++)
            {
                for (int col = 0; col < gridData.ColumnCount; col++)
                {
                    if (!gridData.IsEmpty(row, col))
                    {
                        allBlocks.Add(gridData.GetBlock(row, col));
                    }
                }
            }
        }

        /// <summary>
        /// Fisher-Yates shuffle algorithm for unbiased random permutation.
        /// Time complexity: O(n)
        /// </summary>
        private void FisherYatesShuffle(List<BlockData> list)
        {
            int n = list.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                // Swap elements
                BlockData temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        /// <summary>
        /// Places shuffled blocks back on the grid.
        /// </summary>
        private void PlaceBlocksOnGrid()
        {
            int index = 0;

            for (int row = 0; row < gridData.RowCount; row++)
            {
                for (int col = 0; col < gridData.ColumnCount; col++)
                {
                    if (index < allBlocks.Count)
                    {
                        BlockData block = allBlocks[index];
                        block.row = row;
                        block.column = col;
                        gridData.SetBlock(row, col, block);
                        index++;
                    }
                    else
                    {
                        gridData.ClearBlock(row, col);
                    }
                }
            }
        }

        /// <summary>
        /// Guarantees at least one valid move exists after shuffle.
        /// If deadlock still exists, creates a valid pair by swapping.
        /// This is NOT blind shuffle - we deterministically create a valid move.
        /// </summary>
        private void GuaranteeValidMove()
        {
            // Check if shuffle already created valid moves
            if (!deadlockDetector.QuickDeadlockCheck())
                return;

            // Still deadlock - create a guaranteed valid pair
            CreateValidPair();

            // Safety check - if still deadlock (shouldn't happen), try again
            int attempts = 0;
            while (deadlockDetector.QuickDeadlockCheck() && attempts < 10)
            {
                CreateValidPair();
                attempts++;
            }

            if (attempts >= 10)
            {
                Debug.LogWarning("SmartShuffler: Could not guarantee valid move after 10 attempts");
            }
        }

        /// <summary>
        /// Creates a valid pair by finding a position and setting its neighbor to the same color.
        /// </summary>
        private void CreateValidPair()
        {
            int rowCount = gridData.RowCount;
            int columnCount = gridData.ColumnCount;

            // Find a random non-empty position
            int startRow = Random.Range(0, rowCount);
            int startCol = Random.Range(0, columnCount);

            // Search for a valid position to create a pair
            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < columnCount; c++)
                {
                    int row = (startRow + r) % rowCount;
                    int col = (startCol + c) % columnCount;

                    if (gridData.IsEmpty(row, col))
                        continue;

                    int color = gridData.GetColorAt(row, col);

                    // Try to set right neighbor to same color
                    if (col + 1 < columnCount && !gridData.IsEmpty(row, col + 1))
                    {
                        SetBlockColor(row, col + 1, color);
                        return;
                    }

                    // Try to set bottom neighbor to same color
                    if (row + 1 < rowCount && !gridData.IsEmpty(row + 1, col))
                    {
                        SetBlockColor(row + 1, col, color);
                        return;
                    }

                    // Try to set left neighbor to same color
                    if (col - 1 >= 0 && !gridData.IsEmpty(row, col - 1))
                    {
                        SetBlockColor(row, col - 1, color);
                        return;
                    }

                    // Try to set top neighbor to same color
                    if (row - 1 >= 0 && !gridData.IsEmpty(row - 1, col))
                    {
                        SetBlockColor(row - 1, col, color);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the color of a block at the specified position.
        /// </summary>
        private void SetBlockColor(int row, int col, int colorIndex)
        {
            BlockData block = gridData.GetBlock(row, col);
            block.colorIndex = colorIndex;
            gridData.SetBlock(row, col, block);
        }
    }
}
