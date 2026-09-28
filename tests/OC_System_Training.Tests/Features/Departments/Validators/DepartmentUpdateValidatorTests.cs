using FluentAssertions;
using OC_System_Training.Features.Departments.Dtos;
using OC_System_Training.Features.Departments.Validators;
using Xunit;

namespace OC_System_Training.Tests.Features.Departments.Validators;

public class DepartmentUpdateValidatorTests
{
    private readonly DepartmentUpdateValidator _validator = new();

    [Fact]
    public void ValidUpdate_ShouldPassValidation()
    {
        var update = new DepartmentUpdate
        {
            Name = "Software Engineering",
            Code = "SE"
        };

        var result = _validator.Validate(update);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "SE", "Name")]
    [InlineData("Software Engineering", "", "Code")]
    [InlineData("Software Engineering", "S", "Code")]
    public void InvalidUpdate_ShouldFailValidation(string name, string code, string expectedErrorProperty)
    {
        var update = new DepartmentUpdate { Name = name, Code = code };

        var result = _validator.Validate(update);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == expectedErrorProperty);
    }
}
