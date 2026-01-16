using System.Collections.Generic;
using UnityEngine;

public class BlockPool : MonoBehaviour, IBlockPool
{
    [SerializeField]
    private GameObject blockPrefab;

    [SerializeField]
    private Transform poolParent;

    private Queue<BlockView> availableBlocks;
    private List<BlockView> allBlocks;
    private int initialPoolSize;

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

    public void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            BlockView block = CreateNewBlock();
            block.Reset();
            availableBlocks.Enqueue(block);
        }
    }

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

    public void Return(BlockView block)
    {
        if (block == null)
            return;

        block.Reset();
        block.transform.SetParent(poolParent);
        availableBlocks.Enqueue(block);
    }

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

    public int AvailableCount => availableBlocks.Count;
    public int TotalCount => allBlocks.Count;

    private void OnDestroy()
    {
        Clear();
    }
}
