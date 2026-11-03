using EList.Cache.Base;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EList.Cache.Redis
{
    public class RedisCache : IRedisCache
    {
        protected readonly ICacheOptionsProvider<RedisCacheOptions> cacheOptionsProvider;

        private readonly object globalSyncRoot = new object();
        private readonly Dictionary<string, object> syncRoots = new Dictionary<string, object>();
        protected readonly ConcurrentDictionary<object, SemaphoreSlim> locks = new ConcurrentDictionary<object, SemaphoreSlim>();
        //private readonly SemaphoreSlim globalSemaphore = new SemaphoreSlim(1);

        protected readonly ConnectionMultiplexer connectionMultiplexer;

        public RedisCache(ICacheOptionsProvider<RedisCacheOptions> cacheOptionsProvider)
        {
            this.cacheOptionsProvider = cacheOptionsProvider;

            var cacheOptions = this.cacheOptionsProvider.Get();

            connectionMultiplexer = ConnectionMultiplexer.Connect(cacheOptions.GetConnectionString());
        }

        public void Add<T>(string key, T value) where T : class
        {
            var cacheOptions = cacheOptionsProvider.Get();
            Add(key, value, cacheOptions.ExpirationTime);
        }

        public Task AddAsync<T>(string key, T value) where T : class
        {
            var cacheOptions = cacheOptionsProvider.Get();
            return AddAsync(key, value, cacheOptions.ExpirationTime);
        }

        public void Add<T>(string key, T value, TimeSpan expiration) where T : class
        {
            var dataBase = connectionMultiplexer.GetDatabase();
            var cacheContent = JsonConvert.SerializeObject(value);
            key = KeyBuilder.Build(key);
            dataBase.StringSet(key, cacheContent, expiration);
        }

        public async Task AddAsync<T>(string key, T value, TimeSpan expiration) where T : class
        {
            var dataBase = connectionMultiplexer.GetDatabase();
            var cacheContent = JsonConvert.SerializeObject(value);
            key = KeyBuilder.Build(key);
            await dataBase.StringSetAsync(key, cacheContent, expiration);
        }

        void ICache.Add<T>(string key, T value)
        {
            var cacheOptions = cacheOptionsProvider.Get();

            var dataBase = connectionMultiplexer.GetDatabase();
            var cacheContent = JsonConvert.SerializeObject(value);
            key = KeyBuilder.Build(key);
            dataBase.StringSet(key, cacheContent, cacheOptions.ExpirationTime);
        }

        public void Add<T>(string firstKey, string secondKey, T value) where T : class
        {
            var cacheOptions = cacheOptionsProvider.Get();

            var dataBase = connectionMultiplexer.GetDatabase();
            var cacheContent = JsonConvert.SerializeObject(value);
            var key = KeyBuilder.Build(firstKey, secondKey);
            dataBase.StringSet(key, cacheContent, cacheOptions.ExpirationTime);
        }

        public T Get<T>(string key) where T : class
        {
            key = KeyBuilder.Build(key);

            var dataBase = connectionMultiplexer.GetDatabase();
            if (!dataBase.KeyExists(key))
                return default;
            var cacheContent = dataBase.StringGet(key);
            var cacheObject = JsonConvert.DeserializeObject<T>(cacheContent);
            return cacheObject;
        }

        public async Task<T> GetAsync<T>(string key) where T : class
        {
            key = KeyBuilder.Build(key);

            var dataBase = connectionMultiplexer.GetDatabase();
            if (!await dataBase.KeyExistsAsync(key))
                return default;
            var cacheContent = await dataBase.StringGetAsync(key);
            var cacheObject = JsonConvert.DeserializeObject<T>(cacheContent);
            return cacheObject;
        }

        T ICache.Get<T>(string key)
        {
            key = KeyBuilder.Build(key);

            var dataBase = connectionMultiplexer.GetDatabase();
            var cacheContent = dataBase.StringGet(key);
            var cacheObject = JsonConvert.DeserializeObject<T>(cacheContent);
            return cacheObject;
        }

        public T Get<T>(string firstKey, string secondKey) where T : class
        {
            var dataBase = connectionMultiplexer.GetDatabase();
            var key = KeyBuilder.Build(firstKey, secondKey);
            if (!dataBase.KeyExists(key))
                return default;
            var cacheContent = dataBase.StringGet(key);
            var cacheObject = JsonConvert.DeserializeObject<T>(cacheContent);
            return cacheObject;
        }

        public void Clear()
        {
            var cacheOptions = cacheOptionsProvider.Get();

            var dataBase = connectionMultiplexer.GetDatabase();
            var server = connectionMultiplexer.GetServer(cacheOptions.RedisHost);
            var removableKeys = server.Keys(pattern: $"*").ToArray();
            dataBase.KeyDelete(removableKeys);
        }

        public void InvalidateByKeyPart(string keyPart)
        {
            var cacheOptions = cacheOptionsProvider.Get();
            keyPart = KeyBuilder.Build(keyPart);

            var dataBase = connectionMultiplexer.GetDatabase();
            var server = connectionMultiplexer.GetServer(cacheOptions.RedisHost);
            var removableKeys = server.Keys(pattern: $"{keyPart}*").ToArray();
            dataBase.KeyDelete(removableKeys);
        }

        public async Task<Dictionary<string, T>> GetValuesByKeyPartAsync<T>(string keyPart)
        {
            var cacheOptions = cacheOptionsProvider.Get();
            keyPart = KeyBuilder.Build(keyPart);

            var dataBase = connectionMultiplexer.GetDatabase();
            var server = connectionMultiplexer.GetServer(cacheOptions.RedisHost);

            var targetKeys = await server
                .KeysAsync(pattern: $"{keyPart}*")
                .AsyncEnumerableToArrayAsync();

            var rawValues = await dataBase.StringGetAsync(targetKeys);

            var values = rawValues
                .Select(x => JsonConvert.DeserializeObject<T>(x))
                .ToArray();

            return targetKeys
                .Zip(values, (k, v) => new { key = k.WithoutInstanceKey(), value = v })
                .ToDictionary(item => item.key, item => item.value);
        }

        public void Invalidate(string key)
        {
            var cacheOptions = cacheOptionsProvider.Get();
            key = KeyBuilder.Build(key);

            var dataBase = connectionMultiplexer.GetDatabase();
            var server = connectionMultiplexer.GetServer(cacheOptions.RedisHost);
            var removableKeys = server.Keys(pattern: $"{key}").ToArray();
            dataBase.KeyDelete(removableKeys);
        }

        public async Task InvalidateAsync(string key)
        {
            var cacheOptions = cacheOptionsProvider.Get();
            key = KeyBuilder.Build(key);

            var dataBase = connectionMultiplexer.GetDatabase();
            var server = connectionMultiplexer.GetServer(cacheOptions.RedisHost);
            var removableKeys = server.Keys(pattern: $"{key}").ToArray();
            await dataBase.KeyDeleteAsync(removableKeys);
        }

        public void Invalidate(string firstKey, string secondKey)
        {
            var cacheOptions = cacheOptionsProvider.Get();
            var key = KeyBuilder.Build(firstKey, secondKey);

            var dataBase = connectionMultiplexer.GetDatabase();
            var server = connectionMultiplexer.GetServer(cacheOptions.RedisHost);
            var removableKeys = server.Keys(pattern: $"{key}").ToArray();
            dataBase.KeyDelete(removableKeys);
        }

        public async Task InvalidateAsync(string firstKey, string secondKey)
        {
            var cacheOptions = cacheOptionsProvider.Get();
            var key = KeyBuilder.Build(firstKey, secondKey);

            var dataBase = connectionMultiplexer.GetDatabase();
            var server = connectionMultiplexer.GetServer(cacheOptions.RedisHost);
            var removableKeys = server.Keys(pattern: $"{key}").ToArray();
            await dataBase.KeyDeleteAsync(removableKeys);
        }

        public T GetOrUpdate<T>(string key, Func<CacheContent<T>> valueFunc, bool forceUpdate = false, Func<T, bool> cacheDataCriteriaFunc = null)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                var result = valueFunc();
                return result?.Content;
            }

            var fullKey = KeyBuilder.Build(key);
            return GetOrUpdateInternal(fullKey, valueFunc, forceUpdate, cacheDataCriteriaFunc);
        }

        private T GetOrUpdateInternal<T>(string key, Func<CacheContent<T>> valueFunc, bool forceUpdate = false, Func<T, bool> cacheDataCriteriaFunc = null)
            where T : class
        {
            var dataBase = connectionMultiplexer.GetDatabase();

            string cacheContent;

            if (!dataBase.KeyExists(key) || forceUpdate)
            {
                var lockObj = GetOrCreateSyncRootObject(key);
                lock (lockObj)
                {
                    if (!dataBase.KeyExists(key) || forceUpdate)
                    {
                        var value = valueFunc();
                        if (value == null)
                            throw new NullReferenceException($"valueFunc() has expected a result of type CacheContent<{typeof(T).Name}>, but got null");

                        if (cacheDataCriteriaFunc != null)
                        {
                            if (!cacheDataCriteriaFunc(value.Content))
                            {
                                return value.Content;
                            }
                        }

                        cacheContent = JsonConvert.SerializeObject(value.Content);

                        dataBase.StringSet(key, cacheContent, value.ExpirationTime);
                        return value.Content;
                    }
                }
            }

            cacheContent = dataBase.StringGet(key);
            var cacheObject = JsonConvert.DeserializeObject<T>(cacheContent);
            return cacheObject;
        }

        public async Task<T> GetOrUpdateAsync<T>(string key, Func<Task<CacheContent<T>>> valueFunc, bool forceUpdate = false, Func<T, Task<bool>> cacheDataCriteriaFunc = null)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                var result = await valueFunc();
                return result?.Content;
            }

            var fullKey = KeyBuilder.Build(key);
            return await GetOrUpdateInternalAsync(fullKey, valueFunc, forceUpdate, cacheDataCriteriaFunc);
        }

        private async Task<T> GetOrUpdateInternalAsync<T>(string key, Func<Task<CacheContent<T>>> valueFunc, bool forceUpdate = false, Func<T, Task<bool>> cacheDataCriteriaFunc = null)
            where T : class
        {
            var dataBase = connectionMultiplexer.GetDatabase();
            var cacheContent = default(string);
            if (!await dataBase.KeyExistsAsync(key) || forceUpdate)
            {
                var lockObj = locks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));
                await lockObj.WaitAsync();

                try
                {
                    if (!dataBase.KeyExists(key) || forceUpdate)
                    {
                        var value = await valueFunc();
                        if (value == null)
                            throw new NullReferenceException(
                                $"valueFunc() has expected a result of type CacheContent<{typeof(T).Name}>, but got null");

                        if (cacheDataCriteriaFunc != null)
                        {
                            if (!await cacheDataCriteriaFunc(value.Content))
                            {
                                return value.Content;
                            }
                        }

                        cacheContent = JsonConvert.SerializeObject(value.Content);
                        StringSetInternal(key, cacheContent, value.ExpirationTime);
                        return value.Content;
                    }
                }
                finally
                {
                    lockObj.Release();
                }
            }

            cacheContent = await dataBase.StringGetAsync(key);
            var cacheObject = JsonConvert.DeserializeObject<T>(cacheContent);
            return cacheObject;
        }

        protected virtual void StringSetInternal(RedisKey key, RedisValue value, TimeSpan? expiry = null)
        {
            var dataBase = connectionMultiplexer.GetDatabase();
            dataBase.StringSet(key, value, expiry);
        }

        // TODO: remove this method, use method above
        T ICache.GetOrUpdate<T>(string key, Func<CacheContent<T>> valueFunc)
        {
            throw new NotImplementedException();
        }

        public T GetOrUpdate<T>(string firstKey, string secondKey, Func<CacheContent<T>> valueFunc, bool forceUpdate = false, Func<T, bool> cacheDataCriteriaFunc = null)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(firstKey) || string.IsNullOrWhiteSpace(secondKey))
            {
                var result = valueFunc();
                return result?.Content;
            }

            var fullKey = KeyBuilder.Build(firstKey, secondKey);
            return GetOrUpdateInternal(fullKey, valueFunc, forceUpdate, cacheDataCriteriaFunc);
        }

        public async Task<T> GetOrUpdateAsync<T>(string firstKey, string secondKey, Func<Task<CacheContent<T>>> valueFunc, bool forceUpdate = false, Func<T, Task<bool>> cacheDataCriteriaFunc = null)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(firstKey) || string.IsNullOrWhiteSpace(secondKey))
            {
                var result = await valueFunc();
                return result?.Content;
            }

            var fullKey = KeyBuilder.Build(firstKey, secondKey);
            return await GetOrUpdateInternalAsync(fullKey, valueFunc, forceUpdate, cacheDataCriteriaFunc);
        }

        private object GetOrCreateSyncRootObject(string key)
        {
            lock (globalSyncRoot)
            {
                var found = syncRoots.TryGetValue(key, out object result);
                if (!found)
                {
                    result = new object();
                    syncRoots.Add(key, result);
                }

                return result;
            }
        }

        public int GetFirstLevelKeysCount()
        {
            var cacheOptions = cacheOptionsProvider.Get();
            var patternFirstKeys = KeyBuilder.Build("*");
            var patternSecondKeys = KeyBuilder.Build("*", "*");

            var server = connectionMultiplexer.GetServer(cacheOptions.RedisHost);

            var firstKeys = server.Keys(pattern: patternFirstKeys).ToList();
            var secondKeys = server.Keys(pattern: patternSecondKeys).ToList();

            var firstKeysCount = firstKeys.Count();
            var secondKeysCount = secondKeys.Count();

            var distinсtFirstKeys = new List<string>();
            var regexFirstKeys = new Regex($"^{KeyBuilder.InstanceKey}/(.*)/(?:.*)");

            foreach (var secondKey in secondKeys)
            {
                var secondKeyValue = secondKey.ToString();
                var firstKeyName = regexFirstKeys.Replace(secondKeyValue, "$1");

                if (!distinсtFirstKeys.Contains(firstKeyName))
                    distinсtFirstKeys.Add(firstKeyName);
            }

            return firstKeysCount - secondKeysCount + distinсtFirstKeys.Count;
        }

        public int GetSecondLevelKeysCount(string firstKey)
        {
            var cacheOptions = cacheOptionsProvider.Get();
            var patternSecondKeys = KeyBuilder.Build(firstKey, "*");

            var server = connectionMultiplexer.GetServer(cacheOptions.RedisHost);
            var secondKeysCount = server.Keys(pattern: patternSecondKeys).Count();
            return secondKeysCount;

        }
    }
}