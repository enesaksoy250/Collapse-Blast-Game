using System.Collections.Generic;
using UnityEngine;

public sealed class GroupVisualUpdater
{
    private readonly GridManager gridManager;
    private readonly GameConfig gameConfig;
    private readonly GroupFinder groupFinder;

    public GroupVisualUpdater(GridManager gridManager, GameConfig gameConfig)
    {
        this.gridManager = gridManager;
        this.gameConfig = gameConfig;
        this.groupFinder = gridManager.GroupFinder;
    }

    public void UpdateAllIcons()
    {
        ResetAllIcons();

        List<List<Vector2Int>> allGroups = groupFinder.FindAllGroups();

        foreach (List<Vector2Int> group in allGroups)
        {
            IconState state = gameConfig.GetIconStateForGroupSize(group.Count);

            foreach (Vector2Int pos in group)
            {
                gridManager.UpdateBlockIconState(pos.y, pos.x, state);
            }
        }
    }

    private void ResetAllIcons()
    {
        int rowCount = gridManager.RowCount;
        int columnCount = gridManager.ColumnCount;

        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < columnCount; col++)
            {
                if (!gridManager.GridData.IsEmpty(row, col))
                {
                    gridManager.UpdateBlockIconState(row, col, IconState.Default);
                }
            }
        }
    }
}
