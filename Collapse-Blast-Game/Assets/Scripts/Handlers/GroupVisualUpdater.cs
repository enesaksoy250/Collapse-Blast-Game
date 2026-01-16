using System.Collections.Generic;
using UnityEngine;

public class GroupVisualUpdater
{
    private GridManager gridManager;
    private GameConfig gameConfig;
    private GroupFinder groupFinder;

    public GroupVisualUpdater(GridManager gridManager, GameConfig gameConfig)
    {
        this.gridManager = gridManager;
        this.gameConfig = gameConfig;
        this.groupFinder = gridManager.GroupFinder;
    }

    public void UpdateAllIcons()
    {
        int rowCount = gridManager.RowCount;
        int columnCount = gridManager.ColumnCount;

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

    public void UpdateIconsForPositions(List<Vector2Int> positions)
    {
        HashSet<Vector2Int> processedPositions = new HashSet<Vector2Int>();

        foreach (Vector2Int pos in positions)
        {
            if (processedPositions.Contains(pos))
                continue;

            if (gridManager.GridData.IsEmpty(pos.y, pos.x))
                continue;

            List<Vector2Int> group = groupFinder.FindGroup(pos.y, pos.x);
            IconState state = gameConfig.GetIconStateForGroupSize(group.Count);

            foreach (Vector2Int groupPos in group)
            {
                gridManager.UpdateBlockIconState(groupPos.y, groupPos.x, state);
                processedPositions.Add(groupPos);
            }
        }
    }
}
