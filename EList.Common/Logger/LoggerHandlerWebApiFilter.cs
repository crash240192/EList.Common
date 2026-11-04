using EList.Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using NLog;

#pragma warning disable CS1591

namespace EList.Common.Logger
{
    public class LoggerHandlerWebApiFilter : ActionFilterAttribute
    {
        private const string REQUEST_CONTEXT_KEY = "request";
        private const string START_DATE_CONTEXT_KEY = "startDate";
        private const string LOGGER_NAME_CONTEXT_KEY = "logger";
        private const string X_FORWARDED_FOR = "X-Forwarded-For";

        private static readonly ILogger nLogger = LogManager.GetCurrentClassLogger();

        private static readonly ILoggerWrapper defaultLogger = new NLogLoggerWrapper(nLogger);

        private readonly ILoggerWrapper logger;

        public LoggerHandlerWebApiFilter()
            : this(defaultLogger)
        {}

        public LoggerHandlerWebApiFilter(ILoggerWrapper logger)
        {
            this.logger = logger;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var debugLevel = Enums.LogLevel.Debug;

            var exception = context?.Exception;
            if (exception == null && !logger.IsEnabled(debugLevel))
                return;

            var correlationId = GetCorrelationId(context.HttpContext);

            var startDate = context.HttpContext.Items[START_DATE_CONTEXT_KEY] as DateTime?;
            var request = context.HttpContext.Items[REQUEST_CONTEXT_KEY]?.ToString();
            var loggerName = context.HttpContext.Items[LOGGER_NAME_CONTEXT_KEY]?.ToString()
                ?? context.HttpContext.Request.Path.ToString();

            context.HttpContext.Request.Headers.TryGetValue(X_FORWARDED_FOR, out var ip);

            var requesterIp = ip.HasStringValue()
                ? ip.ToString()
                : context.HttpContext.Connection.RemoteIpAddress?.ToString();

            var elapsed = startDate.HasValue ? DateTime.UtcNow - startDate : null;

            var response = exception != null
                ? ExceptionFormatter.Format(exception, ExceptionFormatKind.Diagnostic)
                : SerializeForLog(ExtractResponseBody(context.Result));

            var meta = new Dictionary<string, object>
                {
                    { "httpMethod", context.HttpContext.Request.Method }
                };

            if (exception == null)
            {
                var resultStatusCode = context.Result as IStatusCodeActionResult;
                meta.Add("statusCode", resultStatusCode?.StatusCode ?? context.HttpContext.Response.StatusCode);
            }
            else
            {
                meta.Add("StackTrace", exception.StackTrace);
                meta.Add("exceptionChain", response);
            }

            logger.Write(
                exception != null ? Enums.LogLevel.Error : debugLevel,
                correlationId,
                null,
                loggerName,
                request,
                response,
                requesterIp,
                elapsed,
                exception,
                meta);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var debugEnabled = logger.IsEnabled(Enums.LogLevel.Debug);
            var request = debugEnabled ? SerializeForLog(context.ActionArguments) : null;
            var loggerName = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

            context.HttpContext.Items[START_DATE_CONTEXT_KEY] = DateTime.UtcNow;
            context.HttpContext.Items[REQUEST_CONTEXT_KEY] = request;
            context.HttpContext.Items[LOGGER_NAME_CONTEXT_KEY] = loggerName;

            if (debugEnabled)
                logger.Debug(GetCorrelationId(context.HttpContext), null, loggerName, null, "Метод начат");
        }

        private static string GetCorrelationId(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            if (httpContext.Items.TryGetValue(Constants.Constants.HttpHeaderKeys.CORRELATION_ID, out var itemValue) && itemValue != null)
            {
                return itemValue.ToString();
            }

            if (!httpContext.Request.Headers.TryGetValue(Constants.Constants.HttpHeaderKeys.CORRELATION_ID, out var headerValues))
                return null;

            foreach (var value in headerValues)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return null;
        }

        private static string SerializeForLog(object value)
        {
            try
            {
                return JsonConvert.SerializeObject(value);
            }
            catch (Exception failure)
            {
                return $"Не удалось сериализовать '{value.GetType().Name}': {failure.Message}";
            }
        }

        private static object ExtractResponseBody(IActionResult result)
        {
            return result switch
            {
                ObjectResult objectResult => objectResult.Value,
                JsonResult jsonResult => jsonResult.Value,
                ContentResult contentResult => contentResult.Content,
                _ => result
            };
        }

    }
}

#pragma warning restore
