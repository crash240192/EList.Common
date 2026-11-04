using System;
using System.Threading.Tasks;

namespace EList.Cache.Base
{
    public interface ITwoLevelCache : ICache
    {
        T GetOrUpdate<T>(string key, Func<CacheContent<T>> valueFunc, bool forceUpdate = false, Func<T, bool> cacheDataCriteriaFunc = null)
            where T : class;

        Task<T> GetOrUpdateAsync<T>(string key, Func<Task<CacheContent<T>>> valueFunc, bool forceUpdate = false, Func<T, Task<bool>> cacheDataCriteriaFunc = null)
            where T : class;

        T GetOrUpdate<T>(string firstKey, string secondKey, Func<CacheContent<T>> valueFunc, bool forceUpdate = false, Func<T, bool> cacheDataCriteriaFunc = null)
            where T : class;

        Task<T> GetOrUpdateAsync<T>(string firstKey, string secondKey, Func<Task<CacheContent<T>>> valueFunc, bool forceUpdate = false, Func<T, Task<bool>> cacheDataCriteriaFunc = null)
            where T : class;

        new T Get<T>(string key) where T : class;

        Task<T> GetAsync<T>(string key) where T : class;

        T Get<T>(string firstKey, string secondKey) where T : class;

        new void Add<T>(string key, T value) where T : class;

        Task AddAsync<T>(string key, T value) where T : class;

        void Add<T>(string firstKey, string secondKey, T value) where T : class;

        int GetFirstLevelKeysCount();

        int GetSecondLevelKeysCount(string key);

        void Invalidate(string key);

        Task InvalidateAsync(string key);

        void Invalidate(string firstKey, string secondKey);

        Task InvalidateAsync(string firstKey, string secondKey);
    }
}
