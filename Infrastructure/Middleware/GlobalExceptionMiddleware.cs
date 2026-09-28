using System.Text.Json;
using Training_Project.Infrastructure.Logging;
using Serilog.Context;

namespace Training_Project.Infrastructure.Middleware;

/// <summary>
/// Global exception handling middleware intercepting unhandled exceptions across the HTTP pipeline.
/// Returns detailed diagnostic error responses (message, exception type, stack trace) exclusively in Development mode,
/// while providing safe, non-leaking error responses in Production.
/// Enriches logging context with categorized ErrorType and TraceId for specialized log file routing.
/// </summary>
public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var errorType = ErrorClassifier.Classify(ex);

            using (LogContext.PushProperty("ErrorType", errorType.ToString()))
            using (LogContext.PushProperty("TraceId", context.TraceIdentifier))
            {
                logger.LogError(ex, "[{ErrorType}] Unhandled exception occurred during request execution for {Method} {Path}",
                    errorType, context.Request.Method, context.Request.Path);
            }

            await HandleExceptionAsync(context, ex, errorType);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, ErrorType errorType)
    {
        if (context.Response.HasStarted)
        {
            logger.LogWarning("The response has already started; the global exception middleware will not write a response.");
            return;
        }

        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        object errorPayload;

        if (environment.IsDevelopment())
        {
            errorPayload = new
            {
                status = StatusCodes.Status500InternalServerError,
                title = "Internal Server Error",
                errorType = errorType.ToString(),
                message = exception.Message,
                exceptionType = exception.GetType().FullName,
                stackTrace = exception.StackTrace,
                innerException = exception.InnerException?.Message
            };
        }
        else
        {
            errorPayload = new
            {
                status = StatusCodes.Status500InternalServerError,
                title = "Internal Server Error",
                message = "An unexpected error occurred while processing your request. Please contact technical support."
            };
        }

        var json = JsonSerializer.Serialize(errorPayload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = environment.IsDevelopment()
        });

        await context.Response.WriteAsync(json);
    }
}
