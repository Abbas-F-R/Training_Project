using System.ComponentModel.DataAnnotations;

namespace OC_System_Training.Features.Auth.Dtos;

// تعليق تدريبي: DTO تسجيل مستخدم جديد
public class RegisterRequest
{
    [Required(ErrorMessage = "الاسم الكامل مطلوب.")]
    [StringLength(150, ErrorMessage = "الاسم الكامل لا يتجاوز 150 حرفاً.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "اسم المستخدم مطلوب.")]
    [StringLength(100, ErrorMessage = "اسم المستخدم لا يتجاوز 100 حرف.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة.")]
    [MinLength(6, ErrorMessage = "كلمة المرور يجب ألا تقل عن 6 أحرف.")]
    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "Admin";
}
