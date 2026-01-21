using UnityEngine;

public sealed class GridData
{
    private BlockData[,] blocks;
    private int rowCount;
    private int columnCount;

    public int RowCount => rowCount;
    public int ColumnCount => columnCount;

    public GridData(int rowCount, int columnCount)
    {
        this.rowCount = rowCount;
        this.columnCount = columnCount;
        blocks = new BlockData[rowCount, columnCount];
        Clear();
    }

    public BlockData GetBlock(int row, int column)
    {
        if (!IsValidPosition(row, column))
        {
            Debug.LogWarning($"Invalid position: ({row}, {column})");
            return BlockData.Empty(row, column);
        }
        return blocks[row, column];
    }

    public void SetBlock(int row, int column, BlockData data)
    {
        if (!IsValidPosition(row, column))
        {
            Debug.LogWarning($"Invalid position: ({row}, {column})");
            return;
        }
        data.row = row;
        data.column = column;
        blocks[row, column] = data;
    }

    public void ClearBlock(int row, int column)
    {
        if (!IsValidPosition(row, column))
            return;
        blocks[row, column] = BlockData.Empty(row, column);
    }

    public bool IsValidPosition(int row, int column)
    {
        return row >= 0 && row < rowCount && column >= 0 && column < columnCount;
    }

    public bool IsEmpty(int row, int column)
    {
        if (!IsValidPosition(row, column))
            return true;
        return blocks[row, column].isEmpty;
    }

    public int GetColorAt(int row, int column)
    {
        if (!IsValidPosition(row, column) || blocks[row, column].isEmpty)
            return -1;
        return blocks[row, column].colorIndex;
    }

    public void Clear()
    {
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < columnCount; col++)
            {
                blocks[row, col] = BlockData.Empty(row, col);
            }
        }
    }
}
