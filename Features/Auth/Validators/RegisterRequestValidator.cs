using FluentValidation;
using OC_System_Training.Features.Auth.Dtos;

namespace OC_System_Training.Features.Auth.Validators;

// تعليق تدريبي: مدقق طلب إنشاء مستخدم جديد باستخدام FluentValidation
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("الاسم الكامل مطلوب.")
            .MaximumLength(150).WithMessage("الاسم الكامل يجب ألا يتجاوز 150 حرفاً.");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("اسم المستخدم مطلوب.")
            .MinimumLength(3).WithMessage("اسم المستخدم يجب ألا يقل عن 3 أحرف.")
            .MaximumLength(100).WithMessage("اسم المستخدم يجب ألا يتجاوز 100 حرف.")
            .Matches(@"^[a-zA-Z0-9_\.]+$").WithMessage("اسم المستخدم يجب أن يحتوي على أحرف إنجليزية وأرقام ونقاط فقط.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة.")
            .MinimumLength(6).WithMessage("كلمة المرور يجب ألا تقل عن 6 أحرف.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("الدور الأمني مطلوب.")
            .Must(role => role is "Admin" or "Teacher" or "Student" or "Staff")
            .WithMessage("الدور المحدد غير صالح في النظام.");
    }
}
