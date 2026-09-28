using OC_System_Training.Features.AuditLogs.Dtos;

namespace OC_System_Training.Features.AuditLogs.Repositories;

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
