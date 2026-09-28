using Training_Project.Features.AuditLogs.Dtos;
using Training_Project.Shared.Base.dto;

namespace Training_Project.Features.AuditLogs.Services;

/// <summary>
/// Service contract for querying audit log records.
/// </summary>
public interface IAuditLogService
{
    Task<ServiceResult<List<AuditLogResponse>>> GetAll(ServiceRequest<AuditLogFilter> request);
}
