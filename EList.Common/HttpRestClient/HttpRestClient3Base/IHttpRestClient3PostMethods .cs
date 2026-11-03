namespace EList.Common.HttpRestClient.HttpRestClient3Base
{
#nullable enable

    public partial interface IHttpRestClient3
    {
        Task<HttpResult<string>> PostAsync(string url, Dictionary<string, string> headers, FormUrlEncodedContent content);

        Task<HttpResult<string>> PostAsync(string url, Dictionary<string, string> headers, FormUrlEncodedContent content, CancellationToken? ct = default);

        Task<HttpResult<string>> PostAsync(string url, Dictionary<string, string> headers, FormUrlEncodedContent content, TimeSpan? timeout = default);

        Task<HttpResult<string>> PostAsync(string url, Dictionary<string, string> headers, object content);

        Task<HttpResult<string>> PostAsync(string url, Dictionary<string, string> headers, object content, CancellationToken? ct = default);

        Task<HttpResult<string>> PostAsync(string url, Dictionary<string, string> headers, object content, TimeSpan? timeout = default);

        Task<HttpResult<string>> PostAsync(string url, FormUrlEncodedContent content, CancellationToken? ct = default);

        Task<HttpResult<string>> PostAsync(string url, FormUrlEncodedContent content, TimeSpan? timeout = default);

        Task<HttpResult<string>> PostAsync(string url, object content, CancellationToken? ct = default);

        Task<HttpResult<string>> PostAsync(string url, object content, TimeSpan? timeout = default);

        Task<HttpResult<T>> PostAsync<T>(string url, Dictionary<string, string> headers, FormUrlEncodedContent content) where T : class;

        Task<HttpResult<T>> PostAsync<T>(string url, Dictionary<string, string> headers, FormUrlEncodedContent content, CancellationToken? ct = default) where T : class;

        Task<HttpResult<T>> PostAsync<T>(string url, Dictionary<string, string> headers, FormUrlEncodedContent content, TimeSpan? timeout = default) where T : class;

        Task<HttpResult<T>> PostAsync<T>(string url, Dictionary<string, string> headers, object content) where T : class;

        Task<HttpResult<T>> PostAsync<T>(string url, Dictionary<string, string> headers, object content, CancellationToken? ct = default) where T : class;

        Task<HttpResult<T>> PostAsync<T>(string url, Dictionary<string, string> headers, object content, TimeSpan? timeout = default) where T : class;

        Task<HttpResult<T>> PostAsync<T>(string url, FormUrlEncodedContent content, CancellationToken? ct = default) where T : class;

        Task<HttpResult<T>> PostAsync<T>(string url, FormUrlEncodedContent content, TimeSpan? timeout = default) where T : class;

        Task<HttpResult<T>> PostAsync<T>(string url, object content, CancellationToken? ct = default) where T : class;

        Task<HttpResult<T>> PostAsync<T>(string url, object content, TimeSpan? timeout = default) where T : class;
    }

#nullable restore
}