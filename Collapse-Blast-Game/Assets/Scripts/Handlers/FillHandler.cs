using System.Collections.Generic;
using UnityEngine;

public class FillHandler
{
    private GridManager gridManager;
    private GameConfig gameConfig;

    public event System.Action OnFillComplete;

    public FillHandler(GridManager gridManager, GameConfig gameConfig)
    {
        this.gridManager = gridManager;
        this.gameConfig = gameConfig;
    }

    public int FillEmptyCells()
    {
        int spawnedCount = 0;
        int rowCount = gridManager.RowCount;
        int columnCount = gridManager.ColumnCount;

        for (int col = 0; col < columnCount; col++)
        {
            spawnedCount += FillColumn(col, rowCount);
        }

        OnFillComplete?.Invoke();
        return spawnedCount;
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

                BlockView block = gridManager.SpawnBlock(row, col, colorIndex, spawnPosition);
                if (block != null)
                {
                    block.MoveTo(targetPosition, gameConfig.fallDuration);
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

        if (maxSpawnedInColumn > 0)
        {
            OnFillComplete?.Invoke();
        }

        return maxSpawnedInColumn;
    }

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
