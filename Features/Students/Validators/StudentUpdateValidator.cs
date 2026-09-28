using FluentValidation;
using Training_Project.Features.Students.Dtos;

namespace Training_Project.Features.Students.Validators;

/// <summary>
/// Validator for student update requests.
/// </summary>
public class StudentUpdateValidator : AbstractValidator<StudentUpdate>
{
    public StudentUpdateValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Student full name is required.")
            .MaximumLength(150).WithMessage("Student full name cannot exceed 150 characters.");

        RuleFor(x => x.StudentCode)
            .NotEmpty().WithMessage("Student code is required.")
            .MaximumLength(50).WithMessage("Student code cannot exceed 50 characters.")
            .Matches(@"^[A-Za-z0-9_-]+$").WithMessage("Student code can only contain alphanumeric characters, underscores, and dashes.");

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("A valid department must be selected.");

        RuleFor(x => x.Stage)
            .InclusiveBetween(1, 6).WithMessage("Academic stage must be between 1 and 6.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("A valid email address is required.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^07[0-9]{9}$").When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Phone number must start with 07 and consist of 11 digits.");

        RuleFor(x => x.BirthDate)
            .LessThanOrEqualTo(DateTime.Today).When(x => x.BirthDate.HasValue)
            .WithMessage("Date of birth must be in the past.");
    }
}
