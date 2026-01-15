using System.Collections.Generic;
using UnityEngine;
using CollapseBlast.Managers;

namespace CollapseBlast.Handlers
{
    /// <summary>
    /// Handles the blast/collapse mechanic when blocks are destroyed.
    /// </summary>
    public class BlastHandler
    {
        private GridManager gridManager;

        // Event fired when blocks are blasted
        public event System.Action<List<Vector2Int>> OnBlocksBlasted;

        public BlastHandler(GridManager gridManager)
        {
            this.gridManager = gridManager;
        }

        /// <summary>
        /// Blasts (removes) all blocks in the given group.
        /// </summary>
        /// <param name="group">List of positions to blast</param>
        /// <returns>True if blast was successful</returns>
        public bool BlastGroup(List<Vector2Int> group)
        {
            if (group == null || group.Count < 2)
                return false;

            // Remove all blocks in the group
            gridManager.RemoveBlocks(group);

            // Notify listeners
            OnBlocksBlasted?.Invoke(group);

            return true;
        }

        /// <summary>
        /// Gets the columns affected by a blast.
        /// Used to optimize gravity calculations.
        /// </summary>
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
}
