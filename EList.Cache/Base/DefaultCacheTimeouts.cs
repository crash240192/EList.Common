using System;

namespace EList.Cache.Base
{
    public static class DefaultCacheTimeouts
    {
        public static readonly TimeSpan RedisDefaultTimeout = new TimeSpan(0, 15, 0);
        public static readonly TimeSpan DictionariesOfNamesTimeout = new TimeSpan(0, 15, 0);
        public static readonly TimeSpan DictionariesOfValueSetsTimeout = new TimeSpan(0, 15, 0);
        public static readonly TimeSpan DictionariesTimeout = new TimeSpan(0, 15, 0);
        public static readonly TimeSpan OrganizationsTimeout = new TimeSpan(365, 0, 0, 0);
        public static readonly TimeSpan DistrictListTimeout = new TimeSpan(365, 0, 0, 0);
        /// <summary>
        /// Кэш ответов Геокодера Яндекса. По условиям использования — не дольше 30 суток.
        /// </summary>
        public static readonly TimeSpan GeocodeTimeout = new TimeSpan(14, 0, 0, 0);
    }
}
