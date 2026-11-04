using Microsoft.AspNetCore.Http;

namespace EList.Common.Middleware
{
    /// <summary>
    /// Кладёт correlationId в <see cref="HttpContext.Items"/>:
    /// из заголовка (X-Correlation-Id / correlationId), иначе новый Guid.
    /// Чтение — через <see cref="EList.Common.HttpRestClient.HttpContextCorrelationIdProvider"/>.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        public const string CorrelationIdHeaderName = "X-Correlation-Id";

        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

        public Task InvokeAsync(HttpContext context)
        {
            var correlationId = ResolveCorrelationId(context);
            context.Items[Constants.Constants.HttpHeaderKeys.CORRELATION_ID] = correlationId;

            if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
                context.Response.Headers[CorrelationIdHeaderName] = correlationId;

            return _next.Invoke(context);
        }

        private static string ResolveCorrelationId(HttpContext context)
        {
            if (context.Items.TryGetValue(Constants.Constants.HttpHeaderKeys.CORRELATION_ID, out var existing)
                && existing is string existingString
                && !string.IsNullOrWhiteSpace(existingString))
            {
                return existingString;
            }

            var fromHeader =
                GetHeader(context, CorrelationIdHeaderName)
                ?? GetHeader(context, Constants.Constants.HttpHeaderKeys.CORRELATION_ID);

            return string.IsNullOrWhiteSpace(fromHeader)
                ? Guid.NewGuid().ToString()
                : fromHeader;
        }

        private static string? GetHeader(HttpContext context, string name)
        {
            if (!context.Request.Headers.TryGetValue(name, out var headerValues))
                return null;

            foreach (var value in headerValues)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return null;
        }
    }
}
