namespace EList.Common.HttpRestClient.HttpRestClient3Base
{
#nullable enable

    public partial interface IHttpRestClient3
    {
        Task<HttpResult<string>> PostFormDataAsync(string url, Dictionary<string, string> headers, object content);

        Task<HttpResult<string>> PostFormDataAsync(string url, Dictionary<string, string> headers, object content, CancellationToken? ct = default);

        Task<HttpResult<string>> PostFormDataAsync(string url, Dictionary<string, string> headers, object content, TimeSpan? timeout = default);

        Task<HttpResult<string>> PostFormDataAsync(string url, object content, CancellationToken? ct = default);

        Task<HttpResult<string>> PostFormDataAsync(string url, object content, TimeSpan? timeout = default);

        Task<HttpResult<T>> PostFormDataAsync<T>(string url, Dictionary<string, string> headers, object content) where T : class;

        Task<HttpResult<T>> PostFormDataAsync<T>(string url, Dictionary<string, string> headers, object content, CancellationToken? ct = default) where T : class;

        Task<HttpResult<T>> PostFormDataAsync<T>(string url, Dictionary<string, string> headers, object content, TimeSpan? timeout = default) where T : class;

        Task<HttpResult<T>> PostFormDataAsync<T>(string url, object content, CancellationToken? ct = default) where T : class;

        Task<HttpResult<T>> PostFormDataAsync<T>(string url, object content, TimeSpan? timeout = default) where T : class;
    }

#nullable restore
}