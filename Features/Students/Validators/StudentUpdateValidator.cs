using FluentValidation;
using OC_System_Training.Features.Students.Dtos;

namespace OC_System_Training.Features.Students.Validators;

// تعليق تدريبي: مدقق نموذج تعديل بيانات طالب باستخدام FluentValidation
public class StudentUpdateValidator : AbstractValidator<StudentUpdate>
{
    public StudentUpdateValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("اسم الطالب الكامل مطلوب.")
            .MaximumLength(150).WithMessage("اسم الطالب لا يتجاوز 150 حرفاً.");

        RuleFor(x => x.StudentCode)
            .NotEmpty().WithMessage("الرقم الجامعي مطلوب.")
            .MaximumLength(50).WithMessage("الرقم الجامعي لا يتجاوز 50 حرفاً.")
            .Matches(@"^[A-Za-z0-9_-]+$").WithMessage("الرقم الجامعي يجب أن يحتوي على أحرف وأرقام ورموز صحيحة فقط.");

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("يجب تحديد قسم دراسي صالح للطالب.");

        RuleFor(x => x.Stage)
            .InclusiveBetween(1, 6).WithMessage("المرحلة الدراسية يجب أن تكون بين 1 و 6.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("البريد الإلكتروني المدخل غير صالح.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^07[0-9]{9}$").When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("رقم الهاتف يجب أن يبدأ بـ 07 ويتكون من 11 رقماً.");

        RuleFor(x => x.BirthDate)
            .LessThanOrEqualTo(DateTime.Today).When(x => x.BirthDate.HasValue)
            .WithMessage("تاريخ ميلاد الطالب يجب أن يكون في الماضي.");
    }
}
