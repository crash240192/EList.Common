using System;
using System.Threading;
using System.Threading.Tasks;
using EList.Cache.Base;
using StackExchange.Redis;

namespace EList.Cache.Redis
{
    public class AsyncSendRedisCache : RedisCache
    {
        public AsyncSendRedisCache(ICacheOptionsProvider<RedisCacheOptions> cacheOptionsProvider) : base(cacheOptionsProvider)
        {
        }

        protected override void StringSetInternal(RedisKey key, RedisValue value, TimeSpan? expiry = null)
        {
            var t= Task.Run(async () =>
            {
                var dataBase = connectionMultiplexer.GetDatabase();
                if (!await dataBase.KeyExistsAsync(key))
                {
                    var lockObj = locks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));
                    await lockObj.WaitAsync();

                    try
                    {
                        if (!await dataBase.KeyExistsAsync(key))
                            await dataBase.StringSetAsync(key, value, expiry);
                    }
                    finally
                    {
                        lockObj.Release();
                    }
                }
            });
        }
    }
}
