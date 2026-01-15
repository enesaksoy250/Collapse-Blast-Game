using System.Collections.Generic;
using UnityEngine;
using CollapseBlast.Data;
using CollapseBlast.Interfaces;

namespace CollapseBlast.Core
{
    /// <summary>
    /// Finds connected groups of same-colored blocks using BFS algorithm.
    /// Time complexity: O(M×N) for full grid scan.
    /// </summary>
    public class GroupFinder : IGroupFinder
    {
        private GridData gridData;
        private bool[,] visited;
        private int rowCount;
        private int columnCount;

        // Direction vectors for adjacent cells (up, down, left, right)
        private static readonly int[] rowDirections = { -1, 1, 0, 0 };
        private static readonly int[] colDirections = { 0, 0, -1, 1 };

        // Reusable collections to minimize GC allocations
        private Queue<Vector2Int> bfsQueue;
        private List<Vector2Int> currentGroup;

        public GroupFinder(GridData gridData)
        {
            this.gridData = gridData;
            this.rowCount = gridData.RowCount;
            this.columnCount = gridData.ColumnCount;
            this.visited = new bool[rowCount, columnCount];
            this.bfsQueue = new Queue<Vector2Int>(rowCount * columnCount);
            this.currentGroup = new List<Vector2Int>(rowCount * columnCount);
        }

        /// <summary>
        /// Updates the grid data reference (call this after grid changes).
        /// </summary>
        public void SetGridData(GridData gridData)
        {
            this.gridData = gridData;
            if (gridData.RowCount != rowCount || gridData.ColumnCount != columnCount)
            {
                this.rowCount = gridData.RowCount;
                this.columnCount = gridData.ColumnCount;
                this.visited = new bool[rowCount, columnCount];
            }
        }

        /// <summary>
        /// Finds all blocks connected to the block at the given position.
        /// Uses BFS (Breadth-First Search) algorithm.
        /// </summary>
        public List<Vector2Int> FindGroup(int row, int column)
        {
            currentGroup.Clear();

            if (!gridData.IsValidPosition(row, column) || gridData.IsEmpty(row, column))
                return currentGroup;

            int targetColor = gridData.GetColorAt(row, column);
            if (targetColor < 0)
                return currentGroup;

            // Reset visited array for this search
            ClearVisited();

            bfsQueue.Clear();
            bfsQueue.Enqueue(new Vector2Int(column, row));
            visited[row, column] = true;

            while (bfsQueue.Count > 0)
            {
                Vector2Int current = bfsQueue.Dequeue();
                int currentRow = current.y;
                int currentCol = current.x;

                currentGroup.Add(current);

                // Check all 4 adjacent cells
                for (int i = 0; i < 4; i++)
                {
                    int newRow = currentRow + rowDirections[i];
                    int newCol = currentCol + colDirections[i];

                    if (IsValidAndUnvisited(newRow, newCol, targetColor))
                    {
                        visited[newRow, newCol] = true;
                        bfsQueue.Enqueue(new Vector2Int(newCol, newRow));
                    }
                }
            }

            return currentGroup;
        }

        /// <summary>
        /// Finds all groups in the grid.
        /// </summary>
        public List<List<Vector2Int>> FindAllGroups()
        {
            List<List<Vector2Int>> allGroups = new List<List<Vector2Int>>();
            ClearVisited();

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < columnCount; col++)
                {
                    if (!visited[row, col] && !gridData.IsEmpty(row, col))
                    {
                        List<Vector2Int> group = FindGroupInternal(row, col);
                        if (group.Count >= 2)
                        {
                            allGroups.Add(new List<Vector2Int>(group));
                        }
                    }
                }
            }

            return allGroups;
        }

        /// <summary>
        /// Checks if there is at least one valid group (size >= 2) in the grid.
        /// Optimized to return early when a valid group is found.
        /// </summary>
        public bool HasValidGroup()
        {
            ClearVisited();

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < columnCount; col++)
                {
                    if (!visited[row, col] && !gridData.IsEmpty(row, col))
                    {
                        int groupSize = CountGroupSize(row, col);
                        if (groupSize >= 2)
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Internal method for finding a group without clearing visited array.
        /// </summary>
        private List<Vector2Int> FindGroupInternal(int row, int column)
        {
            List<Vector2Int> group = new List<Vector2Int>();

            if (!gridData.IsValidPosition(row, column) || gridData.IsEmpty(row, column))
                return group;

            int targetColor = gridData.GetColorAt(row, column);
            if (targetColor < 0)
                return group;

            bfsQueue.Clear();
            bfsQueue.Enqueue(new Vector2Int(column, row));
            visited[row, column] = true;

            while (bfsQueue.Count > 0)
            {
                Vector2Int current = bfsQueue.Dequeue();
                int currentRow = current.y;
                int currentCol = current.x;

                group.Add(current);

                for (int i = 0; i < 4; i++)
                {
                    int newRow = currentRow + rowDirections[i];
                    int newCol = currentCol + colDirections[i];

                    if (IsValidAndUnvisited(newRow, newCol, targetColor))
                    {
                        visited[newRow, newCol] = true;
                        bfsQueue.Enqueue(new Vector2Int(newCol, newRow));
                    }
                }
            }

            return group;
        }

        /// <summary>
        /// Counts the size of a group without storing positions.
        /// More efficient for just checking if a valid group exists.
        /// </summary>
        private int CountGroupSize(int row, int column)
        {
            if (!gridData.IsValidPosition(row, column) || gridData.IsEmpty(row, column))
                return 0;

            int targetColor = gridData.GetColorAt(row, column);
            if (targetColor < 0)
                return 0;

            int count = 0;
            bfsQueue.Clear();
            bfsQueue.Enqueue(new Vector2Int(column, row));
            visited[row, column] = true;

            while (bfsQueue.Count > 0)
            {
                Vector2Int current = bfsQueue.Dequeue();
                int currentRow = current.y;
                int currentCol = current.x;

                count++;

                for (int i = 0; i < 4; i++)
                {
                    int newRow = currentRow + rowDirections[i];
                    int newCol = currentCol + colDirections[i];

                    if (IsValidAndUnvisited(newRow, newCol, targetColor))
                    {
                        visited[newRow, newCol] = true;
                        bfsQueue.Enqueue(new Vector2Int(newCol, newRow));
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Checks if a position is valid, unvisited, and has the target color.
        /// </summary>
        private bool IsValidAndUnvisited(int row, int column, int targetColor)
        {
            if (!gridData.IsValidPosition(row, column))
                return false;
            if (visited[row, column])
                return false;
            if (gridData.IsEmpty(row, column))
                return false;
            return gridData.GetColorAt(row, column) == targetColor;
        }

        /// <summary>
        /// Clears the visited array.
        /// </summary>
        private void ClearVisited()
        {
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < columnCount; col++)
                {
                    visited[row, col] = false;
                }
            }
        }
    }
}
