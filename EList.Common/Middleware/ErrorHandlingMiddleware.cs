using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using System.Text.Json;
using EList.Common.Logger;
using EList.Common.Models;
using Microsoft.AspNetCore.Http;

namespace EList.Common.Middleware
{
    /// <summary>
    /// Перехват необработанных исключений: JSON 400/401/403/500/502 в обёртке CommandResult.
    /// Логирование ошибки — в LoggerHandlerWebApiFilter (для MVC actions).
    /// </summary>
    public sealed class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
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

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = GetStatusCode(exception);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var message = ExceptionFormatter.Format(exception, ExceptionFormatKind.Client);
            var commandResult = CommandResult.Fail(1, message); // стек не отдаем клиенту - он просто будет залогирован

            var body = JsonSerializer.Serialize(commandResult, _jsonOptions);

            return context.Response.WriteAsync(body);
        }

        private static int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                HttpRequestException r when r.StatusCode.HasValue => (int)r.StatusCode.Value,
                HttpRequestException => StatusCodes.Status502BadGateway,        // недоступен и статуса нет
                AuthenticationException => StatusCodes.Status401Unauthorized,
                UnauthorizedAccessException => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };
        }
    }
}
