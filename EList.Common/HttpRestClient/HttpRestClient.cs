using EList.Common.Logger;
using EList.Common.Threading;
using Newtonsoft.Json;
using NLog;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace EList.Common.HttpRestClient
{
    public class HttpRestClient
    {
        #region NLog
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        private static readonly ILoggerWrapper logger = new NLogLoggerWrapper(Log);
        #endregion

        #region private & constructor
        private const string MEDIA_TYPE = "application/json";
        private readonly string baseUrl;
        private readonly IDictionary<string, string> headers;

        private string AuthorizationHeader => headers?.FirstOrDefault(x => x.Key == "Authorization").Value;

        public HttpRestClient(IDictionary<string, string> headers = null)
        {
            if (this.headers != null &&
                this.headers.Any(x => x.Key != "Authorization"))
            {
                throw new NotSupportedException("Consider passing custom headers to a method, not to the constructor");
            }

            this.headers = headers;
        }

        public HttpRestClient(string baseUrl, IDictionary<string, string> headers = null)
            : this(headers)
        {
            // Пустой baseUrl допустим.
            // Если baseUrl пустой, запрос выполняется по полному url, передаваемому в методы Get и Post.
            // Cм. метод BuildUrl ниже.
            this.baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }
        #endregion


        #region Get, GetAsync
        [Obsolete("Consider using 'await GetAsync'")]
        public TResult Get<TResult>(string url, string correlationId,
            TimeSpan? timeout = null)
            where TResult : class
        {
            var client2 = new HttpRestClient2(correlationId, baseUrl, AuthorizationHeader);
            return AsyncHelper.RunSync(() => client2.GetAsync<TResult>(url, timeout: timeout));
        }

        [Obsolete("Consider using 'await GetAsync'")]
        public string Get(string url, string correlationId,
            TimeSpan? timeout = null)
        {
            var client2 = new HttpRestClient2(correlationId, baseUrl, AuthorizationHeader);
            return AsyncHelper.RunSync(() => client2.GetAsync(url, timeout: timeout));
        }

        public async Task<TResult> GetAsync<TResult>(string url, string correlationId,
            TimeSpan? timeout = null)
            where TResult : class
        {
            var client2 = new HttpRestClient2(correlationId, baseUrl, AuthorizationHeader);
            return await client2.GetAsync<TResult>(url, timeout: timeout);
        }

        public async Task<string> GetAsync(string url, string correlationId,
            TimeSpan? timeout = null)
        {
            var client2 = new HttpRestClient2(correlationId, baseUrl, AuthorizationHeader);
            return await client2.GetAsync(url, timeout: timeout);
        }
        #endregion


        #region Post, PostAsync
        [Obsolete("Consider using 'await PostAsync'")]
        public TResult Post<TResult>(string url, object content, string correlationId,
            TimeSpan? timeout = null)
            where TResult : class
        {
            var client2 = new HttpRestClient2(correlationId, baseUrl, AuthorizationHeader);
            return AsyncHelper.RunSync(() => client2.PostAsync<TResult>(url, content, timeout));
        }

        [Obsolete("Consider using 'await PostAsync'")]
        public TResult Post<TResult>(string url, string content, string correlationId,
            TimeSpan? timeout = null)
            where TResult : class
        {
            var client2 = new HttpRestClient2(correlationId, baseUrl, AuthorizationHeader);
            return AsyncHelper.RunSync(() => client2.PostAsync<TResult>(url, content, timeout));
        }

        [Obsolete("Consider using 'await PostAsync'")]
        public string Post(string url, string content, string correlationId,
            TimeSpan? timeout = null)
        {
            var client2 = new HttpRestClient2(correlationId, baseUrl, AuthorizationHeader);
            return AsyncHelper.RunSync(() => client2.PostAsync(url, content, timeout));
        }

        public async Task<TResult> PostAsync<TResult>(string url, object content, string correlationId,
            TimeSpan? timeout = null)
            where TResult : class
        {
            var client2 = new HttpRestClient2(correlationId, baseUrl, AuthorizationHeader);
            return await client2.PostAsync<TResult>(url, content, timeout);
        }

        public async Task<TResult> PostAsync<TResult>(string url, string content, string correlationId,
            TimeSpan? timeout = null)
            where TResult : class
        {
            var client2 = new HttpRestClient2(correlationId, baseUrl, AuthorizationHeader);
            return await client2.PostAsync<TResult>(url, content, timeout);
        }

        public async Task<string> PostAsync(string url, string content, string correlationId,
            TimeSpan? timeout = null)
        {
            var client2 = new HttpRestClient2(correlationId, baseUrl, AuthorizationHeader);
            return await client2.PostAsync(url, content, timeout);
        }
        #endregion


        #region PostEx, PostForm (FIXME: async/await, use HttpRestClient2)
        public HttpResponse PostEx(string url, string content, string correlationId,
            TimeSpan? timeout = null)
        {
            var fullUrl = UrlHelper.BuildUrl(baseUrl, url);
            var meta = new Dictionary<string, object>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MEDIA_TYPE));

                    var contentObj = new StringContent(content, Encoding.Default, MEDIA_TYPE);

                    if (headers != null)
                        foreach (var header in headers)
                            switch (header.Key.ToLower())
                            {
                                case "content-type": contentObj.Headers.ContentType = new MediaTypeWithQualityHeaderValue(header.Value) { CharSet = "utf-8" }; break;
                                case "content-length": contentObj.Headers.ContentLength = long.Parse(header.Value); break;
                                default: client.DefaultRequestHeaders.Add(header.Key, header.Value); break;
                            }
                    #region Add Headers to Meta
                    if (contentObj?.Headers != null)
                    {
                        var serializedHeaders = JsonConvert.SerializeObject(contentObj?.Headers);
                        meta.Add("headers", serializedHeaders);
                    }
                    #endregion

                    var sw = Stopwatch.StartNew();
                    Task<HttpResponseMessage> postTask;
                    if (timeout == null)
                    {
                        postTask = client.PostAsync(fullUrl, contentObj);
                    }
                    else
                    {
                        var cts = new CancellationTokenSource();
                        cts.CancelAfter(timeout.Value);
                        postTask = client.PostAsync(fullUrl, contentObj, cts.Token);
                    }
                    postTask.Wait();

                    #region Add statusCode to Meta                
                    var statusCode = postTask?.Result?.StatusCode;
                    var responseCode = statusCode.HasValue ? (int?)statusCode.Value : null;
                    if (responseCode != null)
                    {
                        meta.Add("statusCode", responseCode);
                    }
                    #endregion

                    var readTask = postTask.Result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    #region logger       
                    if (postTask.IsFaulted)
                    {
                        logger.Error(correlationId, null, fullUrl, "Post Method Failed", sw.Elapsed, postTask.Exception, null, meta: meta, incomingMessage: content);
                    }
                    else
                    {
                        meta.Add("taskStatus", postTask.Status.ToString());
                        logger.Info(correlationId, null, fullUrl, readTask.Result, null, null,
                            incomingMessage: content,
                            meta: meta);
                    }
                    #endregion
                    return new HttpResponse
                    {
                        Body = readTask.Result,
                        StatusCode = (int)statusCode.Value
                    };
                }
            }
            catch (OperationCanceledException ex)
            {
                var message = $"Failed to perform HTTP POST request to '{url}' timed out attempting to send after {timeout.Value}. Increase the timeout value passed to the call to Request or increase the SendTimeout value on the Binding. The time allowed to this operation may have been a portion of a longer timeout.";
                #region logger      
                if (ex.InnerException != null)
                {
                    meta.Add("innerExMessage", ex.InnerException.Message);
                    meta.Add("innerStackTrace", ex.InnerException.StackTrace);
                }
                logger.Error(correlationId, null, fullUrl, message, null, ex, null, meta: meta, incomingMessage: content);
                #endregion
                throw new HttpRequestException(message, ex);
            }
            catch (Exception ex)
            {
                var message = $"Failed to perform HTTP POST request to '{url}'; see inner exception details";
                #region logger      
                if (ex.InnerException != null)
                {
                    meta.Add("innerExMessage", ex.InnerException.Message);
                    meta.Add("innerStackTrace", ex.InnerException.StackTrace);
                }
                logger.Error(correlationId, null, fullUrl, message, null, ex, null, meta: meta, incomingMessage: content);
                #endregion

                throw new HttpRequestException(message, ex);
            }
        }

        public TResult PostForm<TResult>(string url, Dictionary<string, string> formData, string correlationId,
            TimeSpan? timeout = null)
        {
            var response = PostForm(url, formData, correlationId, timeout);
            return JsonConvert.DeserializeObject<TResult>(response);
        }

        public string PostForm(string url, Dictionary<string, string> formData, string correlationId,
            TimeSpan? timeout = null)
        {
            var fullUrl = UrlHelper.BuildUrl(baseUrl, url);
            var meta = new Dictionary<string, object>();

            try
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:44.0) Gecko/20100101 Firefox/44.0");

                    var contentObj = new MultipartFormDataContent();

                    foreach (var item in formData)
                    {
                        var content = new StringContent(item.Value, Encoding.Default);
                        contentObj.Add(content, $"{item.Key}");
                    }

                    var sw = Stopwatch.StartNew();
                    Task<HttpResponseMessage> postTask;
                    if (timeout == null)
                    {
                        postTask = client.PostAsync(fullUrl, contentObj);
                    }
                    else
                    {
                        var cts = new CancellationTokenSource();
                        cts.CancelAfter(timeout.Value);
                        postTask = client.PostAsync(fullUrl, contentObj, cts.Token);
                    }
                    postTask.Wait();

                    if (postTask.Result == null)
                        throw new ApplicationException("HttpClient.PostAsync has returned null result");

                    meta.Add("taskStatus", postTask.Status.ToString());
                    meta.Add("statusCode", postTask.Result.StatusCode);

                    if (postTask.IsFaulted)
                    {
                        logger.Error(correlationId, null, fullUrl,
                            "HttpClient.PostAsync task has faulted",
                            sw.Elapsed, postTask.Exception, null, meta: meta, incomingMessage: "formData: " + JsonConvert.SerializeObject(formData));

                        throw postTask.Exception;
                    }
                    else if (postTask.Result.StatusCode != HttpStatusCode.OK && postTask.Result.StatusCode != HttpStatusCode.Created)
                    {
                        var errorMsg = $"HttpClient.PostAsync task has returned status code {(int)postTask.Result.StatusCode}. "
                                        + $"Response headers: {postTask.Result.Headers.ToString()}";

                        logger.Error(correlationId, null, fullUrl,
                            errorMsg, sw.Elapsed, null, null, meta: meta, incomingMessage: "formData: " + JsonConvert.SerializeObject(formData));

                        throw new InvalidOperationException(errorMsg);
                    }
                    else
                    {
                        var readTask = postTask.Result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        logger.Info(correlationId, null, fullUrl, readTask.Result, null, sw.Elapsed,
                            incomingMessage: "formData: " + JsonConvert.SerializeObject(formData),
                            meta: meta);

                        return readTask.Result;
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                var message = $"Failed to perform HTTP POST request to '{url}' timed out attempting to send after {timeout.Value}. Increase the timeout value passed to the call to Request or increase the SendTimeout value on the Binding. The time allowed to this operation may have been a portion of a longer timeout.";
                #region logger      
                if (ex.InnerException != null)
                {
                    meta.Add("innerExMessage", ex.InnerException.Message);
                    meta.Add("innerStackTrace", ex.InnerException.StackTrace);
                }
                logger.Error(correlationId, null, fullUrl, message, null, ex, null, meta: meta, incomingMessage: "formData: " + JsonConvert.SerializeObject(formData));
                #endregion

                throw new HttpRequestException(message, ex);
            }
            catch (Exception ex)
            {
                var message = $"Failed to perform HTTP POST request to '{url}': {ex.Message}. See inner exception details";
                #region logger      
                if (ex.InnerException != null)
                {
                    meta.Add("innerExMessage", ex.InnerException.Message);
                    meta.Add("innerStackTrace", ex.InnerException.StackTrace);
                }
                logger.Error(correlationId, null, fullUrl, message, null, ex, null, meta: meta, incomingMessage: "formData: " + JsonConvert.SerializeObject(formData));
                #endregion

                throw new HttpRequestException(message, ex);
            }
        }
        #endregion


        #region Put, PutAsync
        [Obsolete("Consider using 'await PutAsync'")]
        public TResult Put<TResult>(string url, object content, string correlationId,
            TimeSpan? timeout = null)
            where TResult : class
        {
            var client2 = new HttpRestClient2(correlationId, baseUrl, AuthorizationHeader);
            return AsyncHelper.RunSync(() => client2.PutAsync<TResult>(url, content, timeout));
        }

        [Obsolete("Consider using 'await PutAsync'")]
        public TResult Put<TResult>(string url, string content, string correlationId,
            TimeSpan? timeout = null)
            where TResult : class
        {
            var client2 = new HttpRestClient2(correlationId, baseUrl, AuthorizationHeader);
            return AsyncHelper.RunSync(() => client2.PutAsync<TResult>(url, content, timeout));
        }

        [Obsolete("Consider using 'await PutAsync'")]
        public string Put(string url, string content, string correlationId,
            TimeSpan? timeout = null)
        {
            var client2 = new HttpRestClient2(correlationId, baseUrl, AuthorizationHeader);
            return AsyncHelper.RunSync(() => client2.PutAsync(url, content, timeout));
        }

        public async Task<TResult> PutAsync<TResult>(string url, object content, string correlationId,
            TimeSpan? timeout = null)
            where TResult : class
        {
            var client2 = new HttpRestClient2(correlationId, baseUrl, AuthorizationHeader);
            return await client2.PutAsync<TResult>(url, content, timeout);
        }

        public async Task<TResult> PutAsync<TResult>(string url, string content, string correlationId,
            TimeSpan? timeout = null)
            where TResult : class
        {
            var client2 = new HttpRestClient2(correlationId, baseUrl, AuthorizationHeader);
            return await client2.PutAsync<TResult>(url, content, timeout);
        }

        public async Task<string> PutAsync(string url, string content, string correlationId,
            TimeSpan? timeout = null)
        {
            var client2 = new HttpRestClient2(correlationId, baseUrl, AuthorizationHeader);
            return await client2.PutAsync(url, content, timeout);
        }
        #endregion


        #region Delete, DeleteAsync
        [Obsolete("Consider using 'await DeleteAsync'")]
        public TResult Delete<TResult>(string url, string correlationId,
            TimeSpan? timeout = null)
             where TResult : class
        {
            var client2 = new HttpRestClient2(correlationId, baseUrl, AuthorizationHeader);
            return AsyncHelper.RunSync(() => client2.DeleteAsync<TResult>(url, timeout: timeout));
        }

        [Obsolete("Consider using 'await DeleteAsync'")]
        public string Delete(string url, string correlationId,
            TimeSpan? timeout = null)
        {
            var client2 = new HttpRestClient2(correlationId, baseUrl, AuthorizationHeader);
            return AsyncHelper.RunSync(() => client2.DeleteAsync(url, timeout: timeout));
        }

        public async Task<TResult> DeleteAsync<TResult>(string url, string correlationId,
            TimeSpan? timeout = null)
            where TResult : class
        {
            var client2 = new HttpRestClient2(correlationId, baseUrl, AuthorizationHeader);
            return await client2.DeleteAsync<TResult>(url, timeout: timeout);
        }

        public async Task<string> DeleteAsync(string url, string correlationId,
            TimeSpan? timeout = null)
        {
            var client2 = new HttpRestClient2(correlationId, baseUrl, AuthorizationHeader);
            return await client2.DeleteAsync(url, timeout: timeout);
        }
        #endregion
    }
}
