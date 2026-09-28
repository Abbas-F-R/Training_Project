using Training_Project.Features.AuditLogs.Dtos;
using Training_Project.Features.AuditLogs.Repositories;
using Training_Project.Shared.Attributes;
using Training_Project.Shared.Base.dto;

namespace Training_Project.Features.AuditLogs.Services;

/// <summary>
/// Service implementation for administrative audit log queries.
/// </summary>
[Scoped]
public class AuditLogService(IAuditLogRepository repository) : IAuditLogService
{
    public async Task<ServiceResult<List<AuditLogResponse>>> GetAll(ServiceRequest<AuditLogFilter> request)
    {
        var (data, totalCount) = await repository.GetAll(request.Dto);
        return ServiceResult<List<AuditLogResponse>>.PagedOk(data, totalCount);
    }
}
