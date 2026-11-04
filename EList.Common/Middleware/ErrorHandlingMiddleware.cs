using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using System.Text.Json;
using EList.Common.Logger;
using EList.Common.Models;
using EList.Common.Support;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using ConfigurationManager = EList.Common.Configuration.ConfigurationManager;

namespace EList.Common.Middleware
{
    /// <summary>
    /// Перехват необработанных исключений: JSON 400/401/403/500/502 в обёртке CommandResult.
    /// Стек клиенту — только в Development/Staging или при features:exposeDetailedErrors.
    /// Логирование MVC-ошибок — в LoggerHandlerWebApiFilter.
    /// </summary>
    public sealed class ErrorHandlingMiddleware
    {
        public const string CorrelationIdHeaderName = "X-Correlation-Id";

        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _environment;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public ErrorHandlingMiddleware(RequestDelegate next, IHostEnvironment environment)
        {
            _next = next;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = GetStatusCode(exception);
            var exposeDetails = ShouldExposeDetailedErrors(_environment);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            if (context.Items.TryGetValue(Constants.Constants.HttpHeaderKeys.CORRELATION_ID, out var corrObj)
                && corrObj is string correlationId
                && !string.IsNullOrWhiteSpace(correlationId)
                && !context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
            {
                context.Response.Headers[CorrelationIdHeaderName] = correlationId;
            }

            string message;
            string? stackTrace = null;

            if (exposeDetails)
            {
                message = ExceptionFormatter.Format(exception, ExceptionFormatKind.Diagnostic)
                          ?? exception.Message;
                stackTrace = exception.ToString();
            }
            else if (statusCode is >= StatusCodes.Status400BadRequest and < StatusCodes.Status500InternalServerError)
            {
                // 4xx — клиенту полезен короткий текст без стека
                message = ExceptionFormatter.Format(exception, ExceptionFormatKind.Client)
                          ?? exception.Message;
            }
            else
            {
                message = "Внутренняя ошибка сервера. Обратитесь в поддержку и укажите correlation id.";
            }

            var commandResult = stackTrace != null
                ? CommandResult.Fail((int)ErrorCode.InternalError, message, stackTrace)
                : CommandResult.Fail((int)ErrorCode.InternalError, message);
            var body = JsonSerializer.Serialize(commandResult, JsonOptions);
            return context.Response.WriteAsync(body);
        }

        /// <summary>
        /// Development и Staging — детали по умолчанию.
        /// Production — только если features:exposeDetailedErrors=true.
        /// </summary>
        internal static bool ShouldExposeDetailedErrors(IHostEnvironment environment)
        {
            if (environment.IsDevelopment() || environment.IsEnvironment("Staging"))
                return true;

            if (!ConfigurationManager.AppSettings.Contains("features:exposeDetailedErrors"))
                return false;

            return bool.TryParse(
                       ConfigurationManager.AppSettings["features:exposeDetailedErrors"],
                       out var enabled)
                   && enabled;
        }

        private static int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                HttpRequestException r when r.StatusCode.HasValue => (int)r.StatusCode.Value,
                HttpRequestException => StatusCodes.Status502BadGateway,
                AuthenticationException => StatusCodes.Status401Unauthorized,
                UnauthorizedAccessException => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };
        }
    }
}
