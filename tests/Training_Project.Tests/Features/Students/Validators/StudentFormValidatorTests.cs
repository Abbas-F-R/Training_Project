using FluentAssertions;
using Training_Project.Features.Students.Dtos;
using Training_Project.Features.Students.Validators;
using Xunit;

namespace Training_Project.Tests.Features.Students.Validators;

public class StudentFormValidatorTests
{
    private readonly StudentFormValidator _validator = new();

    [Fact]
    public void ValidForm_ShouldPassValidation()
    {
        var form = new StudentForm
        {
            FullName = "Ahmed Ali",
            StudentCode = "STU-2026-001",
            Email = "ahmed@example.com",
            PhoneNumber = "07701234567",
            DepartmentId = 1,
            Stage = 2,
            BirthDate = new DateTime(2003, 5, 20)
        };

        var result = _validator.Validate(form);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "STU-001", 1, 1, "FullName")]
    [InlineData("Ahmed Ali", "", 1, 1, "StudentCode")]
    [InlineData("Ahmed Ali", "STU-001", 0, 1, "DepartmentId")]
    [InlineData("Ahmed Ali", "STU-001", 1, 0, "Stage")] // stage < 1
    [InlineData("Ahmed Ali", "STU-001", 1, 7, "Stage")] // stage > 6
    public void InvalidForm_ShouldFailValidation(
        string fullName, string studentCode, long departmentId, int stage, string expectedErrorProperty)
    {
        var form = new StudentForm
        {
            FullName = fullName,
            StudentCode = studentCode,
            DepartmentId = departmentId,
            Stage = stage
        };

        var result = _validator.Validate(form);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == expectedErrorProperty);
    }

    [Fact]
    public void FutureBirthDate_ShouldFailValidation()
    {
        var form = new StudentForm
        {
            FullName = "Ahmed Ali",
            StudentCode = "STU-001",
            DepartmentId = 1,
            Stage = 1,
            BirthDate = DateTime.Today.AddDays(1)
        };

        var result = _validator.Validate(form);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BirthDate");
    }

    [Fact]
    public void InvalidPhonePattern_ShouldFailValidation()
    {
        var form = new StudentForm
        {
            FullName = "Ahmed Ali",
            StudentCode = "STU-001",
            DepartmentId = 1,
            Stage = 1,
            PhoneNumber = "08812345678" // Not starting with 07
        };

        var result = _validator.Validate(form);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PhoneNumber");
    }
}
