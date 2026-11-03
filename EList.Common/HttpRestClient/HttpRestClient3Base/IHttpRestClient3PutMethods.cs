#nullable enable

namespace EList.Common.HttpRestClient.HttpRestClient3Base
{
    public partial interface IHttpRestClient3
    {
        Task<HttpResult<string>> PutAsync(string url, Dictionary<string, string> headers, object content);
        Task<HttpResult<string>> PutAsync(string url, Dictionary<string, string> headers, object content, CancellationToken? ct = default);
        Task<HttpResult<string>> PutAsync(string url, Dictionary<string, string> headers, object content, TimeSpan? timeout = default);
        Task<HttpResult<string>> PutAsync(string url, object content, CancellationToken? ct = default);
        Task<HttpResult<string>> PutAsync(string url, object content, TimeSpan? timeout = default);
        Task<HttpResult<T>> PutAsync<T>(string url, Dictionary<string, string> headers, object content) where T : class;
        Task<HttpResult<T>> PutAsync<T>(string url, Dictionary<string, string> headers, object content, CancellationToken? ct = default) where T : class;
        Task<HttpResult<T>> PutAsync<T>(string url, Dictionary<string, string> headers, object content, TimeSpan? timeout = default) where T : class;
        Task<HttpResult<T>> PutAsync<T>(string url, object content, CancellationToken? ct = default) where T : class;
        Task<HttpResult<T>> PutAsync<T>(string url, object content, TimeSpan? timeout = default) where T : class;
    }

#nullable restore
}