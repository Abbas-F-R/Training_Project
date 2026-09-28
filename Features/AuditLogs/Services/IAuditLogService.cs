using OC_System_Training.Features.AuditLogs.Dtos;
using OC_System_Training.Shared.Base.dto;

namespace OC_System_Training.Features.AuditLogs.Services;

// تعليق تدريبي: واجهة خدمة التدقيق للاستعلامات الإدارية
public interface IAuditLogService
{
    Task<ServiceResult<List<AuditLogResponse>>> GetAll(ServiceRequest<AuditLogFilter> request);
}
