using System.Collections.Generic;
using UnityEngine;

public sealed class BlastHandler
{
    private readonly GridManager gridManager;

    public BlastHandler(GridManager gridManager)
    {
        this.gridManager = gridManager;
    }

    public bool BlastGroup(List<Vector2Int> group)
    {
        if (group == null || group.Count < 2)
            return false;

        gridManager.RemoveBlocks(group);
        return true;
    }

    public HashSet<int> GetAffectedColumns(List<Vector2Int> group)
    {
        HashSet<int> columns = new HashSet<int>();
        foreach (Vector2Int pos in group)
        {
            columns.Add(pos.x);
        }
        return columns;
    }
}
