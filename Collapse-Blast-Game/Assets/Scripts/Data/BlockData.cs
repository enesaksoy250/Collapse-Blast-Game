using UnityEngine;

public struct BlockData
{
    public int colorIndex;
    public int row;
    public int column;
    public bool isEmpty;

    public BlockData(int colorIndex, int row, int column)
    {
        this.colorIndex = colorIndex;
        this.row = row;
        this.column = column;
        this.isEmpty = false;
    }

    public static BlockData Empty(int row, int column)
    {
        return new BlockData
        {
            colorIndex = -1,
            row = row,
            column = column,
            isEmpty = true
        };
    }

    public Vector2Int Position => new Vector2Int(column, row);

    public override string ToString()
    {
        return isEmpty ? $"Empty({row},{column})" : $"Block({colorIndex},{row},{column})";
    }
}
