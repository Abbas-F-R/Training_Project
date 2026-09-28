using FluentValidation;
using OC_System_Training.Features.AuditLogs.Dtos;

namespace OC_System_Training.Features.AuditLogs.Validators;

/// <summary>
/// Validator for audit log pagination and search filter parameters.
/// </summary>
public class AuditLogFilterValidator : AbstractValidator<AuditLogFilter>
{
    public AuditLogFilterValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.EntityName)
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.EntityName))
            .WithMessage("Entity name filter cannot exceed 100 characters.");

        RuleFor(x => x.Action)
            .MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Action))
            .WithMessage("Action filter cannot exceed 50 characters.");

        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate!.Value)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
            .WithMessage("ToDate must be greater than or equal to FromDate.");
    }
}
