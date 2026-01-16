using System.Collections.Generic;
using UnityEngine;

public class GravityHandler
{
    private GridManager gridManager;
    private GameConfig gameConfig;
    private int fallingBlockCount;

    public GravityHandler(GridManager gridManager, GameConfig gameConfig)
    {
        this.gridManager = gridManager;
        this.gameConfig = gameConfig;
    }

    public int ApplyGravity(HashSet<int> affectedColumns = null)
    {
        int maxFallDistance = 0;
        fallingBlockCount = 0;

        int columnCount = gridManager.ColumnCount;
        int rowCount = gridManager.RowCount;

        for (int col = 0; col < columnCount; col++)
        {
            if (affectedColumns != null && !affectedColumns.Contains(col))
                continue;

            int columnMaxFall = ProcessColumn(col, rowCount);
            maxFallDistance = Mathf.Max(maxFallDistance, columnMaxFall);
        }

        return maxFallDistance;
    }

    private int ProcessColumn(int col, int rowCount)
    {
        int maxFallDistance = 0;
        int writeRow = 0;

        for (int readRow = 0; readRow < rowCount; readRow++)
        {
            if (!gridManager.GridData.IsEmpty(readRow, col))
            {
                if (readRow != writeRow)
                {
                    int fallDistance = readRow - writeRow;
                    maxFallDistance = Mathf.Max(maxFallDistance, fallDistance);

                    gridManager.MoveBlock(readRow, col, writeRow, col, true);
                    fallingBlockCount++;
                }
                writeRow++;
            }
        }

        return maxFallDistance;
    }

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
