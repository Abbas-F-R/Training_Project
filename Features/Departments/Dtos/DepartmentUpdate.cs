namespace Training_Project.Features.Departments.Dtos;

/// <summary>
/// Data transfer object for updating an existing academic department.
/// </summary>
public class DepartmentUpdate
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
