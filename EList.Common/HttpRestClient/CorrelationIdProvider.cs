using EList.Common.CorrelationId;
using Microsoft.AspNetCore.Http;

namespace EList.Common.HttpRestClient
{
    public class CorrelationIdProvider : ICorrelationIdProvider
    {
        private const string CORRELATION_ID_CONTEXT_KEY = "correlationId";

        private readonly IHttpContextAccessor httpContextAccessor;

        public CorrelationIdProvider(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public string Get()
        {
            if (httpContextAccessor?.HttpContext?.Items?.ContainsKey(CORRELATION_ID_CONTEXT_KEY) == true)
            {
                return httpContextAccessor.HttpContext.Items[CORRELATION_ID_CONTEXT_KEY].ToString();
            }
            else
            {
                return null;
            }
        }
    }
}
