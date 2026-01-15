using CollapseBlast.Data;
using CollapseBlast.Interfaces;

namespace CollapseBlast.Core
{
    /// <summary>
    /// Detects deadlock situations where no valid moves exist.
    /// A deadlock occurs when there are no groups of 2 or more same-colored adjacent blocks.
    /// </summary>
    public class DeadlockDetector
    {
        private GridData gridData;
        private IGroupFinder groupFinder;

        public DeadlockDetector(GridData gridData, IGroupFinder groupFinder)
        {
            this.gridData = gridData;
            this.groupFinder = groupFinder;
        }

        /// <summary>
        /// Updates references after grid changes.
        /// </summary>
        public void SetGridData(GridData gridData)
        {
            this.gridData = gridData;
        }

        /// <summary>
        /// Checks if the current grid state is a deadlock.
        /// Returns true if no valid moves exist.
        /// </summary>
        public bool IsDeadlock()
        {
            // Use the optimized HasValidGroup method from GroupFinder
            return !groupFinder.HasValidGroup();
        }

        /// <summary>
        /// Quick check by scanning for any adjacent same-colored blocks.
        /// This is faster than finding all groups when we just need to know if moves exist.
        /// </summary>
        public bool QuickDeadlockCheck()
        {
            int rowCount = gridData.RowCount;
            int columnCount = gridData.ColumnCount;

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < columnCount; col++)
                {
                    if (gridData.IsEmpty(row, col))
                        continue;

                    int color = gridData.GetColorAt(row, col);

                    // Check right neighbor
                    if (col + 1 < columnCount && !gridData.IsEmpty(row, col + 1))
                    {
                        if (gridData.GetColorAt(row, col + 1) == color)
                            return false; // Found a valid pair, not a deadlock
                    }

                    // Check bottom neighbor
                    if (row + 1 < rowCount && !gridData.IsEmpty(row + 1, col))
                    {
                        if (gridData.GetColorAt(row + 1, col) == color)
                            return false; // Found a valid pair, not a deadlock
                    }
                }
            }

            return true; // No valid pairs found, it's a deadlock
        }
    }
}
