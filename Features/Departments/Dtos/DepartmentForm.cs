namespace Training_Project.Features.Departments.Dtos;

/// <summary>
/// Data transfer object for creating a new academic department.
/// </summary>
public class DepartmentForm
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
