using EList.Common.Configuration;
using NLog;
using System.Linq;
using StackExchange.Redis;
using EList.Common.Logger;

namespace EList.Cache.Base
{
    /// <summary>
    /// Вспомогательное раширение по работае с кэшами
    /// </summary>
    public static class KeyBuilder
    {
        #region logger
        private static readonly ILoggerWrapper logger = new NLogLoggerWrapper(LogManager.GetCurrentClassLogger());
        private const string LOGGER_NAME = "EList.Cache.";
        #endregion

        private const string APPSETTINGS_KEY = "cache:instanceKey";

        private const string DEFAULT_INSTANCE_KEY = "default";

        private const char CACHE_LEVEL_SEPARATOR = '/';

        private static string instanceKeyValue = null;

        public static string InstanceKey
        {
            get
            {
                if (instanceKeyValue == null)
                {
                    if (ConfigurationManager.AppSettings.Contains(APPSETTINGS_KEY))
                    {
                        instanceKeyValue = ConfigurationManager.AppSettings[APPSETTINGS_KEY];
                    }
                    else
                    {
                        logger.Warn(null, null, $"{LOGGER_NAME}{nameof(InstanceKey)}",
                            $"Missing configuration key: '{APPSETTINGS_KEY}'");

                        instanceKeyValue = DEFAULT_INSTANCE_KEY;
                    }
                }

                return instanceKeyValue;
            }
        }

        /// <summary>
        /// Собрать ключ кэша
        /// </summary>
        /// <param name="key1">Ключ кеша 1-го уровня</param>
        /// <param name="key2">Ключ кеша 2-го уровня</param>
        /// <returns></returns>
        public static string Build(string key1, string key2 = null)
        {
            var keys = new[] { InstanceKey, key1, key2 };
            return string.Join(CACHE_LEVEL_SEPARATOR, keys.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        public static string WithoutInstanceKey(this RedisKey key)
        {
            return string.IsNullOrEmpty(InstanceKey) 
                ? (string) key 
                : key.ToString().Replace($"{InstanceKey}/", string.Empty);
        }
    }
}
