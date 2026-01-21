using System.Collections.Generic;
using UnityEngine;

public sealed class GroupFinder : IGroupFinder
{
    private readonly GridData gridData;
    private readonly int[,] visitedGeneration;
    private readonly int rowCount;
    private readonly int columnCount;

    private static readonly int[] rowDirections = { -1, 1, 0, 0 };
    private static readonly int[] colDirections = { 0, 0, -1, 1 };

    private readonly Queue<Vector2Int> bfsQueue;
    private readonly List<Vector2Int> currentGroup;

    // Generation counter - increment instead of clearing array
    private int currentGeneration;

    // List Pool for FindAllGroups to reduce GC allocations
    private readonly List<List<Vector2Int>> cachedAllGroups;
    private readonly Stack<List<Vector2Int>> listPool;
    private const int INITIAL_POOL_SIZE = 16;

    public GroupFinder(GridData gridData)
    {
        this.gridData = gridData;
        this.rowCount = gridData.RowCount;
        this.columnCount = gridData.ColumnCount;
        this.visitedGeneration = new int[rowCount, columnCount];
        this.bfsQueue = new Queue<Vector2Int>(rowCount * columnCount);
        this.currentGroup = new List<Vector2Int>(rowCount * columnCount);
        this.currentGeneration = 0;

        // Initialize list pool
        this.cachedAllGroups = new List<List<Vector2Int>>(INITIAL_POOL_SIZE);
        this.listPool = new Stack<List<Vector2Int>>(INITIAL_POOL_SIZE);

        // Pre-populate pool
        for (int i = 0; i < INITIAL_POOL_SIZE; i++)
        {
            listPool.Push(new List<Vector2Int>(16));
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

        ResetVisited();

        bfsQueue.Clear();
        bfsQueue.Enqueue(new Vector2Int(column, row));
        MarkVisited(row, column);

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
                    MarkVisited(newRow, newCol);
                    bfsQueue.Enqueue(new Vector2Int(newCol, newRow));
                }
            }
        }

        return currentGroup;
    }

    public List<List<Vector2Int>> FindAllGroups()
    {
        // Return previous lists to pool
        ReturnListsToPool();

        ResetVisited();

        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < columnCount; col++)
            {
                if (!IsVisited(row, col) && !gridData.IsEmpty(row, col))
                {
                    List<Vector2Int> group = GetListFromPool();
                    FindGroupIntoList(row, col, group);

                    if (group.Count >= 2)
                    {
                        cachedAllGroups.Add(group);
                    }
                    else
                    {
                        // Return unused list to pool
                        ReturnListToPool(group);
                    }
                }
            }
        }

        return cachedAllGroups;
    }

    public bool HasValidGroup()
    {
        ResetVisited();

        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < columnCount; col++)
            {
                if (!IsVisited(row, col) && !gridData.IsEmpty(row, col))
                {
                    int groupSize = CountGroupSize(row, col);
                    if (groupSize >= 2)
                        return true;
                }
            }
        }

        return false;
    }

    private void FindGroupIntoList(int row, int column, List<Vector2Int> group)
    {
        if (!gridData.IsValidPosition(row, column) || gridData.IsEmpty(row, column))
            return;

        int targetColor = gridData.GetColorAt(row, column);
        if (targetColor < 0)
            return;

        bfsQueue.Clear();
        bfsQueue.Enqueue(new Vector2Int(column, row));
        MarkVisited(row, column);

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
                    MarkVisited(newRow, newCol);
                    bfsQueue.Enqueue(new Vector2Int(newCol, newRow));
                }
            }
        }
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
        MarkVisited(row, column);

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
                    MarkVisited(newRow, newCol);
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
        if (IsVisited(row, column))
            return false;
        if (gridData.IsEmpty(row, column))
            return false;
        return gridData.GetColorAt(row, column) == targetColor;
    }

    // O(1) reset - just increment the generation counter
    private void ResetVisited()
    {
        currentGeneration++;
    }

    // O(1) check if visited in current generation
    private bool IsVisited(int row, int column)
    {
        return visitedGeneration[row, column] == currentGeneration;
    }

    // O(1) mark as visited in current generation
    private void MarkVisited(int row, int column)
    {
        visitedGeneration[row, column] = currentGeneration;
    }

    // List Pool Methods
    private List<Vector2Int> GetListFromPool()
    {
        if (listPool.Count > 0)
        {
            return listPool.Pop();
        }
        return new List<Vector2Int>(16);
    }

    private void ReturnListToPool(List<Vector2Int> list)
    {
        list.Clear();
        listPool.Push(list);
    }

    private void ReturnListsToPool()
    {
        foreach (var list in cachedAllGroups)
        {
            list.Clear();
            listPool.Push(list);
        }
        cachedAllGroups.Clear();
    }
}
