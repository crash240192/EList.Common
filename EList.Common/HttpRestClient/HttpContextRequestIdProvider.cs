using EList.Common.Logger;
using Microsoft.AspNetCore.Http;

namespace EList.Common.HttpRestClient
{
    public class HttpContextRequestIdProvider : IRequestIdProvider
    {
        private const string REQUEST_ID_CONTEXT_KEY = "X-Request-Id";

        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpContextRequestIdProvider(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public string? Get()
        {
            if (httpContextAccessor?.HttpContext?.Items?.ContainsKey(REQUEST_ID_CONTEXT_KEY) == true)
            {
                return httpContextAccessor.HttpContext.Items[REQUEST_ID_CONTEXT_KEY]?.ToString();
            }

            return null;
        }
    }
}
