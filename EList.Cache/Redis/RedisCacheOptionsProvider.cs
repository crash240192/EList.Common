using EList.Cache.Base;
using EList.Common.Configuration;
using System;

namespace EList.Cache.Redis
{
    public class RedisCacheOptionsProvider : ICacheOptionsProvider<RedisCacheOptions>
    {
        private readonly RedisCacheOptions redisCacheOptions;

        public RedisCacheOptionsProvider()
        {
            redisCacheOptions = new RedisCacheOptions
            {
                ExpirationTime =
                    ConfigurationManager.AppSettings.Contains("cache:redis:defaultTimeout") ?
                        TimeSpan.Parse(ConfigurationManager.AppSettings["cache:redis:defaultTimeout"]) :
                        DefaultCacheTimeouts.RedisDefaultTimeout,

                RedisHost =
                    ConfigurationManager.AppSettings.Contains("cache:redis:host") ?
                        ConfigurationManager.AppSettings["cache:redis:host"] :
                        null,

                Password = ConfigurationManager.AppSettings.Contains("cache:redis:password") ?
                        ConfigurationManager.AppSettings["cache:redis:password"] :
                        null,

                SyncTimeout = GetSyncTimeout()
            };
        }


        private int? GetSyncTimeout()
        {
            if (ConfigurationManager.AppSettings.Contains("cache:redis:syncTimeout"))
                return int.Parse(ConfigurationManager.AppSettings["cache:redis:syncTimeout"]);
            else
                return null;
        }

        public RedisCacheOptions Get()
        {
            return redisCacheOptions;
        }
    }
}