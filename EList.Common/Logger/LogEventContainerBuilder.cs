using EList.Common.Logger.Containers;

namespace EList.Common.Logger
{
    public static class LogEventContainerBuilder
    {
        public static LogEventContainer Build(string correlationId)
        {
            return new LogEventContainer
            {
                CorrelationId = correlationId
            };
        }
    }
}
