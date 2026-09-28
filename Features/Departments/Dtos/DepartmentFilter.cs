using Training_Project.Shared.Base.dto;

namespace Training_Project.Features.Departments.Dtos;

/// <summary>
/// Query filter and pagination parameters for department listings.
/// </summary>
public class DepartmentFilter : BaseFilter
{
    public string? Name { get; set; }
    public string? Code { get; set; }
}
