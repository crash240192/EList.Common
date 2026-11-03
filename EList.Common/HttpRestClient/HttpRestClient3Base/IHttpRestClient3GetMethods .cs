namespace EList.Common.HttpRestClient.HttpRestClient3Base
{
#nullable enable

    public partial interface IHttpRestClient3
    {
        Task<HttpResult<string>> GetAsync(string url);

        Task<HttpResult<string>> GetAsync(string url, Dictionary<string, string>? headers = default);

        Task<HttpResult<string>> GetAsync(string url, Dictionary<string, string>? headers = default, CancellationToken? ct = default);

        Task<HttpResult<string>> GetAsync(string url, Dictionary<string, string>? headers = default, TimeSpan? timeout = default);

        Task<HttpResult<T>> GetAsync<T>(string url) where T : class;

        Task<HttpResult<T>> GetAsync<T>(string url, Dictionary<string, string>? headers = default) where T : class;

        Task<HttpResult<T>> GetAsync<T>(string url, Dictionary<string, string>? headers = default, CancellationToken? ct = default) where T : class;

        Task<HttpResult<T>> GetAsync<T>(string url, Dictionary<string, string>? headers = default, TimeSpan? timeout = default) where T : class;
    }

#nullable restore
}