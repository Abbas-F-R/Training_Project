using OC_System_Training.Features.AuditLogs.Dtos;
using OC_System_Training.Features.AuditLogs.Repositories;
using OC_System_Training.Shared.Attributes;
using OC_System_Training.Shared.Base.dto;

namespace OC_System_Training.Features.AuditLogs.Services;

// تعليق تدريبي: تطبيق خدمة سجل التدقيق للمسؤولين
[Scoped]
public class AuditLogService(IAuditLogRepository repository) : IAuditLogService
{
    public async Task<ServiceResult<List<AuditLogResponse>>> GetAll(ServiceRequest<AuditLogFilter> request)
    {
        var (data, totalCount) = await repository.GetAll(request.Dto);
        return ServiceResult<List<AuditLogResponse>>.PagedOk(data, totalCount);
    }
}
