using System.Net.Http;

namespace EList.Common.HttpRestClient.Exceptions
{
    public class CustomHttpRequestException : HttpRequestException
    {
        public TimeSpan ResponseTime { get; init; }

        public CustomHttpRequestException(string message, Exception? inner, TimeSpan responseTime = default)
            : base(message, inner)
        {
            ResponseTime = responseTime;
        }
    }
}
