using System.Collections.Generic;
using UnityEngine;

public sealed class FillHandler
{
    private readonly GridManager gridManager;

    public FillHandler(GridManager gridManager)
    {
        this.gridManager = gridManager;
    }

    private int FillColumn(int col, int rowCount)
    {
        int emptyCount = 0;
        for (int row = 0; row < rowCount; row++)
        {
            if (gridManager.GridData.IsEmpty(row, col))
            {
                emptyCount++;
            }
        }

        if (emptyCount == 0)
            return 0;

        int spawnIndex = 0;
        for (int row = 0; row < rowCount; row++)
        {
            if (gridManager.GridData.IsEmpty(row, col))
            {
                int colorIndex = Random.Range(0, gridManager.ColorCount);

                int spawnOffset = spawnIndex + 1;
                Vector3 spawnPosition = gridManager.GetSpawnPosition(col, spawnOffset);
                Vector3 targetPosition = gridManager.GridToWorldPosition(row, col);

                Block block = gridManager.SpawnBlock(row, col, colorIndex, spawnPosition);
                if (block != null)
                {
                    block.MoveTo(targetPosition);
                }
                spawnIndex++;
            }
        }

        return emptyCount;
    }

    public int FillColumns(HashSet<int> columns)
    {
        int maxSpawnedInColumn = 0;
        int rowCount = gridManager.RowCount;

        foreach (int col in columns)
        {
            int spawned = FillColumn(col, rowCount);
            maxSpawnedInColumn = Mathf.Max(maxSpawnedInColumn, spawned);
        }

        return maxSpawnedInColumn;
    }
}
