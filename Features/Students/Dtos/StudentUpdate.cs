using OC_System_Training.Shared.Attributes;

namespace OC_System_Training.Features.Students.Dtos;

/// <summary>
/// Data transfer object for updating an existing student profile.
/// </summary>
public class StudentUpdate
{
    public string FullName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    [Sqid]
    public long DepartmentId { get; set; }

    public int Stage { get; set; } = 1;
    public DateTime? BirthDate { get; set; }
}
