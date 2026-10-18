using UniversityApi.Common;

namespace UniversityApi.Middleware;

/// <summary>Централизованная обработка непредвиденных исключений.</summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Подробности — только в лог
            _logger.LogError(ex, "Unhandled exception. TraceId={TraceId}, {Method} {Path}",
                context.TraceIdentifier, context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var body = ReturnResult<object>.Fail(
                StatusCodes.Status500InternalServerError,
                ErrorCodes.InternalServerError,
                "An unexpected error occurred");
            body.TraceId = context.TraceIdentifier;

            // Клиент получает только безопасный структурированный ответ
            await context.Response.WriteAsJsonAsync(body);
        }
    }
}
