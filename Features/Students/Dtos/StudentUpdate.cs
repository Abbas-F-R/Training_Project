using System.ComponentModel.DataAnnotations;
using OC_System_Training.Shared.Attributes;

namespace OC_System_Training.Features.Students.Dtos;

/// <summary>
/// Data transfer object for updating an existing student profile.
/// </summary>
public class StudentUpdate
{
    [Required(ErrorMessage = "Student full name is required.")]
    [StringLength(150, ErrorMessage = "Student full name cannot exceed 150 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Student code is required.")]
    [StringLength(50, ErrorMessage = "Student code cannot exceed 50 characters.")]
    public string StudentCode { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Invalid phone number.")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Department is required.")]
    [Sqid]
    public long DepartmentId { get; set; }

    [Range(1, 6, ErrorMessage = "Academic stage must be between 1 and 6.")]
    public int Stage { get; set; } = 1;

    public DateTime? BirthDate { get; set; }
}
