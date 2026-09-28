using OC_System_Training.Shared.Attributes;

namespace OC_System_Training.Features.Students.Dtos;

// تعليق تدريبي: DTO الخاص بفلاتر وترقيم الطلاب (GET)
// يشمل البحث بالنص، التصفية حسب القسم أو المرحلة الدراسية، إضافة لبيانات الترقيم من BaseFilter
public class StudentFilter : BaseFilter
{
    public string? FullName { get; set; }
    public string? StudentCode { get; set; }

    [Sqid]
    public long? DepartmentId { get; set; }

    public int? Stage { get; set; }
}
