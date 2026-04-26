using EList.Common.Configuration;
using EList.Localization.Properties;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace EList.Localization
{
    /// <summary>
    /// Обращение к полям json файла локализации
    /// </summary>
    public static class Localizator
    {
        /// <summary>
        /// Получение локализованного названия параметра 
        /// </summary>
        /// <param name="language">Локализация</param>
        /// <param name="propertyPath">Путь к значению</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static string GetProperty(string propertyPath, string defaultName)
        {
            byte[]? bytes = null;

            var language = GetLocalization();

            if (language == null)
                return defaultName;

            switch (language)
            {
                case Languages.russian:
                    bytes = Resources.russian;
                    break;
                    //case Languages.english: JObject.FromObject(Resources.english); break;
            }

            if (!(bytes?.Length > 0))
                throw new FileNotFoundException($"Не найден файл ресурсов для локализации {language}");

            using MemoryStream ms = new MemoryStream(bytes);
            {
                var obj = JsonSerializer.Deserialize<object>(ms);
                var str = obj?.ToString();
                var propertiesJObj = JObject.Parse(str);

                var property = propertiesJObj.SelectToken(propertyPath, false);

                if (property == null)
                    throw new ArgumentException($"Не найден параметр в файле локализации по маршруту {propertyPath}");

                return property.ToString();
            }
        }

        private static Languages? GetLocalization()
        {
            var enabled = ConfigurationManager.AppSettings.Contains("localization:enabled")
                ? bool.Parse(ConfigurationManager.AppSettings["localization:enabled"])
                : false;

            if (!enabled)
                return null;

            var curLocalization = ConfigurationManager.AppSettings.Contains("localization:type")
                ? ConfigurationManager.AppSettings["localization:type"]
                : null;

            if (string.IsNullOrWhiteSpace(curLocalization) || curLocalization == "default")
                return Languages.russian;

            switch (curLocalization.ToLower())
            {
                case "ru":
                case "russian": return Languages.russian;

                case "en":
                case "english": return Languages.english;
            }

            return Languages.russian;
        }
    }
}