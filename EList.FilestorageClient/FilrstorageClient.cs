using EList.Common.Configuration;
using EList.Common.CorrelationId;
using EList.Common.HttpRestClient;
using EList.Common.Logger;
using EList.Common.Models;
using Newtonsoft.Json;
using NLog;
using System.Diagnostics;

namespace EList.FilestorageClient
{
    public class FilestorageClient : IFilestorageClient
    {

        #region logger
        private static readonly NLog.ILogger log = LogManager.GetCurrentClassLogger();
        private static readonly ILoggerWrapper logger = new NLogLoggerWrapper(log);
        private const string LOGGER_NAME = "EList.FilestorageClient.";
        #endregion

        private readonly ICorrelationIdProvider _correlationIdProvider;
        private string _baseUrl;
        private string _token;
        private TimeSpan? _timeout;

        public FilestorageClient(ICorrelationIdProvider correlationIdProvider)
        {
            _correlationIdProvider = correlationIdProvider;

            _baseUrl = ConfigurationManager.AppSettings["filestorage:url"];
            _token = ConfigurationManager.AppSettings["filestorage:token"];
            _timeout = ConfigurationManager.AppSettings.Contains("filestorage:timeout")
                ? TimeSpan.Parse(ConfigurationManager.AppSettings["filestorage:timeout"])
                : null;
        }

        public async Task<CommandResult> RegisterAuthDataAsync(Guid userToken, Guid accountId, string JwtHash)
        {
            var correlationId = _correlationIdProvider.Get();
            var execTime = Stopwatch.StartNew();
            var methodName = $"{LOGGER_NAME}{nameof(RegisterAuthDataAsync)}";
            logger.Debug(correlationId, null, methodName, $"Method started", null);

            var client = new HttpRestClient2(_correlationIdProvider.Get(), _baseUrl, _token);
            var request = new AuthorizationDataRequest
            {
                AccountId = accountId,
                JwtHash = JwtHash,
                Token = userToken
            };
            var body = JsonConvert.SerializeObject(request);
            var response = await client.PostAsync<CommandResult>("api/tokenRegistration/register", body, _timeout);

            if (!response.Success)
                logger.Warn(correlationId, null, methodName, $"{response.Message}", null);

            logger.Debug(correlationId, null, methodName, $"Method finished", null);
            return response;
        }

        public async Task<CommandResult> DisableAuthDataAsync(Guid userToken, string JwtHash)
        {
            var correlationId = _correlationIdProvider.Get();
            var execTime = Stopwatch.StartNew();
            var methodName = $"{LOGGER_NAME}{nameof(DisableAuthDataAsync)}";
            logger.Debug(correlationId, null, methodName, $"Method started", null);

            var client = new HttpRestClient2(_correlationIdProvider.Get(), _baseUrl, _token);
            var request = new AuthorizationDataRequest
            {
                JwtHash = JwtHash,
                Token = userToken
            };
            var body = JsonConvert.SerializeObject(request);
            var response = await client.PostAsync<CommandResult>("api/tokenRegistration/disable", body, _timeout);

            if (!response.Success)
                logger.Warn(correlationId, null, methodName, $"{response.Message}", null);

            logger.Debug(correlationId, null, methodName, $"Method finished", null);
            return response;
        }
    }
}