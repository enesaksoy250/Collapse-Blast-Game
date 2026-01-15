using System.Collections.Generic;
using UnityEngine;
using CollapseBlast.Interfaces;

namespace CollapseBlast.Block
{
    /// <summary>
    /// Object pool for BlockView instances.
    /// Minimizes garbage collection by reusing block objects.
    /// </summary>
    public class BlockPool : MonoBehaviour, IBlockPool
    {
        [SerializeField]
        private GameObject blockPrefab;

        [SerializeField]
        private Transform poolParent;

        private Queue<BlockView> availableBlocks;
        private List<BlockView> allBlocks;
        private int initialPoolSize;

        /// <summary>
        /// Initializes the pool with references.
        /// </summary>
        public void Initialize(GameObject prefab, int initialSize)
        {
            blockPrefab = prefab;
            initialPoolSize = initialSize;
            availableBlocks = new Queue<BlockView>(initialSize);
            allBlocks = new List<BlockView>(initialSize);

            if (poolParent == null)
            {
                GameObject poolParentObj = new GameObject("BlockPool");
                poolParentObj.transform.SetParent(transform);
                poolParent = poolParentObj.transform;
            }

            Prewarm(initialSize);
        }

        /// <summary>
        /// Pre-warms the pool by creating blocks in advance.
        /// </summary>
        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                BlockView block = CreateNewBlock();
                block.Reset();
                availableBlocks.Enqueue(block);
            }
        }

        /// <summary>
        /// Gets a block from the pool or creates a new one if pool is empty.
        /// </summary>
        public BlockView Get()
        {
            BlockView block;

            if (availableBlocks.Count > 0)
            {
                block = availableBlocks.Dequeue();
            }
            else
            {
                block = CreateNewBlock();
            }

            return block;
        }

        /// <summary>
        /// Returns a block to the pool for reuse.
        /// </summary>
        public void Return(BlockView block)
        {
            if (block == null)
                return;

            block.Reset();
            block.transform.SetParent(poolParent);
            availableBlocks.Enqueue(block);
        }

        /// <summary>
        /// Creates a new block instance.
        /// </summary>
        private BlockView CreateNewBlock()
        {
            if (blockPrefab == null)
            {
                Debug.LogError("BlockPool: Block prefab is not assigned!");
                return null;
            }

            GameObject blockObj = Instantiate(blockPrefab, poolParent);
            BlockView block = blockObj.GetComponent<BlockView>();

            if (block == null)
            {
                block = blockObj.AddComponent<BlockView>();
            }

            allBlocks.Add(block);
            return block;
        }

        /// <summary>
        /// Clears all blocks from the pool.
        /// </summary>
        public void Clear()
        {
            foreach (BlockView block in allBlocks)
            {
                if (block != null && block.gameObject != null)
                {
                    Destroy(block.gameObject);
                }
            }

            availableBlocks.Clear();
            allBlocks.Clear();
        }

        /// <summary>
        /// Returns all active blocks to the pool.
        /// </summary>
        public void ReturnAllBlocks()
        {
            foreach (BlockView block in allBlocks)
            {
                if (block != null && block.IsActive)
                {
                    Return(block);
                }
            }
        }

        /// <summary>
        /// Gets the count of available blocks in the pool.
        /// </summary>
        public int AvailableCount => availableBlocks.Count;

        /// <summary>
        /// Gets the total count of blocks managed by this pool.
        /// </summary>
        public int TotalCount => allBlocks.Count;

        private void OnDestroy()
        {
            Clear();
        }
    }
}
