using OC_System_Training.Shared.Base.dto;

namespace OC_System_Training.Features.Departments.Dtos;

/// <summary>
/// Query filter and pagination parameters for department listings.
/// </summary>
public class DepartmentFilter : BaseFilter
{
    public string? Name { get; set; }
    public string? Code { get; set; }
}
