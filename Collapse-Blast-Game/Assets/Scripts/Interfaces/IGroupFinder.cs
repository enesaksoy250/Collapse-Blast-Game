using System.Collections.Generic;
using UnityEngine;

namespace CollapseBlast.Interfaces
{
    /// <summary>
    /// Interface for finding connected groups of blocks.
    /// </summary>
    public interface IGroupFinder
    {
        /// <summary>
        /// Finds all blocks connected to the block at the given position.
        /// </summary>
        List<Vector2Int> FindGroup(int row, int column);

        /// <summary>
        /// Finds all groups in the grid.
        /// Returns a list of groups, where each group is a list of positions.
        /// </summary>
        List<List<Vector2Int>> FindAllGroups();

        /// <summary>
        /// Checks if there is at least one valid group (size >= 2) in the grid.
        /// </summary>
        bool HasValidGroup();
    }
}
