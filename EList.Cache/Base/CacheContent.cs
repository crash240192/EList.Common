using System;

namespace EList.Cache.Base
{
    public sealed class CacheContent<T>
    {
        public CacheContent(TimeSpan expirationTime, T content)
        {
            ExpirationTime = expirationTime;
            Content = content;
        }

        public TimeSpan ExpirationTime { get; }
        public T Content { get; }
    }
}
