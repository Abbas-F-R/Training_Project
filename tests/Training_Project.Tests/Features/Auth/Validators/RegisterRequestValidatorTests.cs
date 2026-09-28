using FluentAssertions;
using Training_Project.Features.Auth.Dtos;
using Training_Project.Features.Auth.Validators;
using Xunit;

namespace Training_Project.Tests.Features.Auth.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void ValidRegisterRequest_ShouldPassValidation()
    {
        var request = new RegisterRequest
        {
            FullName = "John Doe",
            UserName = "johndoe",
            Password = "Password123!",
            Role = "User"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "johndoe", "Password123!", "User", "FullName")]
    [InlineData("John Doe", "ab", "Password123!", "User", "UserName")] // < 3 chars
    [InlineData("John Doe", "invalid user!", "Password123!", "User", "UserName")] // spaces/exclamation
    [InlineData("John Doe", "johndoe", "short", "User", "Password")] // < 6 chars
    [InlineData("John Doe", "johndoe", "Password123!", "SuperAdmin", "Role")] // invalid role
    public void InvalidRegisterRequest_ShouldFailValidation(
        string fullName, string userName, string password, string role, string expectedErrorProperty)
    {
        var request = new RegisterRequest
        {
            FullName = fullName,
            UserName = userName,
            Password = password,
            Role = role
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == expectedErrorProperty);
    }
}
