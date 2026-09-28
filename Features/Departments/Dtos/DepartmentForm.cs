using System.ComponentModel.DataAnnotations;

namespace OC_System_Training.Features.Departments.Dtos;

/// <summary>
/// Data transfer object for creating a new academic department.
/// </summary>
public class DepartmentForm
{
    [Required(ErrorMessage = "Department name is required.")]
    [StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department code is required.")]
    [StringLength(20, ErrorMessage = "Department code cannot exceed 20 characters.")]
    public string Code { get; set; } = string.Empty;
}
