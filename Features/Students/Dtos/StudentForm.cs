using Training_Project.Shared.Attributes;

namespace Training_Project.Features.Students.Dtos;

/// <summary>
/// Data transfer object for enrolling a new student.
/// </summary>
public class StudentForm
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
