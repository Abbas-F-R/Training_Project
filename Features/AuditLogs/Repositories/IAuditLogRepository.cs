using OC_System_Training.Features.AuditLogs.Dtos;

namespace OC_System_Training.Features.AuditLogs.Repositories;

// تعليق تدريبي: واجهة مستودع سجل التدقيق والتتبع
// تتيح تسجيل الأحداث الحساسة (مثل الدخول الفاشل أو الناجح) واستعراض السجلات للمسؤول
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
