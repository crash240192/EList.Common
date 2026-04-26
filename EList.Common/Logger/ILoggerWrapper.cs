using System;
using System.Collections.Generic;
using EList.Common.Logger;
using EList.Common.Logger.Containers;
using EList.Common.Logger.Enums;
using Microsoft.AspNetCore.Http;

namespace EList.Common.Logger
{
    public interface ILoggerWrapper
    {
        void Configure(IHttpContextAccessor httpContextAccessor);

        void Write(
            LogLevel level,
            string? correlationId,
            string? systemGuid,
            string? loggerName, // loggerName or responseBody
            string? incomingMessage, // requestBody
            string? message,
            string? clientIP = null,
            TimeSpan? execTime = null,
            Exception? exception = null,
            IDictionary<string, object>? meta = null);

        void ValidationWarn(
            string? correlationId,
            string? systemGuid,
            string? loggerName, // loggerName
            string? message,
            IEnumerable<string>? errorMessages,
            string? clientIP = null,
            IDictionary<string, object>? meta = null);

        void ExternalCallRequestInfo(
            string? correlationId,
            string? externalServiceSystemGuid, // systemguid
            string? requestUrl, // loggerName
            string? requestBody, // incomingmessage
            string? clientIP = null);

        void ExternalCallResponseInfo(
            string? correlationId,
            string? externalServiceSystemGuid, // systemguid
            string? requestUrl, // logger
            string? requestBody, //incomingmessage
            string? responseBody, // message
            TimeSpan? execTime, // exectime
            string? clientIP = null,
            IDictionary<string, object>? meta = null);

        void ExternalCallResponseError(
            string? correlationId,
            string? externalServiceSystemGuid, // systemguid
            string? requestUrl, // logger 
            string? requestBody, //incomingmessage
            string? responseBody, // message
            TimeSpan? execTime,
            Exception? exception,
            string? clientIP = null,
            IDictionary<string, object>? meta = null);

        void Trace(
            string? correlationId,
            string? systemGuid,
            string? loggerName, // loggerName
            string? message,
            string? clientIP = null,
            TimeSpan? execTime = null,
            IDictionary<string, object>? meta = null);

        void Debug(
            string? correlationId,
            string? systemGuid,
            string? loggerName, // logger
            string? message,
            string? clientIP = null,
            TimeSpan? execTime = null,
            IDictionary<string, object>? meta = null);

        void Info(
            string? correlationId,
            string? systemGuid,
            string? loggerName, // logger
            string? message,
            string? clientIP = null,
            TimeSpan? execTime = null,
            IDictionary<string, object>? meta = null,
            string? incomingMessage = null);

        void Warn(
            string? correlationId,
            string? systemGuid,
            string? loggerName, // loggerName
            string? message,
            string? clientIP = null,
            TimeSpan? execTime = null,
            IDictionary<string, object>? meta = null);

        void Error(
            string? correlationId,
            string? systemGuid,
            string? loggerName, // loggerName
            string? message,
            TimeSpan? execTime,
            Exception exception,
            string? clientIP = null,
            IDictionary<string, object>? meta = null,
            string? incomingMessage = null);

        #region Logs with requestId
        void Write(
            LogLevel level,
            RequestIdContainer? requestId,
            string? correlationId,
            string? systemGuid,
            string? loggerName,
            string? incomingMessage,
            string? message,
            string? clientIP = null,
            TimeSpan? execTime = null,
            Exception? exception = null,
            IDictionary<string, object>? meta = null);

        void Trace(
            RequestIdContainer? requestId,
            string? correlationId,
            string? systemGuid,
            string? loggerName,
            string? message,
            string? clientIP = null,
            TimeSpan? execTime = null,
            IDictionary<string, object>? meta = null);

        void Debug(
            RequestIdContainer? requestId,
            string? correlationId,
            string? systemGuid,
            string? loggerName,
            string? message,
            string? clientIP = null,
            TimeSpan? execTime = null,
            IDictionary<string, object>? meta = null);

        void Info(
            RequestIdContainer? requestId,
            string? correlationId,
            string? systemGuid,
            string? loggerName,
            string? message,
            string? clientIP = null,
            TimeSpan? execTime = null,
            IDictionary<string, object>? meta = null,
            string? incomingMessage = null);

        void Warn(
            RequestIdContainer? requestId,
            string? correlationId,
            string? systemGuid,
            string? loggerName,
            string? message,
            string? clientIP = null,
            TimeSpan? execTime = null,
            IDictionary<string, object>? meta = null);

        void Error(
            RequestIdContainer? requestId,
            string? correlationId,
            string? systemGuid,
            string? loggerName,
            string? message,
            TimeSpan? execTime,
            Exception? exception,
            string? clientIP = null,
            IDictionary<string, object>? meta = null,
            string? incomingMessage = null);
        #endregion
    }
}
