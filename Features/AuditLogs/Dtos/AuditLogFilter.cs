using OC_System_Training.Shared.Base.dto;

namespace OC_System_Training.Features.AuditLogs.Dtos;

// تعليق تدريبي: DTO الخاص بتصفية وترقيم سجلات التدقيق والتتبع
public class AuditLogFilter : BaseFilter
{
    public string? EntityName { get; set; }
    public string? Action { get; set; }
    public long? UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
