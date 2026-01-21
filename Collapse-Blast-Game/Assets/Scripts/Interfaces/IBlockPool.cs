public interface IBlockPool
{
    Block Get();
    void Return(Block block);
    void Prewarm(int count);
    void Clear();
}
