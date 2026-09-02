using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace EList.Common.Configuration
{
    public static class ConfigurationManager
    {
        private static readonly object SyncRoot = new();
        private static bool _initialized;

        public static AppSettingsContainer AppSettings { get; private set; }

        static ConfigurationManager()
        {
            EnsureInitialized();
            //try
            //{
            //    var builder = new ConfigurationBuilder()
            //        .SetBasePath(Directory.GetCurrentDirectory());

            //    if (File.Exists("appsettings.Production.json"))
            //        builder = builder.AddJsonFile("appsettings.Production.json");
            //    else
            //        builder = builder.AddJsonFile("appsettings.json");

            //    AppSettings = new AppSettingsContainer(builder.Build());
            //}
            //catch (Exception ex)
            //{
            //    throw new ApplicationException("Unable to read appsettings file", ex);
            //}
        }

        /// <summary>
        /// Подключает конфигурацию ASP.NET Core host (json + env + secrets + CLI).
        /// Вызывать из Program.cs сразу после WebApplication.CreateBuilder.
        /// </summary>
        public static void Initialize(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            lock (SyncRoot)
            {
                AppSettings = new AppSettingsContainer(configuration);
                _initialized = true;
            }
        }
        
        private static void EnsureInitialized()
        {
            if (_initialized)
                return;
            lock (SyncRoot)
            {
                if (_initialized)
                    return;
                AppSettings = new AppSettingsContainer(BuildFallbackConfiguration());
                _initialized = true;
            }
        }

        /// <summary>
        /// Fallback для тестов и процессов без ASP.NET host (workers, tools).
        /// </summary>
        private static IConfiguration BuildFallbackConfiguration()
        {
            try
            {
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                
                return new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddJsonFile($"appsettings.{environment}.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to read configuration", ex);
            }
        }
    }
}
