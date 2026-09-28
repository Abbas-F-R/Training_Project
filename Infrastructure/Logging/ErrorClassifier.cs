using Microsoft.AspNetCore.Http;

namespace Training_Project.Infrastructure.Logging;

/// <summary>
/// Evaluates exceptions and HTTP contexts to categorize errors into structured <see cref="ErrorType"/>s.
/// Used to route logs to specialized log files and enrich telemetry.
/// </summary>
public static class ErrorClassifier
{
    /// <summary>
    /// Classifies an exception and optional HTTP status code into an <see cref="ErrorType"/>.
    /// </summary>
    public static ErrorType Classify(Exception? exception, int? statusCode = null)
    {
        if (statusCode.HasValue)
        {
            if (statusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden)
                return ErrorType.Security;

            if (statusCode is StatusCodes.Status400BadRequest or StatusCodes.Status422UnprocessableEntity)
                return ErrorType.Validation;

            if (statusCode is StatusCodes.Status404NotFound)
                return ErrorType.NotFound;
        }

        if (exception == null)
            return ErrorType.Unhandled;

        if (IsDatabaseException(exception))
            return ErrorType.Database;

        if (IsSecurityException(exception))
            return ErrorType.Security;

        if (IsValidationException(exception))
            return ErrorType.Validation;

        if (IsNotFoundException(exception))
            return ErrorType.NotFound;

        return ErrorType.Unhandled;
    }

    /// <summary>
    /// Inspects the exception chain to identify database connectivity, SQL Server, or query errors.
    /// </summary>
    public static bool IsDatabaseException(Exception ex)
    {
        var current = (Exception?)ex;
        while (current != null)
        {
            var typeName = current.GetType().FullName ?? string.Empty;
            if (typeName.Contains("SqlClient", StringComparison.OrdinalIgnoreCase) ||
                typeName.Contains("DataException", StringComparison.OrdinalIgnoreCase) ||
                typeName.Contains("DbException", StringComparison.OrdinalIgnoreCase) ||
                typeName.Contains("TimeoutException", StringComparison.OrdinalIgnoreCase) ||
                current.Message.Contains("SQL Server", StringComparison.OrdinalIgnoreCase) ||
                current.Message.Contains("Named Pipes Provider", StringComparison.OrdinalIgnoreCase) ||
                current.Message.Contains("TCP Provider", StringComparison.OrdinalIgnoreCase) ||
                current.Message.Contains("Could not open a connection", StringComparison.OrdinalIgnoreCase) ||
                current.Message.Contains("deadlock", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            current = current.InnerException;
        }
        return false;
    }

    /// <summary>
    /// Inspects the exception chain to identify authentication, authorization, or token validation errors.
    /// </summary>
    public static bool IsSecurityException(Exception ex)
    {
        var current = (Exception?)ex;
        while (current != null)
        {
            var typeName = current.GetType().FullName ?? string.Empty;
            if (current is UnauthorizedAccessException ||
                typeName.Contains("SecurityToken", StringComparison.OrdinalIgnoreCase) ||
                typeName.Contains("AuthenticationException", StringComparison.OrdinalIgnoreCase) ||
                typeName.Contains("Forbidden", StringComparison.OrdinalIgnoreCase) ||
                typeName.Contains("SecurityException", StringComparison.OrdinalIgnoreCase) ||
                current.Message.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase) ||
                current.Message.Contains("Forbidden", StringComparison.OrdinalIgnoreCase) ||
                current.Message.Contains("JWT", StringComparison.OrdinalIgnoreCase) ||
                current.Message.Contains("token", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            current = current.InnerException;
        }
        return false;
    }

    /// <summary>
    /// Inspects the exception chain to identify input validation, schema, or argument constraint failures.
    /// </summary>
    public static bool IsValidationException(Exception ex)
    {
        var current = (Exception?)ex;
        while (current != null)
        {
            var typeName = current.GetType().FullName ?? string.Empty;
            if (typeName.Contains("ValidationException", StringComparison.OrdinalIgnoreCase) ||
                current is ArgumentException ||
                current is FormatException)
            {
                return true;
            }
            current = current.InnerException;
        }
        return false;
    }

    /// <summary>
    /// Inspects the exception chain to identify missing resource errors.
    /// </summary>
    public static bool IsNotFoundException(Exception ex)
    {
        var current = (Exception?)ex;
        while (current != null)
        {
            if (current is KeyNotFoundException)
                return true;

            var typeName = current.GetType().FullName ?? string.Empty;
            if (typeName.Contains("NotFoundException", StringComparison.OrdinalIgnoreCase))
                return true;

            current = current.InnerException;
        }
        return false;
    }
}
