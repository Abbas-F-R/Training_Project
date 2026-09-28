using Training_Project.Shared.Attributes;
using Training_Project.Shared.Base.dto;

namespace Training_Project.Features.Students.Dtos;

/// <summary>
/// Query filter and pagination parameters for student directory searches.
/// </summary>
public class StudentFilter : BaseFilter
{
    public string? FullName { get; set; }
    public string? StudentCode { get; set; }

    [Sqid]
    public long? DepartmentId { get; set; }

    public int? Stage { get; set; }
}
