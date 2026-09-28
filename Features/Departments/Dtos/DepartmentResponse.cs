using Training_Project.Shared.Attributes;

namespace Training_Project.Features.Departments.Dtos;

/// <summary>
/// Data transfer object representing department details with obfuscated Sqid identifier.
/// </summary>
public class DepartmentResponse
{
    [Sqid]
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
