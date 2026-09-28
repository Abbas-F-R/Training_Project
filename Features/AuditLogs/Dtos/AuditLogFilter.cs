using Training_Project.Shared.Base.dto;

namespace Training_Project.Features.AuditLogs.Dtos;

/// <summary>
/// Filter and pagination criteria for audit log search queries.
/// </summary>
public class AuditLogFilter : BaseFilter
{
    public string? EntityName { get; set; }
    public string? Action { get; set; }
    public long? UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
