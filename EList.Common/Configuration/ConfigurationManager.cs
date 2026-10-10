using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Linq;

namespace EList.Common.Configuration
{
    public static class ConfigurationManager
    {
        public const string ProductionSettingsFileName = "appsettings.Production.json";

        private static readonly object SyncRoot = new();
        private static bool _initialized;

        public static AppSettingsContainer AppSettings { get; private set; }

        static ConfigurationManager()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// Подключает конфигурацию ASP.NET Core host (json + env + secrets + CLI).
        /// Вызывать из Program.cs сразу после WebApplication.CreateBuilder.
        /// </summary>
        /// <param name="configuration">Host configuration (обычно builder.Configuration).</param>
        /// <param name="contentRootPath">
        /// Content root приложения — чтобы найти серверный
        /// <c>appsettings.Production.json</c>, даже если ASPNETCORE_ENVIRONMENT ≠ Production.
        /// </param>
        public static void Initialize(IConfiguration configuration, string? contentRootPath = null)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            lock (SyncRoot)
            {
                var root = ResolveContentRoot(contentRootPath);
                var effective = EnsureProductionOverlay(configuration, root);
                AppSettings = new AppSettingsContainer(effective);
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
        /// Deploy-convention: на сервере генерируется appsettings.Production.json.
        /// После перехода на host-config оверлей грузился только при
        /// ASPNETCORE_ENVIRONMENT=Production. Если файл есть — подключаем поверх
        /// base / appsettings.{Env}.json, но ниже Environment Variables и CLI.
        /// </summary>
        private static IConfiguration EnsureProductionOverlay(IConfiguration configuration, string contentRootPath)
        {
            var productionPath = Path.Combine(contentRootPath, ProductionSettingsFileName);
            if (!File.Exists(productionPath))
                return configuration;

            if (configuration is IConfigurationBuilder hostBuilder)
            {
                if (!HasProductionJsonSource(hostBuilder))
                {
                    var source = new JsonConfigurationSource
                    {
                        Path = ProductionSettingsFileName,
                        Optional = true,
                        ReloadOnChange = true,
                        FileProvider = new PhysicalFileProvider(contentRootPath)
                    };

                    var insertAt = FindEnvVarsOrCommandLineSourceIndex(hostBuilder);
                    if (insertAt >= 0)
                        hostBuilder.Sources.Insert(insertAt, source);
                    else
                        hostBuilder.Add(source);
                }

                // Тот же объект host ConfigurationManager — источники уже обновлены.
                return configuration;
            }

            // Не host-builder: Production поверх переданного дерева.
            return new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .AddJsonFile(productionPath, optional: true, reloadOnChange: true)
                .Build();
        }

        private static bool HasProductionJsonSource(IConfigurationBuilder builder)
        {
            return builder.Sources.OfType<JsonConfigurationSource>().Any(source =>
                source.Path != null
                && string.Equals(
                    Path.GetFileName(source.Path),
                    ProductionSettingsFileName,
                    StringComparison.OrdinalIgnoreCase));
        }

        private static int FindEnvVarsOrCommandLineSourceIndex(IConfigurationBuilder builder)
        {
            for (var i = 0; i < builder.Sources.Count; i++)
            {
                var name = builder.Sources[i].GetType().Name;
                if (name.Contains("EnvironmentVariables", StringComparison.Ordinal)
                    || name.Contains("CommandLine", StringComparison.Ordinal))
                    return i;
            }

            return -1;
        }

        private static string ResolveContentRoot(string? contentRootPath)
        {
            if (!string.IsNullOrWhiteSpace(contentRootPath))
                return contentRootPath;

            return Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// Fallback для тестов и процессов без ASP.NET host (workers, tools).
        /// </summary>
        private static IConfiguration BuildFallbackConfiguration()
        {
            try
            {
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                    ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                    ?? "Production";

                var basePath = Directory.GetCurrentDirectory();
                var builder = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false);

                // Если env ≠ Production, всё равно подхватить серверный Production.json.
                if (!string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase)
                    && File.Exists(Path.Combine(basePath, ProductionSettingsFileName)))
                {
                    builder.AddJsonFile(ProductionSettingsFileName, optional: true, reloadOnChange: false);
                }

                return builder
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
