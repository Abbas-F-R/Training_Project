using FluentValidation;
using OC_System_Training.Features.Students.Dtos;

namespace OC_System_Training.Features.Students.Validators;

// تعليق تدريبي: مدقق معلمات فلترة وترقيم الطلاب
public class StudentFilterValidator : AbstractValidator<StudentFilter>
{
    public StudentFilterValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("رقم الصفحة يجب أن يكون 1 على الأقل.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("حجم الصفحة يجب أن يكون بين 1 و 100.");

        RuleFor(x => x.Stage)
            .InclusiveBetween(1, 6).When(x => x.Stage.HasValue)
            .WithMessage("المرحلة الدراسية المحددة للفلترة يجب أن تكون بين 1 و 6.");
    }
}
