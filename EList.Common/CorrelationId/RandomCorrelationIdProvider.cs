using System;

namespace EList.Common.CorrelationId
{
    public class RandomCorrelationIdProvider : ICorrelationIdProvider
    {
        public string Get()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
