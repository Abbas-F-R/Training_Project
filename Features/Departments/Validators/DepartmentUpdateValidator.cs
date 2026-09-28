using FluentValidation;
using OC_System_Training.Features.Departments.Dtos;

namespace OC_System_Training.Features.Departments.Validators;

/// <summary>
/// Validator for department update requests.
/// </summary>
public class DepartmentUpdateValidator : AbstractValidator<DepartmentUpdate>
{
    public DepartmentUpdateValidator()
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
