using OC_System_Training.Shared.Attributes;

namespace OC_System_Training.Features.Students.Dtos;

// تعليق تدريبي: DTO الخاص بمخرجات بيانات الطالب (Response)
// يحتوي على بيانات الطالب مع اسم ورمز القسم المجلوبة عبر View (vw_Students)
// تستخدم [Sqid] على المعرفات الرقمية لحمايتها
public class StudentResponse
{
    [Sqid]
    public long Id { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    [Sqid]
    public long DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = string.Empty;

    public int Stage { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
