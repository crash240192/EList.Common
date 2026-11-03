using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EList.Cache.Base
{
    public class DisabledCache : ITwoLevelCache
    {
        public void Add<T>(string key, T value) where T : class
        {
        }

        public Task AddAsync<T>(string key, T value) where T : class
        {
            return Task.CompletedTask;
        }

        public void Add<T>(string firstKey, string secondKey, T value) where T : class
        {
        }

        public void Clear()
        {
        }

        public T Get<T>(string key) where T : class
        {
            return default;
        }

        public Task<T> GetAsync<T>(string key) where T : class
        {
            return default;
        }

        public T Get<T>(string firstKey, string secondKey) where T : class
        {
            return default;
        }

        public int GetFirstLevelKeysCount()
        {
            return 0;
        }

        public T GetOrUpdate<T>(string key, Func<CacheContent<T>> valueFunc, bool forceUpdate = false, Func<T, bool> cacheDataCriteriaFunc = null)
            where T : class
        {
            return valueFunc().Content;
        }

        public async Task<T> GetOrUpdateAsync<T>(string key, Func<Task<CacheContent<T>>> valueFunc, bool forceUpdate = false, Func<T, Task<bool>> cacheDataCriteriaFunc = null)
            where T : class
        {
            var result = await valueFunc();
            return result?.Content;
        }

        public T GetOrUpdate<T>(string firstKey, string secondKey, Func<CacheContent<T>> valueFunc, bool forceUpdate = false, Func<T, bool> cacheDataCriteriaFunc = null)
            where T : class
        {
            return valueFunc().Content;
        }

        public async Task<T> GetOrUpdateAsync<T>(string firstKey, string secodKey, Func<Task<CacheContent<T>>> valueFunc, bool forceUpdate = false, Func<T, Task<bool>> cacheDataCriteriaFunc = null)
            where T : class
        {
            var result = await valueFunc();
            return result?.Content;
        }

        public int GetSecondLevelKeysCount(string key)
        {
            return 0;
        }

        public void Invalidate(string key)
        {
        }

        public Task InvalidateAsync(string key)
        {
            return Task.CompletedTask;
        }

        public void Invalidate(string firstKey, string secondKey)
        {
        }

        public Task InvalidateAsync(string firstKey, string secondKey)
        {
            return Task.CompletedTask;
        }

        public void InvalidateByKeyPart(string keyPart)
        {
        }

        public Task<Dictionary<string, T>> GetValuesByKeyPartAsync<T>(string keyPart) => Task.FromResult(new Dictionary<string, T>());

        void ICache.Add<T>(string key, T value)
        {
        }

        T ICache.Get<T>(string key)
        {
            return default;
        }

        T ICache.GetOrUpdate<T>(string key, Func<CacheContent<T>> valueFunc)
        {
            return valueFunc().Content;
        }
    }
}
