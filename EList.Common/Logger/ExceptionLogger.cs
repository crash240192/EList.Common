using System.Text;

namespace EList.Common.Logger
{
    public static class ExceptionLogger
    {
        public static int LogException(
            ILoggerWrapper logger,
            string correlationId,
            string loggerName,
            string message,
            TimeSpan execTime,
            Exception ex,
            Dictionary<string, object> meta = null,
            string incomingMessage = null,
            int level = 0)
        {
            if (ex is AggregateException)
            {
                ((AggregateException)ex).InnerExceptions?.Select(innerEx =>
                    LogException(logger, correlationId, loggerName, message, execTime, innerEx, meta, incomingMessage, level + 1));
            }

            if (ex.InnerException != null)
            {
                LogException(logger, correlationId, loggerName, message, execTime, ex.InnerException, meta, incomingMessage, level + 1);
            }

            var levelStr = level > 0 ? $" (inner #{level})" : string.Empty;
            var exMessage = $"{message}{levelStr}: {ex.Message}";

            logger.Error(correlationId, null, loggerName, exMessage, execTime, ex, null, meta, incomingMessage);

            return 0;
        }

        /// <summary>
        /// Собирает в кучу весь текст ошибки
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string GetFullMessageText(Exception ex)
        {
            var exceptionMessage = new StringBuilder();
            if (ex.InnerException != null)
                exceptionMessage.AppendLine(GetFullMessageText(ex.InnerException));
            exceptionMessage.AppendLine(ex.Message);
            return exceptionMessage.ToString();
        }
    }
}
