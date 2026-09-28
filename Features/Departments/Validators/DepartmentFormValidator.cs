using FluentValidation;
using OC_System_Training.Features.Departments.Dtos;

namespace OC_System_Training.Features.Departments.Validators;

// تعليق تدريبي: مدقق نموذج إنشاء قسم دراسي جديد
public class DepartmentFormValidator : AbstractValidator<DepartmentForm>
{
    public DepartmentFormValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم القسم مطلوب ولا يمكن تركه فارغاً.")
            .MaximumLength(100).WithMessage("اسم القسم لا يتجاوز 100 حرف.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("رمز القسم مطلوب.")
            .MinimumLength(2).WithMessage("رمز القسم يجب ألا يقل عن حرفين.")
            .MaximumLength(20).WithMessage("رمز القسم لا يتجاوز 20 حرفاً.")
            .Matches(@"^[A-Z0-9_-]+$").WithMessage("رمز القسم يجب أن يتكون من أحرف إنجليزية كبيرة وأرقام فقط (مثل CS, SE).");
    }
}
