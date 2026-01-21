public sealed class DeadlockDetector
{
    private readonly GridData gridData;
    private readonly IGroupFinder groupFinder;

    public DeadlockDetector(GridData gridData, IGroupFinder groupFinder)
    {
        this.gridData = gridData;
        this.groupFinder = groupFinder;
    }

    public bool IsDeadlock()
    {
        return !groupFinder.HasValidGroup();
    }

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

                if (col + 1 < columnCount && !gridData.IsEmpty(row, col + 1))
                {
                    if (gridData.GetColorAt(row, col + 1) == color)
                        return false;
                }

                if (row + 1 < rowCount && !gridData.IsEmpty(row + 1, col))
                {
                    if (gridData.GetColorAt(row + 1, col) == color)
                        return false;
                }
            }
        }

        return true;
    }
}
