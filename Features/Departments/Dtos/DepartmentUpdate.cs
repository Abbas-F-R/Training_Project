using System.ComponentModel.DataAnnotations;

namespace OC_System_Training.Features.Departments.Dtos;

// تعليق تدريبي: DTO الخاص بتعديل قسم دراسي موجود (PUT)
public class DepartmentUpdate
{
    [Required(ErrorMessage = "اسم القسم مطلوب.")]
    [StringLength(100, ErrorMessage = "اسم القسم لا يتجاوز 100 حرف.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز القسم مطلوب.")]
    [StringLength(20, ErrorMessage = "رمز القسم لا يتجاوز 20 حرفاً.")]
    public string Code { get; set; } = string.Empty;
}
