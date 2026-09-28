namespace OC_System_Training.Features.Departments.Dtos;

// تعليق تدريبي: DTO الخاص بفلاتر وترقيم الأقسام الدراسية (GET)
// يرث من BaseFilter ليرث PageNumber وPageSize تلقائياً
public class DepartmentFilter : BaseFilter
{
    public string? Name { get; set; }
    public string? Code { get; set; }
}
