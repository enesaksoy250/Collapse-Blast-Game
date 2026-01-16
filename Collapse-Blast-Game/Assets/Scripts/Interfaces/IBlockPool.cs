public interface IBlockPool
{
    BlockView Get();
    void Return(BlockView block);
    void Prewarm(int count);
    void Clear();
}
