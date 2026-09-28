using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace Training_Project.Infrastructure.Logging;

/// <summary>
/// Configures and registers Serilog structured file logging partitioned by error types.
/// Incorporates asynchronous non-blocking disk I/O, daily rolling partitions, retention limits,
/// and automated error categorization into specialized log sinks.
/// </summary>
public static class SerilogLoggingExtensions
{
    /// <summary>
    /// Configures Serilog as the primary logging provider with partitioned error-type file sinks.
    /// </summary>
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        var loggingOptions = builder.Configuration
            .GetSection(LoggingOptions.SectionName)
            .Get<LoggingOptions>() ?? new LoggingOptions();

        var logsFolder = Path.Combine(builder.Environment.ContentRootPath, loggingOptions.LogsFolder);
        var errorsFolder = Path.Combine(logsFolder, "errors");

        Directory.CreateDirectory(logsFolder);
        Directory.CreateDirectory(errorsFolder);

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            // 1. Console Output for developer visibility
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            // 2. Global Rolling Application Log (all Information and above)
            .WriteTo.Async(a => a.File(
                path: Path.Combine(logsFolder, "app-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: loggingOptions.RetainedFileCountLimit,
                fileSizeLimitBytes: loggingOptions.FileSizeLimitBytes,
                rollOnFileSizeLimit: true,
                outputTemplate: loggingOptions.OutputTemplate,
                shared: true))
            // 3. Consolidated Errors Log (all Warning, Error, Fatal)
            .WriteTo.Logger(sub => sub
                .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Warning)
                .WriteTo.Async(a => a.File(
                    path: Path.Combine(errorsFolder, "all-errors-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: loggingOptions.RetainedFileCountLimit,
                    fileSizeLimitBytes: loggingOptions.FileSizeLimitBytes,
                    rollOnFileSizeLimit: true,
                    outputTemplate: loggingOptions.OutputTemplate,
                    shared: true)))
            // 4. Dedicated Database Errors Log (SQL Server, connectivity, query execution)
            .WriteTo.Logger(sub => sub
                .Filter.ByIncludingOnly(e => MatchesErrorType(e, ErrorType.Database) ||
                                             (e.Exception != null && ErrorClassifier.IsDatabaseException(e.Exception)))
                .WriteTo.Async(a => a.File(
                    path: Path.Combine(errorsFolder, "database-errors-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: loggingOptions.RetainedFileCountLimit,
                    fileSizeLimitBytes: loggingOptions.FileSizeLimitBytes,
                    rollOnFileSizeLimit: true,
                    outputTemplate: loggingOptions.OutputTemplate,
                    shared: true)))
            // 5. Dedicated Security Errors Log (Authentication, Authorization, Tokens)
            .WriteTo.Logger(sub => sub
                .Filter.ByIncludingOnly(e => MatchesErrorType(e, ErrorType.Security) ||
                                             (e.Exception != null && ErrorClassifier.IsSecurityException(e.Exception)))
                .WriteTo.Async(a => a.File(
                    path: Path.Combine(errorsFolder, "security-errors-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: loggingOptions.RetainedFileCountLimit,
                    fileSizeLimitBytes: loggingOptions.FileSizeLimitBytes,
                    rollOnFileSizeLimit: true,
                    outputTemplate: loggingOptions.OutputTemplate,
                    shared: true)))
            // 6. Dedicated Validation Errors Log (Input rules, malformed payloads)
            .WriteTo.Logger(sub => sub
                .Filter.ByIncludingOnly(e => MatchesErrorType(e, ErrorType.Validation) ||
                                             (e.Exception != null && ErrorClassifier.IsValidationException(e.Exception)))
                .WriteTo.Async(a => a.File(
                    path: Path.Combine(errorsFolder, "validation-errors-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: loggingOptions.RetainedFileCountLimit,
                    fileSizeLimitBytes: loggingOptions.FileSizeLimitBytes,
                    rollOnFileSizeLimit: true,
                    outputTemplate: loggingOptions.OutputTemplate,
                    shared: true)))
            // 7. Dedicated Unhandled / Runtime Errors Log
            .WriteTo.Logger(sub => sub
                .Filter.ByIncludingOnly(e => MatchesErrorType(e, ErrorType.Unhandled) ||
                                             (e.Level >= LogEventLevel.Error && e.Exception != null &&
                                              !ErrorClassifier.IsDatabaseException(e.Exception) &&
                                              !ErrorClassifier.IsSecurityException(e.Exception) &&
                                              !ErrorClassifier.IsValidationException(e.Exception) &&
                                              !ErrorClassifier.IsNotFoundException(e.Exception)))
                .WriteTo.Async(a => a.File(
                    path: Path.Combine(errorsFolder, "unhandled-errors-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: loggingOptions.RetainedFileCountLimit,
                    fileSizeLimitBytes: loggingOptions.FileSizeLimitBytes,
                    rollOnFileSizeLimit: true,
                    outputTemplate: loggingOptions.OutputTemplate,
                    shared: true)));

        Log.Logger = loggerConfig.CreateLogger();
        builder.Host.UseSerilog();

        return builder;
    }

    private static bool MatchesErrorType(LogEvent logEvent, ErrorType errorType)
    {
        if (logEvent.Properties.TryGetValue("ErrorType", out var propertyValue))
        {
            var strVal = propertyValue.ToString();
            return strVal.Contains(errorType.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}
