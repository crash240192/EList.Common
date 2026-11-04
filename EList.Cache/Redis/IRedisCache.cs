using System;
using System.Threading.Tasks;
using EList.Cache.Base;

namespace EList.Cache.Redis
{
    public interface IRedisCache : ITwoLevelCache
    {
        void Add<T>(string key, T value, TimeSpan expiration) where T : class;

        Task AddAsync<T>(string key, T value, TimeSpan expiration) where T : class;
    }
}