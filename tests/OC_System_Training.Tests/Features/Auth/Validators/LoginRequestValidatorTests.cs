using FluentAssertions;
using OC_System_Training.Features.Auth.Dtos;
using OC_System_Training.Features.Auth.Validators;
using Xunit;

namespace OC_System_Training.Tests.Features.Auth.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void ValidLoginRequest_ShouldPassValidation()
    {
        var request = new LoginRequest
        {
            UserName = "admin",
            Password = "Password123!"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Password123!", "UserName")]
    [InlineData("admin", "", "Password")]
    [InlineData("admin", "12345", "Password")] // less than 6 chars
    public void InvalidLoginRequest_ShouldFailValidation(string userName, string password, string expectedErrorProperty)
    {
        var request = new LoginRequest
        {
            UserName = userName,
            Password = password
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == expectedErrorProperty);
    }
}
