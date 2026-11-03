using System;

namespace EList.Cache.Redis
{
    public class RedisCacheOptions
    {
        public TimeSpan ExpirationTime { get; set; }
        public string RedisHost { get; set; }
        public int? SyncTimeout { get; set; }
        public string Password { get; set; }

        public string GetConnectionString()
        {
            var connectionString = $"{RedisHost}";

            if (!string.IsNullOrWhiteSpace(Password))
                connectionString += $",password={Password}";

            if (SyncTimeout != null)
                connectionString += $",syncTimeout={SyncTimeout}";

            return connectionString;
        }
    }
}