using FluentValidation;
using OC_System_Training.Features.Auth.Dtos;

namespace OC_System_Training.Features.Auth.Validators;

/// <summary>
/// Validator for user registration requests.
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
            .MaximumLength(100).WithMessage("Username cannot exceed 100 characters.")
            .Matches(@"^[a-zA-Z0-9_\.]+$").WithMessage("Username can only contain alphanumeric characters, underscores, and periods.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => role is "Admin" or "User" or "Teacher" or "Student" or "Staff")
            .WithMessage("Invalid role specified.");
    }
}
