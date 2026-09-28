using FluentAssertions;
using Training_Project.Features.Students.Dtos;
using Training_Project.Features.Students.Validators;
using Xunit;

namespace Training_Project.Tests.Features.Students.Validators;

public class StudentFilterValidatorTests
{
    private readonly StudentFilterValidator _validator = new();

    [Fact]
    public void ValidFilter_ShouldPassValidation()
    {
        var filter = new StudentFilter
        {
            PageNumber = 1,
            PageSize = 20,
            FullName = "Ali",
            Stage = 3
        };

        var result = _validator.Validate(filter);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void InvalidPagination_ShouldFailValidation(int pageNumber, int pageSize)
    {
        var filter = new StudentFilter { PageNumber = pageNumber, PageSize = pageSize };

        var result = _validator.Validate(filter);

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void InvalidStage_ShouldFailValidation(int stage)
    {
        var filter = new StudentFilter { PageNumber = 1, PageSize = 10, Stage = stage };

        var result = _validator.Validate(filter);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Stage");
    }
}
