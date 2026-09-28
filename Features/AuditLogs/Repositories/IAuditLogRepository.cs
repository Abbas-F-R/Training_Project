using Training_Project.Features.AuditLogs.Dtos;

namespace Training_Project.Features.AuditLogs.Repositories;

/// <summary>
/// Data-access contract for appending audit records and querying historical audit trails.
/// </summary>
public interface IAuditLogRepository
{
    Task LogAsync(
        long? userId,
        string action,
        string entityName,
        string? entityId = null,
        string? changes = null,
        string? ipAddress = null,
        string? userAgent = null,
        bool isSuccess = true);

    Task<(List<AuditLogResponse>? data, int totalCount)> GetAll(AuditLogFilter filter);
}
