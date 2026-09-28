using System.ComponentModel.DataAnnotations;
using OC_System_Training.Shared.Attributes;

namespace OC_System_Training.Features.Students.Dtos;

// تعليق تدريبي: DTO الخاص بإنشاء طالب جديد (POST)
public class StudentForm
{
    [Required(ErrorMessage = "اسم الطالب الكامل مطلوب.")]
    [StringLength(150, ErrorMessage = "اسم الطالب لا يتجاوز 150 حرفاً.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "الرقم الجامعي مطلوب.")]
    [StringLength(50, ErrorMessage = "الرقم الجامعي لا يتجاوز 50 حرفاً.")]
    public string StudentCode { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح.")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "رقم الهاتف غير صالح.")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "يجب تحديد القسم الدراسي.")]
    [Sqid]
    public long DepartmentId { get; set; }

    [Range(1, 6, ErrorMessage = "المرحلة الدراسية يجب أن تكون بين 1 و 6.")]
    public int Stage { get; set; } = 1;

    public DateTime? BirthDate { get; set; }
}
