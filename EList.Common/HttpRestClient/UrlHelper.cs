using NLog;

namespace EList.Common.HttpRestClient
{
    public static class UrlHelper
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public static string BuildUrl(string baseUrl, string url)
        {
            logger.Debug($"Build url: baseUrl = '{baseUrl}'");
            logger.Debug($"Build url: url = '{url}'");

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                logger.Debug($"Build url: result = '{url}'");
                return url;
            }

            baseUrl = baseUrl.TrimEnd('/');
            url = url.TrimStart('/');
            var result = $"{baseUrl}/{url}";

            logger.Debug($"Build url: result = '{result}'");
            return result;
        }
    }
}
