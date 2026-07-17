using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace EList.Common.Configuration
{
    public static class ConfigurationManager
    {
        public static AppSettingsContainer AppSettings { get; private set; }

        static ConfigurationManager()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory());

                if (File.Exists("appsettings.Production.json"))
                    builder = builder.AddJsonFile("appsettings.Production.json");
                else
                    builder = builder.AddJsonFile("appsettings.json");

                AppSettings = new AppSettingsContainer(builder.Build());
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to read appsettings file", ex);
            }
        }
    }
}
