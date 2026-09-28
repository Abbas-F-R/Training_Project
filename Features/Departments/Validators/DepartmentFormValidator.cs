using FluentValidation;
using Training_Project.Features.Departments.Dtos;

namespace Training_Project.Features.Departments.Validators;

/// <summary>
/// Validator for department creation requests.
/// </summary>
public class DepartmentFormValidator : AbstractValidator<DepartmentForm>
{
    public DepartmentFormValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Department name is required.")
            .MaximumLength(100).WithMessage("Department name cannot exceed 100 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Department code is required.")
            .MinimumLength(2).WithMessage("Department code must be at least 2 characters.")
            .MaximumLength(20).WithMessage("Department code cannot exceed 20 characters.")
            .Matches(@"^[A-Z0-9_-]+$").WithMessage("Department code must consist of uppercase alphanumeric characters, dashes, or underscores.");
    }
}
