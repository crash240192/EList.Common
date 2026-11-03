using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using EList.Common.Configuration;
using EList.Common.Extensions;
using EList.Common.HttpRestClient.HttpRestClient3Base;
using EList.Common.Logger;
using EList.Common.Logger.Containers;
using Microsoft.AspNetCore.Http;
using N3.Common.Net.Exceptions;
using N3.Common.Net.Http;
using Newtonsoft.Json;
using NLog;

namespace EList.Common.HttpRestClient
{
    public class HttpRestClient3 : IHttpRestClient3
    {
        #region nlog

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        private static readonly ILoggerWrapper logger = new NLogLoggerWrapper(Log);

        #endregion nlog

        private const string APPSETTING_POOLED_CONNECTION_LIFETIME_KEY = "httpHandler:pooledConnectionLifetime";
        private static readonly HttpClient httpClient;
        private static readonly SocketsHttpHandler socketsClientHandler;
        private string AuthorizationToken { get; }
        public bool NoWriteLongResponse { get; }
        private string BaseUrl { get; }
        private string CorrelationId { get; }

        private string RequestId { get; }

        #region ctors

        static HttpRestClient3()
        {
            socketsClientHandler = new SocketsHttpHandler
            {
                AllowAutoRedirect = false
            };

            socketsClientHandler.SslOptions.RemoteCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            socketsClientHandler.SslOptions.EnabledSslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;

            if (ConfigurationManager.AppSettings.Contains(APPSETTING_POOLED_CONNECTION_LIFETIME_KEY))
                socketsClientHandler.PooledConnectionLifetime
                    = TimeSpan.Parse(ConfigurationManager.AppSettings[APPSETTING_POOLED_CONNECTION_LIFETIME_KEY]);

            httpClient = new HttpClient(socketsClientHandler, false);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:44.0) Gecko/20100101 Firefox/44.0");

            if (ConfigurationManager.AppSettings.ContainsSection("httpClient:timeout"))
            {
                bool timeoutParsed = int.TryParse(ConfigurationManager.AppSettings["httpClient:timeout"], out var timeoutMs);
                if (timeoutParsed)
                    httpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMs);
            }
        }

        public HttpRestClient3(
            string correlationId,
            string baseUrl = null,
            string authorizationToken = null,
            bool noWriteLongResponse = false,
            string requestId = null)
        {
            CorrelationId = correlationId;
            BaseUrl = baseUrl;
            RequestId = requestId ?? string.Empty;
            if (!BaseUrl.EndsWith('/'))
            {
                BaseUrl += '/';
            }
            AuthorizationToken = authorizationToken;
            NoWriteLongResponse = noWriteLongResponse;
        }

        #endregion ctors

        #region Public Methods

        #region Delete Methods Implementation

        public async Task<HttpResult<T>> DeleteAsync<T>(string url, Dictionary<string, string>? headers = default, TimeSpan? timeout = default) where T : class
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            AddHeaders(requestMessage, headers);
            return await SendRequestAsync<T>(requestMessage, timeout);
        }

        public Task<HttpResult<string>> DeleteAsync(string url, Dictionary<string, string> headers = null)
        {
            return DeleteAsync(url, headers, CancellationToken.None);
        }

        public Task<HttpResult<T>> DeleteAsync<T>(string url, Dictionary<string, string> headers = null) where T : class
        {
            return DeleteAsync<T>(url, headers, CancellationToken.None);
        }

        public async Task<HttpResult<T>> DeleteAsync<T>(string url, Dictionary<string, string>? headers = default, CancellationToken? ct = default) where T : class
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            AddHeaders(requestMessage, headers);
            return await SendRequestAsync<T>(requestMessage, cancellationToken: ct);
        }

        public async Task<HttpResult<string>> DeleteAsync(string url, Dictionary<string, string>? headers = default, TimeSpan? timeout = default)
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            AddHeaders(requestMessage, headers);
            return await SendRequestAsync(requestMessage, timeout);
        }

        public async Task<HttpResult<string>> DeleteAsync(string url, Dictionary<string, string>? headers = default, CancellationToken? ct = default)
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            AddHeaders(requestMessage, headers);
            return await SendRequestAsync(requestMessage, cancellationToken: ct);
        }

        #endregion Delete Methods Implementation

        #region Get Methods Implementation

        public async Task<HttpResult<T>> GetAsync<T>(string url, Dictionary<string, string>? headers = default, TimeSpan? timeout = default) where T : class
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
            AddHeaders(requestMessage, headers);
            return await SendRequestAsync<T>(requestMessage, timeout);
        }

        public Task<HttpResult<string>> GetAsync(string url, Dictionary<string, string> headers = null)
        {
            return GetAsync(url, headers, CancellationToken.None);
        }

        public Task<HttpResult<T>> GetAsync<T>(string url, Dictionary<string, string> headers = null) where T : class
        {
            return GetAsync<T>(url, headers, CancellationToken.None);
        }

        public Task<HttpResult<T>> GetAsync<T>(string url) where T : class
        {
            return GetAsync<T>(url, headers: null, CancellationToken.None);
        }

        public async Task<HttpResult<T>> GetAsync<T>(string url, Dictionary<string, string>? headers = default, CancellationToken? ct = default) where T : class
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
            AddHeaders(requestMessage, headers);
            return await SendRequestAsync<T>(requestMessage, cancellationToken: ct);
        }

        public async Task<HttpResult<string>> GetAsync(string url, Dictionary<string, string>? headers = default, TimeSpan? timeout = default)
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
            AddHeaders(requestMessage, headers);
            return await SendRequestAsync(requestMessage, timeout);
        }

        public Task<HttpResult<string>> GetAsync(string url)
        {
            return GetAsync<string>(url, headers: null, CancellationToken.None);
        }

        public async Task<HttpResult<string>> GetAsync(string url, Dictionary<string, string>? headers = default, CancellationToken? ct = default)
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
            AddHeaders(requestMessage, headers);
            return await SendRequestAsync(requestMessage, cancellationToken: ct);
        }

        #endregion Get Methods Implementation

        #region Post Methods Implementation

        public async Task<HttpResult<T>> PostAsync<T>(string url, Dictionary<string, string> headers, object content, TimeSpan? timeout = default) where T : class
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            AddHeaders(requestMessage, headers);
            AddContent(requestMessage, content);
            return await SendRequestAsync<T>(requestMessage, timeout);
        }

        public Task<HttpResult<string>> PostFormDataAsync(string url, Dictionary<string, string> headers, object content)
        {
            return PostFormDataAsync(url, headers, content, CancellationToken.None);
        }

        public Task<HttpResult<T>> PostFormDataAsync<T>(string url, Dictionary<string, string> headers, object content) where T : class
        {
            return PostFormDataAsync<T>(url, headers, content, CancellationToken.None);
        }

        public Task<HttpResult<string>> PostAsync(string url, Dictionary<string, string> headers, FormUrlEncodedContent content)
        {
            return PostAsync(url, headers, content, CancellationToken.None);
        }

        public Task<HttpResult<string>> PostAsync(string url, Dictionary<string, string> headers, object content)
        {
            return PostAsync(url, headers, content, CancellationToken.None);
        }

        public Task<HttpResult<T>> PostAsync<T>(string url, Dictionary<string, string> headers, FormUrlEncodedContent content) where T : class
        {
            return PostAsync<T>(url, headers, content, CancellationToken.None);
        }

        public Task<HttpResult<T>> PostAsync<T>(string url, Dictionary<string, string> headers, object content) where T : class
        {
            return PostAsync<T>(url, headers, content, CancellationToken.None);
        }

        public async Task<HttpResult<T>> PostAsync<T>(string url, Dictionary<string, string> headers, object content, CancellationToken? ct = default) where T : class
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            AddHeaders(requestMessage, headers);
            AddContent(requestMessage, content);
            return await SendRequestAsync<T>(requestMessage, cancellationToken: ct);
        }

        public async Task<HttpResult<T>> PostAsync<T>(string url, object content, TimeSpan? timeout = default) where T : class
        {
            return await PostAsync<T>(url, null, content, timeout);
        }

        public async Task<HttpResult<T>> PostAsync<T>(string url, object content, CancellationToken? ct = default) where T : class
        {
            return await PostAsync<T>(url, null, content, ct);
        }

        public async Task<HttpResult<string>> PostAsync(string url, Dictionary<string, string> headers, object content, TimeSpan? timeout = default)
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            AddHeaders(requestMessage, headers);
            AddContent(requestMessage, content);
            return await SendRequestAsync(requestMessage, timeout);
        }

        public async Task<HttpResult<string>> PostAsync(string url, Dictionary<string, string> headers, object content, CancellationToken? ct = default)
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            AddHeaders(requestMessage, headers);
            AddContent(requestMessage, content);
            return await SendRequestAsync(requestMessage, cancellationToken: ct);
        }

        public async Task<HttpResult<string>> PostAsync(string url, object content, TimeSpan? timeout = default)
        {
            return await PostAsync(url, null, content, timeout);
        }

        public async Task<HttpResult<string>> PostAsync(string url, object content, CancellationToken? ct = default)
        {
            return await PostAsync(url, null, content, ct);
        }

        public async Task<HttpResult<T>> PostAsync<T>(string url, Dictionary<string, string> headers, FormUrlEncodedContent content, TimeSpan? timeout = default) where T : class
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            AddHeaders(requestMessage, headers);
            requestMessage.Content = content;
            return await SendRequestAsync<T>(requestMessage, timeout);
        }

        public async Task<HttpResult<T>> PostAsync<T>(string url, Dictionary<string, string> headers, FormUrlEncodedContent content, CancellationToken? ct = default) where T : class
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            AddHeaders(requestMessage, headers);
            requestMessage.Content = content;
            return await SendRequestAsync<T>(requestMessage, cancellationToken: ct);
        }

        public async Task<HttpResult<T>> PostAsync<T>(string url, FormUrlEncodedContent content, TimeSpan? timeout = default) where T : class
        {
            return await PostAsync<T>(url, null, content, timeout);
        }

        public async Task<HttpResult<T>> PostAsync<T>(string url, FormUrlEncodedContent content, CancellationToken? ct = default) where T : class
        {
            return await PostAsync<T>(url, null, content, ct);
        }

        public async Task<HttpResult<string>> PostAsync(string url, Dictionary<string, string> headers, FormUrlEncodedContent content, TimeSpan? timeout = default)
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            AddHeaders(requestMessage, headers);
            requestMessage.Content = content;
            return await SendRequestAsync(requestMessage, timeout);
        }

        public async Task<HttpResult<string>> PostAsync(string url, Dictionary<string, string> headers, FormUrlEncodedContent content, CancellationToken? ct = default)
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            AddHeaders(requestMessage, headers);
            requestMessage.Content = content;
            return await SendRequestAsync(requestMessage, cancellationToken: ct);
        }

        public async Task<HttpResult<string>> PostAsync(string url, FormUrlEncodedContent content, TimeSpan? timeout = default)
        {
            return await PostAsync(url, null, content, timeout);
        }

        public async Task<HttpResult<string>> PostAsync(string url, FormUrlEncodedContent content, CancellationToken? ct = default)
        {
            return await PostAsync(url, null, content, ct);
        }

        #endregion Post Methods Implementation

        #region Put Methods Implementation

        public async Task<HttpResult<T>> PutAsync<T>(string url, Dictionary<string, string> headers, object content, TimeSpan? timeout = default) where T : class
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Put, requestUri);
            AddHeaders(requestMessage, headers);
            AddContent(requestMessage, content);
            return await SendRequestAsync<T>(requestMessage, timeout);
        }

        public Task<HttpResult<string>> PutAsync(string url, Dictionary<string, string> headers, object content)
        {
            return PutAsync(url, headers, content, CancellationToken.None);
        }

        public Task<HttpResult<T>> PutAsync<T>(string url, Dictionary<string, string> headers, object content) where T : class
        {
            return PutAsync<T>(url, headers, content, CancellationToken.None);
        }

        public async Task<HttpResult<T>> PutAsync<T>(string url, Dictionary<string, string> headers, object content, CancellationToken? ct = default) where T : class
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Put, requestUri);
            AddHeaders(requestMessage, headers);
            AddContent(requestMessage, content);
            return await SendRequestAsync<T>(requestMessage, cancellationToken: ct);
        }

        public async Task<HttpResult<T>> PutAsync<T>(string url, object content, TimeSpan? timeout = default) where T : class
        {
            return await PutAsync<T>(url, null, content, timeout);
        }

        public async Task<HttpResult<T>> PutAsync<T>(string url, object content, CancellationToken? ct = default) where T : class
        {
            return await PutAsync<T>(url, null, content, ct);
        }

        public async Task<HttpResult<string>> PutAsync(string url, Dictionary<string, string> headers, object content, TimeSpan? timeout = default)
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Put, requestUri);
            AddHeaders(requestMessage, headers);
            AddContent(requestMessage, content);
            return await SendRequestAsync(requestMessage, timeout);
        }

        public async Task<HttpResult<string>> PutAsync(string url, Dictionary<string, string> headers, object content, CancellationToken? ct = default)
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Put, requestUri);
            AddHeaders(requestMessage, headers);
            AddContent(requestMessage, content);
            return await SendRequestAsync(requestMessage, cancellationToken: ct);
        }

        public async Task<HttpResult<string>> PutAsync(string url, object content, TimeSpan? timeout = default)
        {
            return await PutAsync(url, null, content, timeout);
        }

        public async Task<HttpResult<string>> PutAsync(string url, object content, CancellationToken? ct = default)
        {
            return await PutAsync(url, null, content, ct);
        }

        #endregion Put Methods Implementation

        #region Patch Methods Implementation

        public async Task<HttpResult<T>> PatchAsync<T>(string url, Dictionary<string, string> headers, object content, TimeSpan? timeout = default) where T : class
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            AddHeaders(requestMessage, headers);
            AddContent(requestMessage, content);
            return await SendRequestAsync<T>(requestMessage, timeout);
        }

        public Task<HttpResult<string>> PatchAsync(string url, Dictionary<string, string> headers, object content)
        {
            return PatchAsync(url, headers, content, CancellationToken.None);
        }

        public Task<HttpResult<T>> PatchAsync<T>(string url, Dictionary<string, string> headers, object content) where T : class
        {
            return PatchAsync<T>(url, headers, content, CancellationToken.None);
        }

        public async Task<HttpResult<T>> PatchAsync<T>(string url, Dictionary<string, string> headers, object content, CancellationToken? ct = default) where T : class
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            AddHeaders(requestMessage, headers);
            AddContent(requestMessage, content);
            return await SendRequestAsync<T>(requestMessage, cancellationToken: ct);
        }

        public async Task<HttpResult<T>> PatchAsync<T>(string url, object content, TimeSpan? timeout = default) where T : class
        {
            return await PatchAsync<T>(url, null, content, timeout);
        }

        public async Task<HttpResult<T>> PatchAsync<T>(string url, object content, CancellationToken? ct = default) where T : class
        {
            return await PatchAsync<T>(url, null, content, ct);
        }

        public async Task<HttpResult<string>> PatchAsync(string url, Dictionary<string, string> headers, object content, TimeSpan? timeout = default)
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            AddHeaders(requestMessage, headers);
            AddContent(requestMessage, content);
            return await SendRequestAsync(requestMessage, timeout);
        }

        public async Task<HttpResult<string>> PatchAsync(string url, Dictionary<string, string> headers, object content, CancellationToken? ct = default)
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            AddHeaders(requestMessage, headers);
            AddContent(requestMessage, content);
            return await SendRequestAsync(requestMessage, cancellationToken: ct);
        }

        public async Task<HttpResult<string>> PatchAsync(string url, object content, TimeSpan? timeout = default)
        {
            return await PatchAsync(url, null, content, timeout);
        }

        public async Task<HttpResult<string>> PatchAsync(string url, object content, CancellationToken? ct = default)
        {
            return await PatchAsync(url, null, content, ct);
        }

        #endregion Patch Methods Implementation

        #region PostFormData Methods Implementation

        public async Task<HttpResult<T>> PostFormDataAsync<T>(string url, Dictionary<string, string> headers, object content, TimeSpan? timeout = default) where T : class
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            AddHeaders(requestMessage, headers);
            requestMessage.Content = EncodeContentAsMultipartFormData(content);
            return await SendRequestAsync<T>(requestMessage, timeout);
        }

        public async Task<HttpResult<T>> PostFormDataAsync<T>(string url, Dictionary<string, string> headers, object content, CancellationToken? ct = default) where T : class
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            AddHeaders(requestMessage, headers);
            requestMessage.Content = EncodeContentAsMultipartFormData(content);
            return await SendRequestAsync<T>(requestMessage, cancellationToken: ct);
        }

        public async Task<HttpResult<T>> PostFormDataAsync<T>(string url, object content, TimeSpan? timeout = default) where T : class
        {
            return await PostFormDataAsync<T>(url, null, content, timeout);
        }

        public async Task<HttpResult<T>> PostFormDataAsync<T>(string url, object content, CancellationToken? ct = default) where T : class
        {
            return await PostFormDataAsync<T>(url, null, content, ct);
        }

        public async Task<HttpResult<string>> PostFormDataAsync(string url, Dictionary<string, string> headers, object content, TimeSpan? timeout = default)
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            AddHeaders(requestMessage, headers);
            requestMessage.Content = EncodeContentAsMultipartFormData(content);
            return await SendRequestAsync(requestMessage, timeout);
        }

        public async Task<HttpResult<string>> PostFormDataAsync(string url, Dictionary<string, string> headers, object content, CancellationToken? ct = default)
        {
            var requestUri = BuildRequestUri(url);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            AddHeaders(requestMessage, headers);
            requestMessage.Content = EncodeContentAsMultipartFormData(content);
            return await SendRequestAsync(requestMessage, cancellationToken: ct);
        }

        public async Task<HttpResult<string>> PostFormDataAsync(string url, object content, TimeSpan? timeout = default)
        {
            return await PostFormDataAsync(url, null, content, timeout);
        }

        public async Task<HttpResult<string>> PostFormDataAsync(string url, object content, CancellationToken? ct = default)
        {
            return await PostFormDataAsync(url, null, content, ct);
        }

        #endregion PostFormData Methods Implementation

        #endregion Public Methods

        #region Helpers

        private static MultipartFormDataContent EncodeContentAsMultipartFormData(object content)
        {
            var multipartContent = new MultipartFormDataContent();
            foreach (var prop in content.GetType().GetProperties())
            {
                var propValue = prop.GetValue(content);
                if (propValue is null)
                {
                    continue;
                }
                else if (propValue is IFormFileCollection files)
                {
                    foreach (var file in files)
                    {
                        var streamContent = new StreamContent(file.OpenReadStream());
                        streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
                        streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                        {
                            Name = prop.Name,
                            FileName = file.FileName
                        };
                        multipartContent.Add(streamContent, prop.Name, file.FileName);
                    }
                }
                else if (propValue is IFormFile file)
                {
                    var streamContent = new StreamContent(file.OpenReadStream());
                    streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
                    streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = prop.Name,
                        FileName = file.FileName
                    };
                    multipartContent.Add(streamContent, prop.Name, file.FileName);
                }
                else if (propValue is string str)
                {
                    var stringContent = new StringContent(str, Encoding.Default, MediaTypeNames.Text.Plain);
                    multipartContent.Add(stringContent, prop.Name);
                }
                else
                {
                    var stringContent = new StringContent(JsonConvert.SerializeObject(propValue));
                    multipartContent.Add(stringContent, prop.Name);
                }
            }
            return multipartContent;
        }

        private static void AddContent(HttpRequestMessage request, object content)
        {
            if (content == null) return;

            if (content is string)
            {
                request.Content = new StringContent(content as string, Encoding.UTF8, MediaTypeNames.Application.Json);
            }
            else
            {
                var json = JsonConvert.SerializeObject(content, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Include,
                    DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                    Formatting = Formatting.Indented,
                    Converters = new List<JsonConverter>()
                    {
                        new Newtonsoft.Json.Converters.StringEnumConverter()
                    },

                    Culture = System.Globalization.CultureInfo.InvariantCulture,
                });
                request.Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            }
        }

        private void AddHeaders(HttpRequestMessage request, IDictionary<string, string> additionalHeaders)
        {
            request.Headers.Add("X-Correlation-Id", CorrelationId);

            if (!RequestId.IsNullOrEmpty())
            {
                request.Headers.Add("X-Request-ID", RequestId);
            }

            if (!string.IsNullOrEmpty(AuthorizationToken))
                request.Headers.Add("Authorization", AuthorizationToken);

            if (additionalHeaders != null)
            {
                foreach (var header in additionalHeaders)
                {
                    if (!request.Headers.Contains(header.Key))
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
            }
        }

        private Uri BuildRequestUri(string url)
        {
            var baseUrlRegex = new Regex("/+$");
            var urlRegex = new Regex("^/+");

            if (url == null)
                throw new ArgumentNullException(nameof(url));

            try
            {
                if (url.StartsWith("http"))
                {
                    var uri = new Uri(url);
                    if (uri.IsAbsoluteUri)
                    {

                        return uri;
                    }
                }
            }
            catch (Exception)
            {
            }

            return new Uri($"{baseUrlRegex.Replace(BaseUrl, "/")}{urlRegex.Replace(url.ToString(), "")}");
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

            logger.Error(CorrelationId, null, loggerName, exMessage, execTime, ex, null, meta, incomingMessage);

            return 0;
        }

        private static async Task<HttpResult<T>> ProcessResponseAsync<T>(HttpResponseMessage response, TimeSpan responseTime) where T : class
        {
            var headers = response.Headers
                .ToDictionary(h => h.Key, h => string.Join(";", h.Value));

            if (typeof(T) == typeof(string))
            {
                var content = await response.Content.ReadAsStringAsync();
                return HttpResult<T>.Create(
                    response.StatusCode,
                    headers,
                    responseTime,
                    content as T
                );
            }
            else
                if (typeof(T) == typeof(byte[]))
                {
                    var content = await response.Content.ReadAsByteArrayAsync();
                    return HttpResult<T>.Create(
                        response.StatusCode,
                        headers,
                        responseTime,
                        content as T
                    );
                }
                else
                {

                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<T>(content);
                    return HttpResult<T>.Create(
                        response.StatusCode,
                        headers,
                        responseTime,
                        data
                    );
                }
        }

        private async Task<HttpResult<T>> SendRequestAsync<T>(HttpRequestMessage request, TimeSpan? timeout = null, CancellationToken? cancellationToken = null) where T : class
        {
            var stopWatch = Stopwatch.StartNew();
            var loggerName = request.RequestUri.AbsoluteUri;
            var requestIdContainer = new RequestIdContainer()
            {
                RequestId = RequestId
            };
            var meta = new Dictionary<string, object>();
            try
            {
                HttpResponseMessage httpResponseMessage;
                meta.Add("requestHeaders", request.Headers);
                meta.Add("requestMethod", request.Method.ToString());
                meta.Add("requestUrl", request.RequestUri.AbsoluteUri);
                if (request.Content is not null)
                {
                    meta.Add("requestContent", await request.Content?.ReadAsStringAsync());
                }

                if (timeout is not null)
                {
#pragma warning disable IDE0063 // Использовать простой оператор using
                    using (var cts = new CancellationTokenSource())
                    {
                        stopWatch.Restart();
                        cts.CancelAfter(timeout.Value);
                        httpResponseMessage = await httpClient.SendAsync(request, cts.Token);
                        stopWatch.Stop();
                    }
#pragma warning restore IDE0063 // Использовать простой оператор using
                }
                else if (cancellationToken is not null)
                {
                    stopWatch.Restart();
                    httpResponseMessage = await httpClient.SendAsync(request, cancellationToken.Value);
                    stopWatch.Stop();
                }
                else
                {
#pragma warning disable IDE0063 // Использовать простой оператор using
                    using (var cts = new CancellationTokenSource())
                    {
                        stopWatch.Restart();
                        cts.CancelAfter(httpClient.Timeout);
                        httpResponseMessage = await httpClient.SendAsync(request, cts.Token);
                        stopWatch.Stop();
                    }
#pragma warning restore IDE0063 // Использовать простой оператор using
                }

                meta.Add("responseStatusCode", httpResponseMessage.StatusCode);

                meta.Add("responseHeaders", httpResponseMessage.Headers?.ToString());

                var processedResponse = await ProcessResponseAsync<T>(httpResponseMessage, stopWatch.Elapsed);
                string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
                meta.Add($"responseContentLength", responseContent?.Length);

                if (!NoWriteLongResponse && responseContent?.Length > 100000)
                {
                    var truncatedResponse = responseContent[..100000];
                    truncatedResponse += "\n+-+-+response has been truncated, max length 100000+-+-+";

                    if (request.Content is not null)
                    {
                        logger.Info(requestIdContainer, CorrelationId, null, loggerName, truncatedResponse ?? string.Empty, null, stopWatch.Elapsed, meta, await request.Content?.ReadAsStringAsync());
                    }
                    else
                    {
                        logger.Info(requestIdContainer, CorrelationId, null, loggerName, truncatedResponse ?? string.Empty, null, stopWatch.Elapsed, meta, null);
                    }
                }

                if (request.Content is not null)
                {
                    logger.Info(requestIdContainer, CorrelationId, null, loggerName, responseContent ?? string.Empty, null, stopWatch.Elapsed, meta, await request.Content?.ReadAsStringAsync());
                }
                else
                {
                    logger.Info(requestIdContainer, CorrelationId, null, loggerName, responseContent ?? string.Empty, null, stopWatch.Elapsed, meta, null);

                }

                if (NoWriteLongResponse && responseContent?.Length > 100000)
                {
                    logger.Debug(requestIdContainer, CorrelationId, null, loggerName, responseContent, null, stopWatch.Elapsed, meta);
                }
                return processedResponse;
            }
            catch (OperationCanceledException ex)
            {
                var exMessage = $"Request timeout: {request.Method} to '{request.RequestUri}' failed after {timeout}. " +
                "Increase timeout value or check server availability.";
                LogException(loggerName, exMessage, 0, stopWatch.Elapsed, ex, meta, await request.Content.ReadAsStringAsync());
                throw new CustomHttpRequestException(ex.Message, ex, stopWatch.Elapsed);
            }
            catch (Exception ex)
            {
                var exMessage = $"Failed to perform {request.Method}Async request to '{request.RequestUri}': {ex.Message}";
                LogException(loggerName, exMessage, 0, stopWatch.Elapsed, ex, meta, await request.Content.ReadAsStringAsync());
                throw new CustomHttpRequestException(exMessage, ex, stopWatch.Elapsed);
            }
        }

        private async Task<HttpResult<string>> SendRequestAsync(HttpRequestMessage request, TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
        {
            var result = await SendRequestAsync<string>(request, timeout, cancellationToken);
            return result;
        }

        #endregion Helpers
    }
}