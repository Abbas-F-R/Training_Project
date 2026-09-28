using FluentValidation;
using OC_System_Training.Features.Departments.Dtos;

namespace OC_System_Training.Features.Departments.Validators;

/// <summary>
/// Validator for department pagination and search filter parameters.
/// </summary>
public class DepartmentFilterValidator : AbstractValidator<DepartmentFilter>
{
    public DepartmentFilterValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.Name)
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Name))
            .WithMessage("Department name filter cannot exceed 100 characters.");

        RuleFor(x => x.Code)
            .MaximumLength(20).When(x => !string.IsNullOrWhiteSpace(x.Code))
            .WithMessage("Department code filter cannot exceed 20 characters.");
    }
}
