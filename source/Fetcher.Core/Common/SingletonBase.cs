namespace artm.Fetcher.Core.Common
{
    public abstract class SingletonBase<T>
        where T : SingletonBase<T>
    {
        public static readonly T Instance = default(T);
    }
}