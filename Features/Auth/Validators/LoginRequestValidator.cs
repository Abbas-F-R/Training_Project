using FluentValidation;
using OC_System_Training.Features.Auth.Dtos;

namespace OC_System_Training.Features.Auth.Validators;

// تعليق تدريبي: مدقق طلب تسجيل الدخول باستخدام FluentValidation
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("اسم المستخدم مطلوب ولا يمكن تركه فارغاً.")
            .MaximumLength(100).WithMessage("اسم المستخدم يجب ألا يتجاوز 100 حرف.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة ولا يمكن تركها فارغة.")
            .MinimumLength(6).WithMessage("كلمة المرور يجب ألا تقل عن 6 أحرف.");
    }
}
