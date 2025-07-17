namespace Common.Pool
{
    public interface IPoolable
    {
        void OnGetFromPool();
        void OnReturnToPool();
    }
}