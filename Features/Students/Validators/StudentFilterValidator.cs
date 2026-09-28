using FluentValidation;
using OC_System_Training.Features.Students.Dtos;

namespace OC_System_Training.Features.Students.Validators;

/// <summary>
/// Validator for student pagination and filter criteria.
/// </summary>
public class StudentFilterValidator : AbstractValidator<StudentFilter>
{
    public StudentFilterValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.Stage)
            .InclusiveBetween(1, 6).When(x => x.Stage.HasValue)
            .WithMessage("Stage filter must be between 1 and 6.");
    }
}
