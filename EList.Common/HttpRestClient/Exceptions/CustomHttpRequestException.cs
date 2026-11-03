using System;
using System.Net.Http;

namespace N3.Common.Net.Exceptions;

public class CustomHttpRequestException : HttpRequestException
{
    public TimeSpan ResponseTime { get; init; }
    public CustomHttpRequestException(string message, Exception inner, TimeSpan responseTime = default)
        : base(message, inner)
    {
        ResponseTime = responseTime;
    }
}