using FluentAssertions;
using Training_Project.Features.Departments.Dtos;
using Training_Project.Features.Departments.Validators;
using Xunit;

namespace Training_Project.Tests.Features.Departments.Validators;

public class DepartmentFormValidatorTests
{
    private readonly DepartmentFormValidator _validator = new();

    [Fact]
    public void ValidForm_ShouldPassValidation()
    {
        var form = new DepartmentForm
        {
            Name = "Computer Science",
            Code = "CS"
        };

        var result = _validator.Validate(form);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "CS", "Name")]
    [InlineData("Computer Science", "", "Code")]
    [InlineData("Computer Science", "C", "Code")] // < 2 chars
    [InlineData("Computer Science", "cs lowercase", "Code")] // spaces/lowercase regex fail
    public void InvalidForm_ShouldFailValidation(string name, string code, string expectedErrorProperty)
    {
        var form = new DepartmentForm { Name = name, Code = code };

        var result = _validator.Validate(form);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == expectedErrorProperty);
    }
}
