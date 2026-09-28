using Training_Project.Shared.Attributes;

namespace Training_Project.Features.AuditLogs.Dtos;

/// <summary>
/// Audit trail response representation for administrative audit queries.
/// </summary>
public class AuditLogResponse
{
    [Sqid]
    public long Id { get; set; }

    [Sqid]
    public long? UserId { get; set; }

    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Changes { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime CreatedAt { get; set; }
}
