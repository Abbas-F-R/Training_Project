using FluentAssertions;
using OC_System_Training.Features.AuditLogs.Dtos;
using OC_System_Training.Features.AuditLogs.Validators;
using Xunit;

namespace OC_System_Training.Tests.Features.AuditLogs.Validators;

public class AuditLogFilterValidatorTests
{
    private readonly AuditLogFilterValidator _validator = new();

    [Fact]
    public void ValidFilter_ShouldPassValidation()
    {
        var filter = new AuditLogFilter
        {
            PageNumber = 1,
            PageSize = 20,
            EntityName = "Students",
            Action = "INSERT",
            FromDate = DateTime.Today.AddDays(-7),
            ToDate = DateTime.Today
        };

        var result = _validator.Validate(filter);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void InvalidPagination_ShouldFailValidation(int pageNumber, int pageSize)
    {
        var filter = new AuditLogFilter
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = _validator.Validate(filter);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ToDateBeforeFromDate_ShouldFailValidation()
    {
        var filter = new AuditLogFilter
        {
            PageNumber = 1,
            PageSize = 10,
            FromDate = DateTime.Today,
            ToDate = DateTime.Today.AddDays(-1)
        };

        var result = _validator.Validate(filter);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ToDate");
    }
}
