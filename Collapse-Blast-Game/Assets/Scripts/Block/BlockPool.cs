using System.Collections.Generic;
using UnityEngine;

public sealed class BlockPool : MonoBehaviour, IBlockPool
{
    [SerializeField]
    private GameObject blockPrefab;

    [SerializeField]
    private Transform poolParent;

    private Queue<Block> availableBlocks;
    private List<Block> allBlocks;

    public void Initialize(GameObject prefab, int initialSize)
    {
        blockPrefab = prefab;
        availableBlocks = new Queue<Block>(initialSize);
        allBlocks = new List<Block>(initialSize);

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
            Block block = CreateNewBlock();
            block.Reset();
            availableBlocks.Enqueue(block);
        }
    }

    public Block Get()
    {
        Block block;

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

    public void Return(Block block)
    {
        if (block == null)
            return;

        block.Reset();
        block.transform.SetParent(poolParent);
        availableBlocks.Enqueue(block);
    }

    private Block CreateNewBlock()
    {
        if (blockPrefab == null)
        {
            Debug.LogError("BlockPool: Block prefab is not assigned!");
            return null;
        }

        GameObject blockObj = Instantiate(blockPrefab, poolParent);
        Block block = blockObj.GetComponent<Block>();

        if (block == null)
        {
            block = blockObj.AddComponent<Block>();
        }

        allBlocks.Add(block);
        return block;
    }

    public void Clear()
    {
        foreach (Block block in allBlocks)
        {
            if (block != null && block.gameObject != null)
            {
                Destroy(block.gameObject);
            }
        }

        availableBlocks.Clear();
        allBlocks.Clear();
    }

    private void OnDestroy()
    {
        Clear();
    }
}
