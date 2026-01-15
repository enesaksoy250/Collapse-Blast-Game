using System.Collections.Generic;
using UnityEngine;
using CollapseBlast.Config;
using CollapseBlast.Core;
using CollapseBlast.Managers;

namespace CollapseBlast.Handlers
{
    /// <summary>
    /// Updates block icons based on group sizes.
    /// Blocks in larger groups display different icons.
    /// </summary>
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

        /// <summary>
        /// Updates the icons of all blocks based on their group sizes.
        /// Should be called after any grid change (blast, gravity, fill).
        /// </summary>
        public void UpdateAllIcons()
        {
            int rowCount = gridManager.RowCount;
            int columnCount = gridManager.ColumnCount;

            // First, reset all blocks to default state
            ResetAllIcons();

            // Find all groups and update their icons
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

        /// <summary>
        /// Resets all block icons to default state.
        /// </summary>
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

        /// <summary>
        /// Updates icons for specific positions only.
        /// More efficient when only a portion of the grid changed.
        /// </summary>
        public void UpdateIconsForPositions(List<Vector2Int> positions)
        {
            HashSet<Vector2Int> processedPositions = new HashSet<Vector2Int>();

            foreach (Vector2Int pos in positions)
            {
                if (processedPositions.Contains(pos))
                    continue;

                if (gridManager.GridData.IsEmpty(pos.y, pos.x))
                    continue;

                // Find the group for this position
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
}
