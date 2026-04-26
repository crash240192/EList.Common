using Newtonsoft.Json;
using NLog;
using Microsoft.AspNetCore.Http;
using EList.Common.Logger.Containers;

namespace EList.Common.Logger
{
    public class NLogLoggerWrapper : ILoggerWrapper
    {
        private readonly ILogger logger;
        private readonly ILogger failsafeLogger = LogManager.GetCurrentClassLogger();
        private IHttpContextAccessor httpContextAccessor;

        public NLogLoggerWrapper(ILogger logger)
        {
            this.logger = logger;
        }

        public void Configure(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public void Write(
            Enums.LogLevel level,
            string? correlationId,
            string? token,
            string? loggerName, // methodName or responseBody
            string? incomingMessage, // requestBody
            string? message,
            string? clientIP = null,
            TimeSpan? execTime = null,
            Exception? exception = null,
            IDictionary<string, object>? meta = null)
        {
            try
            {
                var logEvent = new LogEventInfo();

                logEvent.Level = LogLevel.FromString(level.ToString());

                logEvent.Properties.Add("correlationId", correlationId);

                logEvent.Properties.Add("token", token);

                logEvent.LoggerName = loggerName;

                if (!string.IsNullOrWhiteSpace(incomingMessage))
                    logEvent.Properties.Add("incomingMessage", incomingMessage.Trim());

                logEvent.Message = message?.Trim();

                if (execTime.HasValue)
                {
                    logEvent.Properties.Add("execTime", (int)execTime.Value.TotalMilliseconds);
                }

                if (exception != null)
                    logEvent.Exception = exception;

                if (meta != null)
                    logEvent.Properties.Add("meta", JsonConvert.SerializeObject(meta));

                if (string.IsNullOrWhiteSpace(clientIP))
                {
                    clientIP = httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                }

                logEvent.Properties.Add("clientIP", clientIP);

                logger.Log(logEvent);
            }
            catch (Exception ex)
            {
                failsafeLogger.Error(ex, "Failed to write LogEventInfo to NLog logger");
            }
        }

        public void ValidationWarn(
            string? correlationId,
            string? systemGuid,
            string? methodName, // loggerName
            string? message, // incomingmessage
            IEnumerable<string>? errorMessages,
            string? clientIP = null,
            IDictionary<string, object>? meta = null)
        {
            Write(Enums.LogLevel.Warn, correlationId, systemGuid, methodName,
                null, $"{message}: {string.Join("; ", errorMessages)}", clientIP: clientIP);
        }

        public void ExternalCallRequestInfo(
            string? correlationId,
            string? externalServiceSystemGuid, // systemguid
            string? requestUrl, // logger
            string? requestBody, // incomingmessage
            string? clientIP = null)
        {
            Write(Enums.LogLevel.Info, correlationId, externalServiceSystemGuid, requestUrl,
                requestBody, string.Empty, clientIP: clientIP);
        }

        public void ExternalCallResponseInfo(
            string? correlationId,
            string? externalServiceSystemGuid, // systemguid
            string? requestUrl, // logger
            string? requestBody, //incomingmessage
            string? responseBody, // message
            TimeSpan? execTime, // exectime
            string? clientIP = null,
            IDictionary<string, object>? meta = null)
        {
            Write(Enums.LogLevel.Info, correlationId, externalServiceSystemGuid, requestUrl,
                requestBody, responseBody, clientIP, execTime, meta: meta);
        }

        public void ExternalCallResponseError(
            string? correlationId,
            string? externalServiceSystemGuid, // systemguid
            string? requestUrl, // logger 
            string? requestBody, //incomingmessage
            string? responseBody, // message
            TimeSpan? execTime,
            Exception? exception,
            string? clientIP = null,
            IDictionary<string, object>? meta = null)
        {
            Write(Enums.LogLevel.Error, correlationId, externalServiceSystemGuid, requestUrl,
                requestBody, responseBody, clientIP, execTime, exception, meta: meta);
        }

        public void Trace(
            string? correlationId,
            string? systemGuid,
            string? methodName, // loggerName
            string? message,
            string? clientIP = null,
            TimeSpan? execTime = null,
            IDictionary<string, object>? meta = null)
        {
            Write(Enums.LogLevel.Trace, correlationId, systemGuid, methodName,
                null, message, clientIP, execTime, null, meta);
        }

        public void Debug(
            string? correlationId,
            string? systemGuid,
            string? methodName, // logger
            string? message,
            string? clientIP = null,
            TimeSpan? execTime = null,
            IDictionary<string, object>? meta = null)
        {
            Write(Enums.LogLevel.Debug, correlationId, systemGuid, methodName,
                null, message, clientIP, execTime, null, meta);
        }

        public void Info(
            string? correlationId,
            string? systemGuid,
            string? methodName, // logger
            string? message,
            string? clientIP = null,
            TimeSpan? execTime = null,
            IDictionary<string, object>? meta = null,
            string? incomingMessage = null)
        {
            Write(Enums.LogLevel.Info, correlationId, systemGuid, methodName,
                incomingMessage, message, clientIP, execTime, null, meta);
        }

        public void Warn(
            string? correlationId,
            string? systemGuid,
            string? methodName, // loggerName
            string? message,
            string? clientIP = null,
            TimeSpan? execTime = null,
            IDictionary<string, object>? meta = null)
        {
            Write(Enums.LogLevel.Warn, correlationId, systemGuid, methodName,
                null, message, clientIP, execTime, null, meta);
        }

        public void Error(
            string? correlationId,
            string? systemGuid,
            string? methodName, // loggerName
            string? message,
            TimeSpan? execTime,
            Exception? exception,
            string? clientIP = null,
            IDictionary<string, object>? meta = null,
            string? incomingMessage = null)
        {
            Write(Enums.LogLevel.Error, correlationId, systemGuid, methodName,
                incomingMessage, message, clientIP, execTime, exception, meta);
        }

        #region Logs with requestId
        public void Write(
            Enums.LogLevel level,
            RequestIdContainer? requestId,
            string? correlationId,
            string? systemGuid,
            string? loggerName,
            string? incomingMessage,
            string? message,
            string? clientIP = null,
            TimeSpan? execTime = null,
            Exception? exception = null,
            IDictionary<string, object>? meta = null)
        {
            try
            {
                var logEvent = new LogEventInfo();

                logEvent.Level = LogLevel.FromString(level.ToString());

                logEvent.Properties.Add("correlationId", correlationId);

                logEvent.Properties.Add("systemGuid", systemGuid);

                logEvent.LoggerName = loggerName;

                if (!string.IsNullOrWhiteSpace(incomingMessage))
                    logEvent.Properties.Add("incomingMessage", incomingMessage.Trim());

                logEvent.Message = message?.Trim();

                if (execTime.HasValue)
                {
                    logEvent.Properties.Add("execTime", (int)execTime.Value.TotalMilliseconds);
                }

                if (exception != null)
                    logEvent.Exception = exception;

                if (meta != null)
                    logEvent.Properties.Add("meta", JsonConvert.SerializeObject(meta));

                if (string.IsNullOrWhiteSpace(clientIP))
                {
                    clientIP = httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                }

                logEvent.Properties.Add("clientIP", clientIP);

                if (requestId != null)
                    logEvent.Properties.Add("requestId", requestId.RequestId);

                logger.Log(logEvent);
            }
            catch (Exception ex)
            {
                failsafeLogger.Error(ex, "Failed to write LogEventInfo to NLog logger");
            }
        }

        public void Trace(
            RequestIdContainer? requestId,
            string? correlationId,
            string? systemGuid,
            string? loggerName,
            string? message,
            string? clientIP = null,
            TimeSpan? execTime = null,
            IDictionary<string, object>? meta = null)
        {
            Write(Enums.LogLevel.Trace, requestId, correlationId, systemGuid, loggerName,
                null, message, clientIP, execTime, null, meta);
        }

        public void Debug(
            RequestIdContainer? requestId,
            string? correlationId,
            string? systemGuid,
            string? loggerName,
            string? message,
            string? clientIP = null,
            TimeSpan? execTime = null,
            IDictionary<string, object>? meta = null)
        {
            Write(Enums.LogLevel.Debug, requestId, correlationId, systemGuid, loggerName,
                null, message, clientIP, execTime, null, meta);
        }

        public void Info(
            RequestIdContainer? requestId,
            string? correlationId,
            string? systemGuid,
            string? loggerName,
            string? message,
            string? clientIP = null,
            TimeSpan? execTime = null,
            IDictionary<string, object>? meta = null,
            string? incomingMessage = null)
        {
            Write(Enums.LogLevel.Info, requestId, correlationId, systemGuid, loggerName,
               incomingMessage, message, clientIP, execTime, null, meta);
        }

        public void Warn(
            RequestIdContainer? requestId,
            string? correlationId,
            string? systemGuid,
            string? loggerName,
            string? message,
            string? clientIP = null,
            TimeSpan? execTime = null,
            IDictionary<string, object>? meta = null)
        {
            Write(Enums.LogLevel.Warn, requestId, correlationId, systemGuid, loggerName,
                null, message, clientIP, execTime, null, meta);
        }

        public void Error(
            RequestIdContainer? requestId,
            string? correlationId,
            string? systemGuid,
            string? loggerName,
            string? message,
            TimeSpan? execTime,
            Exception? exception,
            string? clientIP = null,
            IDictionary<string, object>? meta = null,
            string? incomingMessage = null)
        {
            Write(Enums.LogLevel.Error, requestId, correlationId, systemGuid, loggerName,
                incomingMessage, message, clientIP, execTime, exception, meta);
        }
        #endregion
    }
}
