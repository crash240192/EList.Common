using System.Net;

#nullable enable
namespace EList.Common.HttpRestClient.HttpRestClient3Base
{
    public class HttpResult<T> where T : class
    {
        public HttpStatusCode StatusCode { get; init; }
        public Dictionary<string, string>? ResponseHeaders { get; init; }
        public TimeSpan ResponseTime { get; init; }
        public T? Result { get; init; }

        public static HttpResult<T> Create(
            HttpStatusCode statusCode,
            Dictionary<string, string> headers,
            TimeSpan responseTime,
            T? result = null)
        {
            return new HttpResult<T>
            {
                StatusCode = statusCode,
                ResponseHeaders = headers,
                ResponseTime = responseTime,
                Result = result,
            };
        }
    }
}
