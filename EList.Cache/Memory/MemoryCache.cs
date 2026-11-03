using EList.Cache.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EList.Cache.Memory
{
    public class MemoryCache : IMemoryCache
    {
        class CacheItem
        {
            public DateTime CreatedTimestamp { get; set; }
            public TimeSpan ExpirationTime { get; set; }
            public object Content { get; set; }
        }

        private readonly object globalSyncRoot = new object();
        private readonly IDictionary<string, object> syncRoots = new Dictionary<string, object>();

        private readonly IDictionary<string, CacheItem> cache = new Dictionary<string, CacheItem>();

        private readonly ConcurrentDictionary<object, SemaphoreSlim> locks = new ConcurrentDictionary<object, SemaphoreSlim>();
        private readonly SemaphoreSlim globalSemaphore = new SemaphoreSlim(1);

        public T GetOrUpdate<T>(string key, Func<CacheContent<T>> valueFunc, bool forceUpdate = false, Func<T, bool> cacheDataCriteriaFunc = null)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                var result = valueFunc();
                return result?.Content;
            }

            CacheItem obj;
            bool found;

            lock (globalSyncRoot)
            {
                found = cache.TryGetValue(key, out obj);
            }

            var expired = found && DateTime.UtcNow > obj.CreatedTimestamp + obj.ExpirationTime;

            if (found && !expired && forceUpdate)
            {
                expired = true;
            }

            if (!found || expired)
            {
                var lockObj = GetOrCreateSyncRootObject(key);
                lock (lockObj)
                {
                    lock (globalSyncRoot)
                    {
                        found = cache.TryGetValue(key, out obj);
                    }

                    expired = found && DateTime.UtcNow > obj.CreatedTimestamp + obj.ExpirationTime;

                    if (found && !expired && forceUpdate)
                    {
                        expired = true;
                    }

                    if (!found || expired)
                    {
                        var item = valueFunc();
                        if (item == null)
                            throw new NullReferenceException($"valueFunc() has expected a result of type CacheContent<{typeof(T).Name}>, but got null");

                        lock (globalSyncRoot)
                        {
                            if (expired)
                            {
                                cache.Remove(key);
                            }

                            obj = new CacheItem { CreatedTimestamp = DateTime.UtcNow, ExpirationTime = item.ExpirationTime, Content = item };

                            if (cacheDataCriteriaFunc != null)
                            {
                                if (!cacheDataCriteriaFunc(item.Content))
                                {
                                    return ((CacheContent<T>)obj.Content).Content;
                                }
                            }

                            cache.Add(key, obj);
                        }
                    }
                }
            }

            return ((CacheContent<T>)obj.Content).Content;
        }

        public async Task<T> GetOrUpdateAsync<T>(string key, Func<Task<CacheContent<T>>> valueFunc, bool forceUpdate = false, Func<T, Task<bool>> cacheDataCriteriaFunc = null)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                var result = await valueFunc();
                return result?.Content;
            }

            CacheItem obj;
            bool found;

            await globalSemaphore.WaitAsync();

            try
            {
                found = cache.TryGetValue(key, out obj);
            }
            finally
            {
                globalSemaphore.Release();
            }

            var expired = found && DateTime.UtcNow > obj.CreatedTimestamp + obj.ExpirationTime;

            if (found && !expired && forceUpdate)
            {
                expired = true;
            }

            if (!found || expired)
            {
                var lockObj = locks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));
                await lockObj.WaitAsync();
                try
                {
                    await globalSemaphore.WaitAsync();

                    try
                    {
                        found = cache.TryGetValue(key, out obj);
                    }
                    finally
                    {
                        globalSemaphore.Release();
                    }

                    expired = found && DateTime.UtcNow > obj.CreatedTimestamp + obj.ExpirationTime;

                    if (found && !expired && forceUpdate)
                    {
                        expired = true;
                    }

                    if (!found || expired)
                    {
                        var item = await valueFunc();
                        if (item == null)
                            throw new NullReferenceException(
                                $"valueFunc() has expected a result of type CacheContent<{typeof(T).Name}>, but got null");

                        await globalSemaphore.WaitAsync();

                        try
                        {
                            if (expired)
                            {
                                cache.Remove(key);
                            }

                            obj = new CacheItem
                            {
                                CreatedTimestamp = DateTime.UtcNow, ExpirationTime = item.ExpirationTime, Content = item
                            };

                            if (cacheDataCriteriaFunc != null)
                            {
                                if (!await cacheDataCriteriaFunc(item.Content))
                                {
                                    return ((CacheContent<T>)obj.Content).Content;
                                }
                            }

                            cache.Add(key, obj);
                        }
                        finally
                        {
                            globalSemaphore.Release();
                        }
                    }
                }
                finally
                {
                    lockObj.Release();
                }
            }

            return ((CacheContent<T>)obj.Content).Content;
        }

        // TODO: remove this method, use method above
        T ICache.GetOrUpdate<T>(string key, Func<CacheContent<T>> valueFunc)
        {
            CacheItem obj;
            bool found;

            lock (globalSyncRoot)
            {
                found = cache.TryGetValue(key, out obj);
            }

            var expired = found && DateTime.UtcNow > obj.CreatedTimestamp + obj.ExpirationTime;

            if (!found || expired)
            {
                var lockObj = GetOrCreateSyncRootObject(key);
                lock (lockObj)
                {
                    lock (globalSyncRoot)
                    {
                        found = cache.TryGetValue(key, out obj);
                    }

                    expired = found && DateTime.UtcNow > obj.CreatedTimestamp + obj.ExpirationTime;

                    if (!found || expired)
                    {
                        var item = valueFunc();
                        if (item == null)
                            throw new NullReferenceException($"valueFunc() has expected a result of type CacheContent<{typeof(T).Name}>, but got null");

                        lock (globalSyncRoot)
                        {
                            if (expired)
                            {
                                cache.Remove(key);
                            }

                            obj = new CacheItem { CreatedTimestamp = DateTime.UtcNow, ExpirationTime = item.ExpirationTime, Content = item };
                            cache.Add(key, obj);
                        }
                    }
                }
            }

            return ((CacheContent<T>)obj.Content).Content;
        }

        public T GetOrUpdate<T>(string firstKey, string secondKey, Func<CacheContent<T>> valueFunc, bool forceUpdate = false, Func<T, bool> cacheDataCriteriaFunc = null)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(firstKey) || string.IsNullOrWhiteSpace(secondKey))
            {
                var result = valueFunc();
                return result?.Content;
            }

            #region Get or create first-level object
            CacheItem firstObj;
            bool firstFound;

            lock (globalSyncRoot)
            {
                firstFound = cache.TryGetValue(firstKey, out firstObj);
            }

            if (!firstFound)
            {
                lock (globalSyncRoot)
                {
                    firstFound = cache.TryGetValue(firstKey, out firstObj);

                    if (!firstFound)
                    {
                        firstObj = new CacheItem { CreatedTimestamp = DateTime.UtcNow, ExpirationTime = TimeSpan.MaxValue, Content = new Dictionary<string, CacheItem>() };
                        cache.Add(firstKey, firstObj);
                    }
                }
            }
            #endregion

            var firstCacheContent = (Dictionary<string, CacheItem>)firstObj.Content;

            CacheItem secondObj;
            bool secondFound;

            var firstLevellockObj = GetOrCreateSyncRootObject(firstKey);
            lock (firstLevellockObj)
            {
                secondFound = firstCacheContent.TryGetValue(secondKey, out secondObj);
            }

            var secondExpired = secondFound && DateTime.UtcNow > secondObj.CreatedTimestamp + secondObj.ExpirationTime;

            if (secondFound && !secondExpired && forceUpdate)
            {
                secondExpired = true;
            }

            if (!secondFound || secondExpired)
            {
                var firstPlusSecondKey = $"{firstKey}/{secondKey}";
                var secondLevelLockObj = GetOrCreateSyncRootObject(firstPlusSecondKey);
                lock (secondLevelLockObj)
                {
                    lock (firstLevellockObj)
                    {
                        secondFound = firstCacheContent.TryGetValue(secondKey, out secondObj);
                    }

                    secondExpired = secondFound && DateTime.UtcNow > secondObj.CreatedTimestamp + secondObj.ExpirationTime;

                    if (secondFound && !secondExpired && forceUpdate)
                    {
                        secondExpired = true;
                    }

                    if (!secondFound || secondExpired)
                    {
                        var item = valueFunc();
                        if (item == null)
                            throw new NullReferenceException($"valueFunc() has expected a result of type CacheContent<{typeof(T).Name}>, but got null");

                        lock (firstLevellockObj)
                        {
                            if (secondExpired)
                            {
                                firstCacheContent.Remove(secondKey);
                            }

                            secondObj = new CacheItem { CreatedTimestamp = DateTime.UtcNow, ExpirationTime = item.ExpirationTime, Content = item };

                            if (cacheDataCriteriaFunc != null)
                            {
                                if (!cacheDataCriteriaFunc(item.Content))
                                {
                                    return ((CacheContent<T>)secondObj.Content).Content;
                                }
                            }

                            firstCacheContent.Add(secondKey, secondObj);
                        }
                    }
                }
            }

            return ((CacheContent<T>)secondObj.Content).Content;
        }

        public async Task<T> GetOrUpdateAsync<T>(string firstKey, string secondKey, Func<Task<CacheContent<T>>> valueFunc, bool forceUpdate = false, Func<T, Task<bool>> cacheDataCriteriaFunc = null)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(firstKey) || string.IsNullOrWhiteSpace(secondKey))
            {
                var result = await valueFunc();
                return result?.Content;
            }

            #region Get or create first-level object
            CacheItem firstObj;
            bool firstFound;

            await globalSemaphore.WaitAsync();

            try
            {
                firstFound = cache.TryGetValue(firstKey, out firstObj);
            }
            finally
            {
                globalSemaphore.Release();
            }

            if (!firstFound)
            {
                await globalSemaphore.WaitAsync();
                try
                {
                    firstFound = cache.TryGetValue(firstKey, out firstObj);

                    if (!firstFound)
                    {
                        firstObj = new CacheItem
                        {
                            CreatedTimestamp = DateTime.UtcNow, ExpirationTime = TimeSpan.MaxValue,
                            Content = new Dictionary<string, CacheItem>()
                        };
                        cache.Add(firstKey, firstObj);
                    }
                }
                finally
                {
                    globalSemaphore.Release();
                }
            }
            #endregion

            var firstCacheContent = (Dictionary<string, CacheItem>)firstObj.Content;

            CacheItem secondObj;
            bool secondFound;

            var firstLevellockObj = locks.GetOrAdd(firstKey, k => new SemaphoreSlim(1, 1));

            await firstLevellockObj.WaitAsync();

            try
            {
                secondFound = firstCacheContent.TryGetValue(secondKey, out secondObj);
            }
            finally
            {
                firstLevellockObj.Release();
            }

            var secondExpired = secondFound && DateTime.UtcNow > secondObj.CreatedTimestamp + secondObj.ExpirationTime;

            if (secondFound && !secondExpired && forceUpdate)
            {
                secondExpired = true;
            }

            if (!secondFound || secondExpired)
            {
                var firstPlusSecondKey = $"{firstKey}/{secondKey}";
                var secondLevelLockObj = locks.GetOrAdd(firstPlusSecondKey, k => new SemaphoreSlim(1, 1));

                await secondLevelLockObj.WaitAsync();
                try
                {
                    await firstLevellockObj.WaitAsync();

                    try
                    {
                        secondFound = firstCacheContent.TryGetValue(secondKey, out secondObj);
                    }
                    finally
                    {
                        firstLevellockObj.Release();
                    }

                    secondExpired = secondFound &&
                                    DateTime.UtcNow > secondObj.CreatedTimestamp + secondObj.ExpirationTime;

                    if (secondFound && !secondExpired && forceUpdate)
                    {
                        secondExpired = true;
                    }

                    if (!secondFound || secondExpired)
                    {
                        var item = await valueFunc();
                        if (item == null)
                            throw new NullReferenceException(
                                $"valueFunc() has expected a result of type CacheContent<{typeof(T).Name}>, but got null");

                        await globalSemaphore.WaitAsync();

                        try
                        {
                            if (secondExpired)
                            {
                                firstCacheContent.Remove(secondKey);
                            }

                            secondObj = new CacheItem
                            {
                                CreatedTimestamp = DateTime.UtcNow, ExpirationTime = item.ExpirationTime, Content = item
                            };

                            if (cacheDataCriteriaFunc != null)
                            {
                                if (!await cacheDataCriteriaFunc(item.Content))
                                {
                                    return ((CacheContent<T>)secondObj.Content).Content;
                                }
                            }

                            firstCacheContent.Add(secondKey, secondObj);
                        }
                        finally
                        {
                            globalSemaphore.Release();
                        }
                    }
                }
                finally
                {
                    secondLevelLockObj.Release();
                }
            }

            return ((CacheContent<T>)secondObj.Content).Content;
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

        public void Clear()
        {
            lock (globalSyncRoot)
            {
                cache.Clear();
            }
        }

        public void InvalidateByKeyPart(string keyPart)
        {
            lock (globalSyncRoot)
            {
                var keysToRemove = cache.Keys.Where(k => k.Contains(keyPart)).ToList();
                if (keysToRemove.Any())
                {
                    foreach (var key in keysToRemove)
                    {
                        cache.Remove(key);
                    }
                }
            }
        }

        public Task<Dictionary<string, T>> GetValuesByKeyPartAsync<T>(string keyPart)
        {
            lock (globalSyncRoot)
            {
                var result = new Dictionary<string, T>();
                foreach (var x in cache)
                {
                    if (x.Key.Contains(keyPart) is false) continue;

                    if (x.Value.Content is T content)
                        result.Add(x.Key, content);
                }

                return Task.FromResult(result);
            }
        }

        public void Invalidate(string key)
        {
            lock (globalSyncRoot)
            {
                cache.Remove(key);
            }
        }

        public void Invalidate(string firstKey, string secondKey)
        {
            lock (globalSyncRoot)
            {
                var firstFound = cache.TryGetValue(firstKey, out CacheItem firstObj);
                if (firstFound)
                {
                    ((Dictionary<string, CacheItem>)firstObj.Content).Remove(secondKey);
                }
            }
        }

        public async Task InvalidateAsync(string firstKey, string secondKey)
        {
            await globalSemaphore.WaitAsync();
            try
            {
                var firstFound = cache.TryGetValue(firstKey, out var firstObj);
                if (firstFound)
                {
                    ((Dictionary<string, CacheItem>)firstObj.Content).Remove(secondKey);
                }
            }
            finally
            {
                globalSemaphore.Release();
            }
        }

        public T Get<T>(string key) where T : class
        {
            CacheItem firstObj;
            bool firstFound;

            lock (globalSyncRoot)
            {
                firstFound = cache.TryGetValue(key, out firstObj);
            }

            var firstExpired = firstFound && DateTime.UtcNow > firstObj.CreatedTimestamp + firstObj.ExpirationTime;
            if (firstFound && !firstExpired)
            {
                return ((CacheContent<T>)firstObj.Content).Content;
            }

            return default;
        }

        T ICache.Get<T>(string key)
        {
            CacheItem firstObj;
            bool firstFound;

            lock (globalSyncRoot)
            {
                firstFound = cache.TryGetValue(key, out firstObj);
            }

            var firstExpired = firstFound && DateTime.UtcNow > firstObj.CreatedTimestamp + firstObj.ExpirationTime;
            if (firstFound && !firstExpired)
            {
                return ((CacheContent<T>)firstObj.Content).Content;
            }

            return default;
        }

        public T Get<T>(string firstKey, string secondKey) where T : class
        {
            CacheItem firstObj;
            bool firstFound;

            lock (globalSyncRoot)
            {
                firstFound = cache.TryGetValue(firstKey, out firstObj);
            }

            if (firstFound)
            {
                var firstCache = (Dictionary<string, CacheItem>)firstObj.Content;

                CacheItem secondObj;
                bool secondFound;

                var firstLevelLockObj = GetOrCreateSyncRootObject(firstKey);
                lock (firstLevelLockObj)
                {
                    secondFound = firstCache.TryGetValue(secondKey, out secondObj);
                }

                var secondExpired = secondFound && DateTime.UtcNow > secondObj.CreatedTimestamp + secondObj.ExpirationTime;
                if (secondFound && !secondExpired)
                {
                    return ((CacheContent<T>)secondObj.Content).Content;
                }
            }

            return default;
        }

        public int GetFirstLevelKeysCount()
        {
            lock (globalSyncRoot)
            {
                return cache.Count();
            }
        }

        public int GetSecondLevelKeysCount(string key)
        {
            lock (globalSyncRoot)
            {
                var firstFound = cache.TryGetValue(key, out CacheItem firstObj);
                if (firstFound)
                {
                    try
                    {
                        return ((Dictionary<string, CacheItem>)firstObj.Content).Count;
                    }
                    catch { }
                }

                return 0;
            }
        }

        #region Not Implemented
        public void Add<T>(string key, T value) where T : class
        {
            throw new NotImplementedException();
        }

        void ICache.Add<T>(string key, T value)
        {
            throw new NotImplementedException();
        }

        public void Add<T>(string firstKey, string secondKey, T value) where T : class
        {
            throw new NotImplementedException();
        }
        
        public async Task InvalidateAsync(string key)
        {
            await globalSemaphore.WaitAsync();

            try
            {
                cache.Remove(key);
            }
            finally
            {
                globalSemaphore.Release();
            }
        }
        
        public async Task<T> GetAsync<T>(string key) where T : class
        {
            CacheItem firstObj;
            bool firstFound;

            await globalSemaphore.WaitAsync();
            try
            {
                firstFound = cache.TryGetValue(key, out firstObj);
            }
            finally
            {
                globalSemaphore.Release();
            }

            var firstExpired = firstFound && DateTime.UtcNow > firstObj.CreatedTimestamp + firstObj.ExpirationTime;
            if (firstFound && !firstExpired)
            {
                return ((CacheContent<T>)firstObj.Content).Content;
            }

            return default;
        }
        
        public Task AddAsync<T>(string key, T value) where T : class
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
