using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField]
    private GameConfig gameConfig;

    [SerializeField]
    private BlockColorData[] colorDataArray;

    [Header("References")]
    [SerializeField]
    private BlockPool blockPool;

    [SerializeField]
    private GameObject blockPrefab;

    [SerializeField]
    private Transform gridParent;

    private GridData gridData;
    private BlockView[,] blockViews;
    private GroupFinder groupFinder;
    private int rowCount;
    private int columnCount;
    private int colorCount;
    private float blockWidth;
    private float blockHeight;

    public GridData GridData => gridData;
    public int RowCount => rowCount;
    public int ColumnCount => columnCount;
    public int ColorCount => colorCount;
    public GroupFinder GroupFinder => groupFinder;
    public BlockColorData[] ColorDataArray => colorDataArray;

    public void Initialize()
    {
        if (gameConfig == null)
        {
            Debug.LogError("GridManager: GameConfig is not assigned!");
            return;
        }

        rowCount = gameConfig.rowCount;
        columnCount = gameConfig.columnCount;
        colorCount = Mathf.Min(gameConfig.colorCount, colorDataArray.Length);

        CalculateBlockDimensions();

        gridData = new GridData(rowCount, columnCount);
        blockViews = new BlockView[rowCount, columnCount];
        groupFinder = new GroupFinder(gridData);

        int poolSize = Mathf.CeilToInt(rowCount * columnCount * 1.5f);
        blockPool.Initialize(blockPrefab, poolSize);

        if (gridParent == null)
        {
            GameObject gridParentObj = new GameObject("GridParent");
            gridParentObj.transform.SetParent(transform);
            gridParent = gridParentObj.transform;
        }
    }

    private void CalculateBlockDimensions()
    {
        if (colorDataArray == null || colorDataArray.Length == 0 || colorDataArray[0] == null)
        {
            blockWidth = gameConfig.blockSize;
            blockHeight = gameConfig.blockSize;
            return;
        }

        Sprite sprite = colorDataArray[0].defaultIcon;
        if (sprite == null)
        {
            blockWidth = gameConfig.blockSize;
            blockHeight = gameConfig.blockSize;
            return;
        }

        float pixelsPerUnit = sprite.pixelsPerUnit;
        blockWidth = (sprite.rect.width / pixelsPerUnit) * gameConfig.blockSize;
        blockHeight = (sprite.rect.height / pixelsPerUnit) * gameConfig.blockSize;
    }

    public void CreateGrid()
    {
        ClearGrid();

        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < columnCount; col++)
            {
                int colorIndex = Random.Range(0, colorCount);
                CreateBlock(row, col, colorIndex);
            }
        }
    }

    public void CreateBlock(int row, int column, int colorIndex)
    {
        BlockData blockData = new BlockData(colorIndex, row, column);
        gridData.SetBlock(row, column, blockData);

        BlockView blockView = blockPool.Get();
        if (blockView == null)
            return;

        blockView.transform.SetParent(gridParent);
        blockView.Initialize(colorIndex, row, column, colorDataArray[colorIndex]);
        blockView.SetWorldPosition(GridToWorldPosition(row, column));
        blockViews[row, column] = blockView;
    }

    public void RemoveBlock(int row, int column)
    {
        BlockView blockView = blockViews[row, column];
        if (blockView != null)
        {
            blockPool.Return(blockView);
            blockViews[row, column] = null;
        }

        gridData.ClearBlock(row, column);
    }

    public void RemoveBlocks(List<Vector2Int> positions)
    {
        foreach (Vector2Int pos in positions)
        {
            RemoveBlock(pos.y, pos.x);
        }
    }

    public void MoveBlock(int fromRow, int fromCol, int toRow, int toCol, bool animate = true)
    {
        BlockView blockView = blockViews[fromRow, fromCol];
        if (blockView == null)
            return;

        BlockData blockData = gridData.GetBlock(fromRow, fromCol);
        gridData.ClearBlock(fromRow, fromCol);
        blockData.row = toRow;
        blockData.column = toCol;
        gridData.SetBlock(toRow, toCol, blockData);

        blockViews[fromRow, fromCol] = null;
        blockViews[toRow, toCol] = blockView;
        blockView.SetGridPosition(toRow, toCol);

        if (animate)
        {
            blockView.MoveTo(GridToWorldPosition(toRow, toCol), gameConfig.fallDuration);
        }
        else
        {
            blockView.SetWorldPosition(GridToWorldPosition(toRow, toCol));
        }
    }

    public BlockView GetBlockView(int row, int column)
    {
        if (!gridData.IsValidPosition(row, column))
            return null;
        return blockViews[row, column];
    }

    public Vector3 GridToWorldPosition(int row, int column)
    {
        float spacing = gameConfig.blockSpacing;
        float totalWidth = blockWidth + spacing;
        float totalHeight = blockHeight + spacing;

        float offsetX = -(columnCount * totalWidth) / 2f + totalWidth / 2f;
        float offsetY = -(rowCount * totalHeight) / 2f + totalHeight / 2f;

        float x = column * totalWidth + offsetX;
        float y = row * totalHeight + offsetY;

        return gridParent.position + new Vector3(x, y, 0);
    }

    public Vector3 GetSpawnPosition(int column, int offsetFromTop)
    {
        return GridToWorldPosition(rowCount + offsetFromTop, column);
    }

    public void ClearGrid()
    {
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < columnCount; col++)
            {
                RemoveBlock(row, col);
            }
        }

        gridData.Clear();
    }

    public void UpdateBlockIconState(int row, int column, IconState state)
    {
        BlockView blockView = blockViews[row, column];
        if (blockView != null)
        {
            blockView.UpdateIconState(state);
        }
    }

    public List<BlockView> GetAllBlockViews()
    {
        List<BlockView> blocks = new List<BlockView>();
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < columnCount; col++)
            {
                if (blockViews[row, col] != null)
                {
                    blocks.Add(blockViews[row, col]);
                }
            }
        }
        return blocks;
    }

    public BlockView SpawnBlock(int row, int column, int colorIndex, Vector3 spawnPosition)
    {
        BlockData blockData = new BlockData(colorIndex, row, column);
        gridData.SetBlock(row, column, blockData);

        BlockView blockView = blockPool.Get();
        if (blockView == null)
            return null;

        blockView.transform.SetParent(gridParent);
        blockView.Initialize(colorIndex, row, column, colorDataArray[colorIndex]);
        blockView.SetWorldPosition(spawnPosition);
        blockViews[row, column] = blockView;

        return blockView;
    }

    private void OnDestroy()
    {
        ClearGrid();
    }
}
