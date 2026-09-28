using OC_System_Training.Shared.Attributes;
using OC_System_Training.Shared.Base.dto;

namespace OC_System_Training.Features.Students.Dtos;

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
