using EList.Common.Logger;
using System.Threading;

namespace EList.Common.CorrelationId
{
    public class CorrelationIdProvider : ICorrelationIdProvider
    {
        public string Get()
        {
            var logEventId = Thread.CurrentThread.ManagedThreadId.ToString();
            var result = LogEventHelper.GetVariableValue(logEventId, LogEventVariable.CorrelationId)?.ToString();
            return result;
        }
    }
}
