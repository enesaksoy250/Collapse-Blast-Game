using System.Collections.Generic;
using UnityEngine;

public class SmartShuffler : IDeadlockHandler
{
    private GridData gridData;
    private IGroupFinder groupFinder;
    private DeadlockDetector deadlockDetector;
    private int colorCount;

    private List<BlockData> allBlocks;

    public event System.Action OnShuffleCompleted;

    public SmartShuffler(GridData gridData, IGroupFinder groupFinder, DeadlockDetector deadlockDetector, int colorCount)
    {
        this.gridData = gridData;
        this.groupFinder = groupFinder;
        this.deadlockDetector = deadlockDetector;
        this.colorCount = colorCount;
        this.allBlocks = new List<BlockData>();
    }

    public void UpdateConfiguration(GridData gridData, int colorCount)
    {
        this.gridData = gridData;
        this.colorCount = colorCount;
    }

    public bool IsDeadlock()
    {
        return deadlockDetector.QuickDeadlockCheck();
    }

    public void Shuffle()
    {
        Debug.Log("[SmartShuffler] Deadlock detected! Starting shuffle...");

        CollectAllBlocks();

        if (allBlocks.Count < 2)
        {
            Debug.LogWarning("[SmartShuffler] Not enough blocks to shuffle");
            return;
        }

        FisherYatesShuffle(allBlocks);
        PlaceBlocksOnGrid();
        GuaranteeValidMove();

        Debug.Log("[SmartShuffler] Shuffle completed! Grid now has valid moves.");

        OnShuffleCompleted?.Invoke();
    }

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

    private void FisherYatesShuffle(List<BlockData> list)
    {
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            BlockData temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

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

    private void GuaranteeValidMove()
    {
        if (!deadlockDetector.QuickDeadlockCheck())
            return;

        CreateValidPair();

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

    private void CreateValidPair()
    {
        int rowCount = gridData.RowCount;
        int columnCount = gridData.ColumnCount;

        int startRow = Random.Range(0, rowCount);
        int startCol = Random.Range(0, columnCount);

        for (int r = 0; r < rowCount; r++)
        {
            for (int c = 0; c < columnCount; c++)
            {
                int row = (startRow + r) % rowCount;
                int col = (startCol + c) % columnCount;

                if (gridData.IsEmpty(row, col))
                    continue;

                int color = gridData.GetColorAt(row, col);

                if (col + 1 < columnCount && !gridData.IsEmpty(row, col + 1))
                {
                    SetBlockColor(row, col + 1, color);
                    return;
                }

                if (row + 1 < rowCount && !gridData.IsEmpty(row + 1, col))
                {
                    SetBlockColor(row + 1, col, color);
                    return;
                }

                if (col - 1 >= 0 && !gridData.IsEmpty(row, col - 1))
                {
                    SetBlockColor(row, col - 1, color);
                    return;
                }

                if (row - 1 >= 0 && !gridData.IsEmpty(row - 1, col))
                {
                    SetBlockColor(row - 1, col, color);
                    return;
                }
            }
        }
    }

    private void SetBlockColor(int row, int col, int colorIndex)
    {
        BlockData block = gridData.GetBlock(row, col);
        block.colorIndex = colorIndex;
        gridData.SetBlock(row, col, block);
    }
}
