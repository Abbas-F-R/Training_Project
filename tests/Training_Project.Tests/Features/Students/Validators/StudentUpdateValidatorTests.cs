using FluentAssertions;
using Training_Project.Features.Students.Dtos;
using Training_Project.Features.Students.Validators;
using Xunit;

namespace Training_Project.Tests.Features.Students.Validators;

public class StudentUpdateValidatorTests
{
    private readonly StudentUpdateValidator _validator = new();

    [Fact]
    public void ValidUpdate_ShouldPassValidation()
    {
        var update = new StudentUpdate
        {
            FullName = "Ahmed Ali Hassan",
            StudentCode = "STU-2026-001",
            Email = "ahmed.ali@example.com",
            PhoneNumber = "07701234567",
            DepartmentId = 1,
            Stage = 3,
            BirthDate = new DateTime(2003, 5, 20)
        };

        var result = _validator.Validate(update);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "STU-001", 1, 1, "FullName")]
    [InlineData("Ahmed Ali", "", 1, 1, "StudentCode")]
    [InlineData("Ahmed Ali", "STU-001", 0, 1, "DepartmentId")]
    [InlineData("Ahmed Ali", "STU-001", 1, 0, "Stage")]
    [InlineData("Ahmed Ali", "STU-001", 1, 7, "Stage")]
    public void InvalidUpdate_ShouldFailValidation(
        string fullName, string studentCode, long departmentId, int stage, string expectedErrorProperty)
    {
        var update = new StudentUpdate
        {
            FullName = fullName,
            StudentCode = studentCode,
            DepartmentId = departmentId,
            Stage = stage
        };

        var result = _validator.Validate(update);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == expectedErrorProperty);
    }
}
