using EList.Common.Configuration;
using EList.Common.Logger;
using Newtonsoft.Json;
using NLog;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;

namespace EList.Common.HttpRestClient
{
    public class HttpRestClient2
    {
        #region NLog
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        private static readonly ILoggerWrapper logger = new NLogLoggerWrapper(Log);
        //private const string LOGGER_NAME = "N3.Common.Net.HttpRestClient.";
        #endregion

        #region private const
        private const string JSON_CONTENT_TYPE = "application/json";
        private const string AUTHORIZATION_HEADER_NAME = "Authorization";
        private const string APPSETTING_POOLED_CONNECTION_LIFETIME_KEY = "httpHandler:pooledConnectionLifetime";
        #endregion

        #region private static HttpClient instance
        private static readonly HttpClient httpClient;
        //private static readonly HttpClientHandler httpClientHandler;
        private static readonly SocketsHttpHandler socketsClientHandler;

        static HttpRestClient2()
        {
            //httpClientHandler = new HttpClientHandler();
            //httpClientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            socketsClientHandler = new SocketsHttpHandler();
            socketsClientHandler.AllowAutoRedirect = false;
            socketsClientHandler.SslOptions.RemoteCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            socketsClientHandler.SslOptions.EnabledSslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;

            if (ConfigurationManager.AppSettings.Contains(APPSETTING_POOLED_CONNECTION_LIFETIME_KEY))
                socketsClientHandler.PooledConnectionLifetime
                    = TimeSpan.Parse(ConfigurationManager.AppSettings[APPSETTING_POOLED_CONNECTION_LIFETIME_KEY]);

            //httpClient = new HttpClient(httpClientHandler, false);
            httpClient = new HttpClient(socketsClientHandler, false);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JSON_CONTENT_TYPE));
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:44.0) Gecko/20100101 Firefox/44.0");
        }
        #endregion

        #region private readonly & constructor
        private readonly string correlationId;
        private readonly string baseUrl;
        private readonly string authorizationToken;

        public HttpRestClient2(
            string correlationId,
            string baseUrl = null,
            string authorizationToken = null)
        {
            this.correlationId = correlationId;
            this.baseUrl = baseUrl;
            this.authorizationToken = authorizationToken;
        }
        #endregion


        public async Task<TResult> GetAsync<TResult>(string url, IDictionary<string, string> headers = null,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
            where TResult : class
        {
            var resultJson = await SendAsync(HttpMethod.Get, url, headers, timeout: timeout, notWriteLongResponse: notWriteLongResponse);
            return JsonConvert.DeserializeObject<TResult>(resultJson);
        }

        public async Task<TResult> TryGetAsync<TResult>(string url, IDictionary<string, string> headers = null,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
        {
            var resultJson = await SendAsync(HttpMethod.Get, url, headers, null, true, timeout, notWriteLongResponse);
            return JsonConvert.DeserializeObject<TResult>(resultJson);
        }

        public async Task<TResult> PostAsync<TResult>(string url, object content,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
            where TResult : class
        {
            return await PostAsync<TResult>(url, null, content, timeout, notWriteLongResponse);
        }

        public async Task<TResult> PostAsync<TResult>(string url, string content,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
            where TResult : class
        {
            return await PostAsync<TResult>(url, null, content, timeout, notWriteLongResponse);
        }

        public async Task<TResult> PostAsync<TResult>(string url, IDictionary<string, string> headers, object content,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
            where TResult : class
        {
            var contentJson = JsonConvert.SerializeObject(content);
            var resultJson = await SendAsync(HttpMethod.Post, url, headers, contentJson, timeout: timeout, notWriteLongResponse: notWriteLongResponse);
            return JsonConvert.DeserializeObject<TResult>(resultJson);
        }

        public async Task<TResult> PostAsync<TResult>(string url, IDictionary<string, string> headers, string content,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
            where TResult : class
        {
            var resultJson = await SendAsync(HttpMethod.Post, url, headers, content, timeout: timeout, notWriteLongResponse: notWriteLongResponse);
            return JsonConvert.DeserializeObject<TResult>(resultJson);
        }

        public async Task<TResult> TryPostAsync<TResult>(string url, object content,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
            where TResult : class
        {
            return await TryPostAsync<TResult>(url, null, content, timeout, notWriteLongResponse);
        }

        public async Task<TResult> TryPostAsync<TResult>(string url, string content,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
            where TResult : class
        {
            return await TryPostAsync<TResult>(url, null, content, timeout, notWriteLongResponse);
        }

        public async Task<TResult> TryPostAsync<TResult>(string url, IDictionary<string, string> headers, object content,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
            where TResult : class
        {
            var contentJson = JsonConvert.SerializeObject(content);
            var resultJson = await SendAsync(HttpMethod.Post, url, headers, contentJson, true, timeout, notWriteLongResponse);
            return JsonConvert.DeserializeObject<TResult>(resultJson);
        }

        public async Task<TResult> TryPostAsync<TResult>(string url, IDictionary<string, string> headers, string content,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
            where TResult : class
        {
            var resultJson = await SendAsync(HttpMethod.Post, url, headers, content, true, timeout, notWriteLongResponse);
            return JsonConvert.DeserializeObject<TResult>(resultJson);
        }

        public async Task<TResult> PutAsync<TResult>(string url, object content,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
            where TResult : class
        {
            return await PutAsync<TResult>(url, null, content, timeout, notWriteLongResponse);
        }

        public async Task<TResult> PutAsync<TResult>(string url, string content,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
            where TResult : class
        {
            return await PutAsync<TResult>(url, null, content, timeout, notWriteLongResponse);
        }

        public async Task<TResult> PutAsync<TResult>(string url, IDictionary<string, string> headers, object content,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
            where TResult : class
        {
            var contentJson = JsonConvert.SerializeObject(content);
            var resultJson = await SendAsync(HttpMethod.Put, url, headers, contentJson, timeout: timeout, notWriteLongResponse: notWriteLongResponse);
            return JsonConvert.DeserializeObject<TResult>(resultJson);
        }

        public async Task<TResult> PutAsync<TResult>(string url, IDictionary<string, string> headers, string content,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
            where TResult : class
        {
            var resultJson = await SendAsync(HttpMethod.Put, url, headers, content, timeout: timeout, notWriteLongResponse: notWriteLongResponse);
            return JsonConvert.DeserializeObject<TResult>(resultJson);
        }

        public async Task<TResult> DeleteAsync<TResult>(string url, IDictionary<string, string> headers = null,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
            where TResult : class
        {
            var resultJson = await SendAsync(HttpMethod.Delete, url, headers, timeout: timeout, notWriteLongResponse: notWriteLongResponse);
            return JsonConvert.DeserializeObject<TResult>(resultJson);
        }

        public async Task<string> GetAsync(string url, IDictionary<string, string> headers = null,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
        {
            return await SendAsync(HttpMethod.Get, url, headers, timeout: timeout, notWriteLongResponse: notWriteLongResponse);
        }

        public async Task<string> TryGetAsync(string url, IDictionary<string, string> headers = null,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
        {
            return await SendAsync(HttpMethod.Get, url, headers, null, true, timeout, notWriteLongResponse);
        }

        public async Task<string> PostAsync(string url, string content,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
        {
            return await PostAsync(url, null, content, timeout, notWriteLongResponse);
        }

        public async Task<string> PostAsync(string url, IDictionary<string, string> headers, string content,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
        {
            return await SendAsync(HttpMethod.Post, url, headers, content, timeout: timeout, notWriteLongResponse: notWriteLongResponse);
        }

        public async Task<string> TryPostAsync(string url, string content,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
        {
            return await TryPostAsync(url, null, content, timeout, notWriteLongResponse);
        }

        public async Task<string> TryPostAsync(string url, IDictionary<string, string> headers, string content,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
        {
            return await SendAsync(HttpMethod.Post, url, headers, content, true, timeout, notWriteLongResponse);
        }

        public async Task<string> PutAsync(string url, string content,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
        {
            return await PutAsync(url, null, content, timeout, notWriteLongResponse);
        }

        public async Task<string> PutAsync(string url, IDictionary<string, string> headers, string content,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
        {
            return await SendAsync(HttpMethod.Put, url, headers, content, timeout: timeout, notWriteLongResponse: notWriteLongResponse);
        }

        public async Task<string> DeleteAsync(string url, IDictionary<string, string> headers = null,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
        {
            return await SendAsync(HttpMethod.Delete, url, headers, timeout: timeout, notWriteLongResponse: notWriteLongResponse);
        }

        public async Task<string> PatchAsync(string url, string content,
            TimeSpan? timeout = null, bool notWriteLongResponse = false)
        {
            return await SendAsync(HttpMethod.Patch, url, null, content, timeout: timeout, notWriteLongResponse: notWriteLongResponse);
        }


        private async Task<string> SendAsync(
            HttpMethod requestMethod,
            string requestUrl,
            IDictionary<string, string> requestHeaders = null,
            string requestContent = null,
            bool ignoreResponseStatusCode = false,
            TimeSpan? timeout = null,
            bool notWriteLongResponse = false)
        {
            var sw = Stopwatch.StartNew();
            var meta = new Dictionary<string, object>();

            // build full request url
            requestUrl = UrlHelper.BuildUrl(baseUrl, requestUrl);

            var loggerName = requestUrl;

            try
            {
                // request method
                meta.Add(nameof(requestMethod), requestMethod.ToString());

                // request url
                meta.Add(nameof(requestUrl), requestUrl);

                // request content
                HttpContent contentMember = null;

                if (!string.IsNullOrWhiteSpace(requestContent))
                {
                    contentMember = new StringContent(requestContent, Encoding.Default, JSON_CONTENT_TYPE);
                    meta.Add(nameof(requestContent), requestContent);
                }

                // build request
                var request = new HttpRequestMessage
                {
                    Method = requestMethod,
                    RequestUri = new Uri(requestUrl),
                    Content = contentMember
                };

                // request headers
                var metaRequestHeaders = new Dictionary<string, string>();

                if (!string.IsNullOrWhiteSpace(authorizationToken))
                {
                    request.Headers.Add(AUTHORIZATION_HEADER_NAME, authorizationToken);
                    metaRequestHeaders.Add(AUTHORIZATION_HEADER_NAME, authorizationToken);
                }

                if (requestHeaders != null)
                {
                    foreach (var entry in requestHeaders)
                    {
                        request.Headers.Add(entry.Key, entry.Value);
                        metaRequestHeaders.Add(entry.Key, entry.Value);
                    }
                }

                meta.Add(nameof(requestHeaders), metaRequestHeaders);

                // perform request              

                HttpResponseMessage response;
                if (timeout == null)
                {
                    response = await httpClient.SendAsync(request);
                }
                else
                {
                    var cts = new CancellationTokenSource();
                    cts.CancelAfter(timeout.Value);
                    response = await httpClient.SendAsync(request, cts.Token);
                }

                if (response == null)
                {
                    throw new ApplicationException($"Response is null");
                }

                // response status code
                var responseStatusCode = response.StatusCode;
                meta.Add(nameof(responseStatusCode), responseStatusCode);

                // response headers
                var responseHeaders = response.Headers;
                meta.Add(nameof(responseHeaders), responseHeaders?.ToString());

                // response content
                string responseContent = null;
                if (response.Content != null)
                {
                    responseContent = await response.Content.ReadAsStringAsync();
                }

                meta.Add($"{nameof(responseContent)}Length", responseContent?.Length);

                if (!ignoreResponseStatusCode)
                {
                    var validStatusCodes = new[] { HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Accepted };
                    if (!validStatusCodes.Contains(responseStatusCode))
                    {
                        throw new HttpRequestException($"Invalid response status code: {(int)responseStatusCode} {responseStatusCode}");
                    }
                }
                
                logger.Info(correlationId, null, loggerName,  notWriteLongResponse && responseContent?.Length > 100000 ? string.Empty : responseContent ?? string.Empty, null, sw.Elapsed, meta, requestContent);

                if (notWriteLongResponse && responseContent?.Length > 100000)
                {
                    logger.Debug(correlationId, null, loggerName, responseContent, null, sw.Elapsed, meta);
                }
                return responseContent;
            }
            catch (OperationCanceledException ex)
            {
                var sendTimeout = timeout ?? httpClient.Timeout;
                var exMessage = $"Failed to perform {requestMethod}Async request to '{requestUrl}' timed out attempting to send after {sendTimeout}." +
                    $"Increase the timeout value passed to the call to Request or increase the SendTimeout value on the Binding." +
                    $"The time allowed to this operation may have been a portion of a longer timeout.";
                LogException(loggerName, exMessage, 0, sw.Elapsed, ex, meta, requestContent);
                throw new HttpRequestException(exMessage, ex);
            }
            catch (Exception ex)
            {
                var exMessage = $"Failed to perform {requestMethod}Async request to '{requestUrl}': {ex.Message}";
                LogException(loggerName, exMessage, 0, sw.Elapsed, ex, meta, requestContent);
                throw new HttpRequestException(exMessage, ex);
            }
        }

        private int LogException(
            string loggerName,
            string message,
            int level,
            TimeSpan execTime,
            Exception ex,
            Dictionary<string, object> meta,
            string incomingMessage)
        {
            if (ex is AggregateException)
            {
                ((AggregateException)ex).InnerExceptions?.Select(innerEx =>
                    LogException(loggerName, message, level + 1, execTime, innerEx, meta, incomingMessage));
            }

            if (ex.InnerException != null)
            {
                LogException(loggerName, message, level + 1, execTime, ex.InnerException, meta, incomingMessage);
            }

            var levelStr = level > 0 ? $" (inner #{level})" : string.Empty;
            var exMessage = $"{message}{levelStr}: {ex.Message}";

            logger.Error(correlationId, null, loggerName, exMessage, execTime, ex, null, meta, incomingMessage);

            return 0;
        }
    }
}
