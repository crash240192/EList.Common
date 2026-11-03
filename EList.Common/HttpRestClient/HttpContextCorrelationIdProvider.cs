using EList.Common.CorrelationId;
using Microsoft.AspNetCore.Http;

namespace EList.Common.HttpRestClient
{
    public class HttpContextCorrelationIdProvider : ICorrelationIdProvider
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpContextCorrelationIdProvider(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public string Get()
        {
            var items = httpContextAccessor?.HttpContext?.Items;
            if (items == null)
                return CorrelationIdScope.Current;

            if (!items.TryGetValue(Constants.Constants.HttpHeaderKeys.CORRELATION_ID, out var value) || value == null)
                return CorrelationIdScope.Current;

            var text = value.ToString();
            return string.IsNullOrWhiteSpace(text) ? CorrelationIdScope.Current : text;
        }
    }
}
