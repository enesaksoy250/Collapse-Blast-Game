using System.Collections.Generic;
using UnityEngine;

public sealed class GravityHandler
{
    private readonly GridManager gridManager;

    public GravityHandler(GridManager gridManager)
    {
        this.gridManager = gridManager;
    }

    public int ApplyGravity(HashSet<int> affectedColumns = null)
    {
        int maxFallDistance = 0;

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
                }
                writeRow++;
            }
        }

        return maxFallDistance;
    }
}
