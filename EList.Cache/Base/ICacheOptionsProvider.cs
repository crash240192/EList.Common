namespace EList.Cache.Base
{
    public interface ICacheOptionsProvider<T>
    {
        T Get();
    }
}
