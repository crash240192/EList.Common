using Microsoft.Extensions.Configuration;

namespace EList.Common.Configuration
{
    public class AppSettingsContainer
    {
        private IConfiguration AppSettings { get; set; }

        public AppSettingsContainer(IConfiguration appSettings)
        {
            AppSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        }

        public string this[string key]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentNullException(nameof(key));

                var value = AppSettings[key];
                if (value == null)
                    throw new ApplicationException($"Value was not found in configuration file; key = '{key}'");

                return value;
            }
        }

        public bool Contains(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            return AppSettings[key] != null;
        }

        public bool ContainsSection(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            return AppSettings.GetSection(key).Exists();
        }

        public IConfigurationSection GetSection(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            return AppSettings.GetSection(key);
        }

        public T GetSection<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (!ContainsSection(key))
                return default;

            return AppSettings.GetSection(key).Get<T>();
        }

        public string GetSectionAsJson(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (!ContainsSection(key))
                return "{}";

            var configSection = GetSection(key);
            var sectionContent = GetSectionContent(configSection);

            return $"{{\n{sectionContent}}}";
        }

        private string GetSectionContent(IConfigurationSection configSection)
        {
            string result = "";

            foreach (var section in configSection.GetChildren())
            {
                result += $"\"{section.Key}\":";

                if (section.Value == null)
                {
                    string subSectionContent = GetSectionContent(section);
                    result += $"{{\n{subSectionContent}}},\n";
                }
                else
                {
                    result += $"\"{section.Value}\",\n";
                }
            }

            return result;
        }

        public T GetValue<T>(string key) where T : new()
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var value = new T();
            AppSettings.Bind(key, value);
            if (value == null)
                throw new ApplicationException($"Value was not found in configuration file; key = '{key}'");

            return value;
        }
    }
}