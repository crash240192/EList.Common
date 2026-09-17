using EList.Common.Configuration;
using EList.Common.CorrelationId;
using EList.Common.HttpRestClient;
using EList.Common.Logger;
using EList.Common.Models;
using Newtonsoft.Json;
using NLog;
using System.Diagnostics;
using FileInfo = EList.FilestorageClient.Models.FileInfo;

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

        public async Task<CommandResult<FileInfo>> GetFileInfoAsync(Guid id, Guid userToken, string jwt)
        {
            var correlationId = _correlationIdProvider.Get();
            var execTime = Stopwatch.StartNew();
            var methodName = $"{LOGGER_NAME}{nameof(GetFileInfoAsync)}";
            logger.Debug(correlationId, null, methodName, $"Method started", null);

            var client = new HttpRestClient2(_correlationIdProvider.Get(), _baseUrl, _token);
            var headers = new Dictionary<string, string>()
            {
                { "Authorization-jwt", jwt }
            };
            var response = await client.GetAsync<CommandResult<FileInfo>>($"api/info/{id}", headers, _timeout);

            if (!response.Success)
                logger.Warn(correlationId, null, methodName, $"{response.Message}", null);

            logger.Debug(correlationId, null, methodName, $"Method finished", null);
            return response;
        }

        public async Task<CommandResult> DeleteFileAsync(Guid id, Guid userToken, string jwt)
        {
            var correlationId = _correlationIdProvider.Get();
            var execTime = Stopwatch.StartNew();
            var methodName = $"{LOGGER_NAME}{nameof(DeleteFileAsync)}";
            logger.Debug(correlationId, null, methodName, $"Method started", null);

            var client = new HttpRestClient2(_correlationIdProvider.Get(), _baseUrl, _token);
            var headers = new Dictionary<string, string>()
            {
                { "Authorization-jwt", jwt }
            };
            var response = await client.DeleteAsync<CommandResult>($"api/delete/{id}", headers, _timeout);

            if (!response.Success)
                logger.Warn(correlationId, null, methodName, $"{response.Message}", null);

            logger.Debug(correlationId, null, methodName, $"Method finished", null);
            return response;
        }

        public async Task<CommandResult> SetFilesVisibilityAsync(IReadOnlyList<Guid> fileIds, FileVisibility visibility)
        {
            var correlationId = _correlationIdProvider.Get();
            var methodName = $"{LOGGER_NAME}{nameof(SetFilesVisibilityAsync)}";
            logger.Debug(correlationId, null, methodName, $"Method started", null);

            if (fileIds == null || fileIds.Count == 0)
                return CommandResult.OK;

            var client = new HttpRestClient2(_correlationIdProvider.Get(), _baseUrl, _token);
            var body = JsonConvert.SerializeObject(new SetFilesVisibilityRequest
            {
                FileIds = fileIds.Where(id => id != Guid.Empty).Distinct().ToList(),
                Visibility = visibility
            });
            var response = await client.PostAsync<CommandResult>("api/internal/setVisibility", body, _timeout);

            if (!response.Success)
                logger.Warn(correlationId, null, methodName, $"{response.Message}", null);

            logger.Debug(correlationId, null, methodName, $"Method finished", null);
            return response;
        }

        public async Task<CommandResult> SetFilesAccessStatusAsync(IReadOnlyList<Guid> fileIds, FileAccessStatus accessStatus)
        {
            var correlationId = _correlationIdProvider.Get();
            var methodName = $"{LOGGER_NAME}{nameof(SetFilesAccessStatusAsync)}";
            logger.Debug(correlationId, null, methodName, $"Method started", null);

            if (fileIds == null || fileIds.Count == 0)
                return CommandResult.OK;

            var client = new HttpRestClient2(_correlationIdProvider.Get(), _baseUrl, _token);
            var body = JsonConvert.SerializeObject(new SetFilesAccessStatusRequest
            {
                FileIds = fileIds.Where(id => id != Guid.Empty).Distinct().ToList(),
                AccessStatus = accessStatus
            });
            var response = await client.PostAsync<CommandResult>("api/internal/setAccessStatus", body, _timeout);

            if (!response.Success)
                logger.Warn(correlationId, null, methodName, $"{response.Message}", null);

            logger.Debug(correlationId, null, methodName, $"Method finished", null);
            return response;
        }

        public async Task<CommandResult<FileDownloadResult>> DownloadFileAsync(Guid fileId, bool? fullSize = null)
        {
            var correlationId = _correlationIdProvider.Get();
            var methodName = $"{LOGGER_NAME}{nameof(DownloadFileAsync)}";
            logger.Debug(correlationId, null, methodName, $"Method started", null);

            try
            {
                var url = $"{_baseUrl.TrimEnd('/')}/api/download/{fileId}";
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.TryAddWithoutValidation("Authorization", _token);
                request.Headers.TryAddWithoutValidation("CorrelationId", correlationId);
                if (fullSize != null)
                    request.Headers.TryAddWithoutValidation("FullSize", fullSize.Value ? "true" : "false");

                using var cts = _timeout != null
                    ? new CancellationTokenSource(_timeout.Value)
                    : new CancellationTokenSource();

                using var response = await SharedHttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                if (!response.IsSuccessStatusCode)
                {
                    var msg = $"Download failed: {(int)response.StatusCode} {response.ReasonPhrase}";
                    logger.Warn(correlationId, null, methodName, msg, null);
                    return CommandResult<FileDownloadResult>.Fail(1, msg);
                }

                var bytes = await response.Content.ReadAsByteArrayAsync(cts.Token);
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
                var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                    ?? fileId.ToString();

                logger.Debug(correlationId, null, methodName, $"Method finished", null);
                return new CommandResult<FileDownloadResult>(new FileDownloadResult
                {
                    Content = bytes,
                    ContentType = contentType,
                    FileName = fileName
                });
            }
            catch (Exception ex)
            {
                logger.Warn(correlationId, null, methodName, $"{ex.Message}", null);
                return CommandResult<FileDownloadResult>.Fail(1, ex.Message);
            }
        }

        public async Task<CommandResult<List<Guid>>> GetGcCandidateIdsAsync(int olderThanDays = 7, int take = 100)
        {
            var correlationId = _correlationIdProvider.Get();
            var methodName = $"{LOGGER_NAME}{nameof(GetGcCandidateIdsAsync)}";
            logger.Debug(correlationId, null, methodName, $"Method started", null);

            var client = new HttpRestClient2(_correlationIdProvider.Get(), _baseUrl, _token);
            var response = await client.GetAsync<CommandResult<List<Guid>>>(
                $"api/internal/gc/candidates?olderThanDays={olderThanDays}&take={take}",
                timeout: _timeout);

            if (!response.Success)
                logger.Warn(correlationId, null, methodName, $"{response.Message}", null);

            logger.Debug(correlationId, null, methodName, $"Method finished", null);
            return response;
        }

        public async Task<CommandResult> DeleteFileAsServiceAsync(Guid id)
        {
            var correlationId = _correlationIdProvider.Get();
            var methodName = $"{LOGGER_NAME}{nameof(DeleteFileAsServiceAsync)}";
            logger.Debug(correlationId, null, methodName, $"Method started", null);

            var client = new HttpRestClient2(_correlationIdProvider.Get(), _baseUrl, _token);
            var response = await client.DeleteAsync<CommandResult>($"api/delete/{id}", timeout: _timeout);

            if (!response.Success)
                logger.Warn(correlationId, null, methodName, $"{response.Message}", null);

            logger.Debug(correlationId, null, methodName, $"Method finished", null);
            return response;
        }

        /// <summary>
        /// Dedicated client for binary download (shared JSON Accept header would break image responses).
        /// </summary>
        private static readonly HttpClient SharedHttpClient = CreateDownloadHttpClient();

        private static HttpClient CreateDownloadHttpClient()
        {
            var handler = new SocketsHttpHandler
            {
                AllowAutoRedirect = false
            };
            handler.SslOptions.RemoteCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            return new HttpClient(handler, false);
        }
    }
}