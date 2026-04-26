using EList.Common.Logger.Enums;

namespace EList.Common.Logger.Containers
{
    public class LogEventContainer
    {
        public string CorrelationId { get; set; }

        public string Guid { get; set; }

        public string LoggerName { get; set; }

        public LogLevel LogLevel { get; set; }

        public string ClientIp { get; set; }

        public string IncommingMessage { get; set; }

        public string Message { get; set; }
        
        public RequestIdContainer RequestId { get; set; }

        public DateTime? StartEventTime { get; set; }

        public DateTime? EndEventTime { get; set; }
    }
}
