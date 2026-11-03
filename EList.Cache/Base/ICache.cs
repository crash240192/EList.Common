using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EList.Cache.Base
{
    public interface ICache
    {
        [Obsolete]
        T GetOrUpdate<T>(string key, Func<CacheContent<T>> valueFunc);

        void Clear();

        T Get<T>(string key);

        void Add<T>(string key, T value);

        void InvalidateByKeyPart(string keyPart);

        Task<Dictionary<string, T>> GetValuesByKeyPartAsync<T>(string keyPart);
    }
}