using System.Collections.Generic;
using UnityEngine;

public class GroupFinder : IGroupFinder
{
    private GridData gridData;
    private bool[,] visited;
    private int rowCount;
    private int columnCount;

    private static readonly int[] rowDirections = { -1, 1, 0, 0 };
    private static readonly int[] colDirections = { 0, 0, -1, 1 };

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

    public List<Vector2Int> FindGroup(int row, int column)
    {
        currentGroup.Clear();

        if (!gridData.IsValidPosition(row, column) || gridData.IsEmpty(row, column))
            return currentGroup;

        int targetColor = gridData.GetColorAt(row, column);
        if (targetColor < 0)
            return currentGroup;

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
