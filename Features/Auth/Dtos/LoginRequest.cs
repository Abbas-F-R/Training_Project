using System.ComponentModel.DataAnnotations;

namespace OC_System_Training.Features.Auth.Dtos;

// تعليق تدريبي: DTO طلب تسجيل الدخول (Login)
public class LoginRequest
{
    [Required(ErrorMessage = "اسم المستخدم مطلوب.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة.")]
    public string Password { get; set; } = string.Empty;
}
