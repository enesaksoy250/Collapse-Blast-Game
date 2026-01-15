using System.Collections.Generic;
using UnityEngine;
using CollapseBlast.Config;
using CollapseBlast.Data;
using CollapseBlast.Block;
using CollapseBlast.Core;
using CollapseBlast.Interfaces;

namespace CollapseBlast.Managers
{
    /// <summary>
    /// Manages the game grid - both logical data and visual representation.
    /// Coordinates between GridData and BlockViews.
    /// </summary>
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

        // Properties
        public GridData GridData => gridData;
        public int RowCount => rowCount;
        public int ColumnCount => columnCount;
        public int ColorCount => colorCount;
        public GroupFinder GroupFinder => groupFinder;
        public BlockColorData[] ColorDataArray => colorDataArray;

        /// <summary>
        /// Initializes the grid manager with configuration.
        /// </summary>
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

            // Initialize data structures
            gridData = new GridData(rowCount, columnCount);
            blockViews = new BlockView[rowCount, columnCount];
            groupFinder = new GroupFinder(gridData);

            // Initialize block pool
            int poolSize = Mathf.CeilToInt(rowCount * columnCount * 1.5f);
            blockPool.Initialize(blockPrefab, poolSize);

            // Create grid parent if not set
            if (gridParent == null)
            {
                GameObject gridParentObj = new GameObject("GridParent");
                gridParentObj.transform.SetParent(transform);
                gridParent = gridParentObj.transform;
            }
        }

        /// <summary>
        /// Creates the initial grid with random blocks.
        /// </summary>
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

        /// <summary>
        /// Creates a single block at the specified position.
        /// </summary>
        public void CreateBlock(int row, int column, int colorIndex)
        {
            // Create block data
            BlockData blockData = new BlockData(colorIndex, row, column);
            gridData.SetBlock(row, column, blockData);

            // Create block view
            BlockView blockView = blockPool.Get();
            if (blockView == null)
                return;

            blockView.transform.SetParent(gridParent);
            blockView.Initialize(colorIndex, row, column, colorDataArray[colorIndex]);
            blockView.SetWorldPosition(GridToWorldPosition(row, column));
            blockViews[row, column] = blockView;
        }

        /// <summary>
        /// Removes a block at the specified position.
        /// </summary>
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

        /// <summary>
        /// Removes multiple blocks at once.
        /// </summary>
        public void RemoveBlocks(List<Vector2Int> positions)
        {
            foreach (Vector2Int pos in positions)
            {
                RemoveBlock(pos.y, pos.x);
            }
        }

        /// <summary>
        /// Moves a block from one position to another.
        /// </summary>
        public void MoveBlock(int fromRow, int fromCol, int toRow, int toCol, bool animate = true)
        {
            BlockView blockView = blockViews[fromRow, fromCol];
            if (blockView == null)
                return;

            // Update data
            BlockData blockData = gridData.GetBlock(fromRow, fromCol);
            gridData.ClearBlock(fromRow, fromCol);
            blockData.row = toRow;
            blockData.column = toCol;
            gridData.SetBlock(toRow, toCol, blockData);

            // Update view
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

        /// <summary>
        /// Gets the block view at the specified position.
        /// </summary>
        public BlockView GetBlockView(int row, int column)
        {
            if (!gridData.IsValidPosition(row, column))
                return null;
            return blockViews[row, column];
        }

        /// <summary>
        /// Converts grid coordinates to world position.
        /// </summary>
        public Vector3 GridToWorldPosition(int row, int column)
        {
            float blockSize = gameConfig.blockSize;
            float spacing = gameConfig.blockSpacing;
            float totalSize = blockSize + spacing;

            // Calculate offset to center the grid
            float offsetX = -(columnCount * totalSize) / 2f + totalSize / 2f;
            float offsetY = -(rowCount * totalSize) / 2f + totalSize / 2f;

            float x = column * totalSize + offsetX;
            float y = row * totalSize + offsetY;

            return gridParent.position + new Vector3(x, y, 0);
        }

        /// <summary>
        /// Gets the spawn position for a new block (above the grid).
        /// </summary>
        public Vector3 GetSpawnPosition(int column, int offsetFromTop)
        {
            return GridToWorldPosition(rowCount + offsetFromTop, column);
        }

        /// <summary>
        /// Clears all blocks from the grid.
        /// </summary>
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

        /// <summary>
        /// Updates the icon state of a block based on group size.
        /// </summary>
        public void UpdateBlockIconState(int row, int column, IconState state)
        {
            BlockView blockView = blockViews[row, column];
            if (blockView != null)
            {
                blockView.UpdateIconState(state);
            }
        }

        /// <summary>
        /// Gets all active block views.
        /// </summary>
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

        /// <summary>
        /// Spawns a new block at the given position (from pool).
        /// </summary>
        public BlockView SpawnBlock(int row, int column, int colorIndex, Vector3 spawnPosition)
        {
            // Create block data
            BlockData blockData = new BlockData(colorIndex, row, column);
            gridData.SetBlock(row, column, blockData);

            // Create block view
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
}
