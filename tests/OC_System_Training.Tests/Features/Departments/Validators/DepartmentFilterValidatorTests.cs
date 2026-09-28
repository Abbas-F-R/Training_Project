using FluentAssertions;
using OC_System_Training.Features.Departments.Dtos;
using OC_System_Training.Features.Departments.Validators;
using Xunit;

namespace OC_System_Training.Tests.Features.Departments.Validators;

public class DepartmentFilterValidatorTests
{
    private readonly DepartmentFilterValidator _validator = new();

    [Fact]
    public void ValidFilter_ShouldPassValidation()
    {
        var filter = new DepartmentFilter
        {
            PageNumber = 1,
            PageSize = 25,
            Name = "Computer",
            Code = "CS"
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
        var filter = new DepartmentFilter { PageNumber = pageNumber, PageSize = pageSize };

        var result = _validator.Validate(filter);

        result.IsValid.Should().BeFalse();
    }
}
