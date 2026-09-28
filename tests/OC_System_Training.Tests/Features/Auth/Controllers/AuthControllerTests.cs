using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OC_System_Training.Features.Auth.Controllers;
using OC_System_Training.Features.Auth.Dtos;
using OC_System_Training.Features.Auth.Services;
using OC_System_Training.Shared.Base;
using OC_System_Training.Shared.Base.dto;
using Xunit;

namespace OC_System_Training.Tests.Features.Auth.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(_authServiceMock.Object);

        var userMock = new Mock<ICurrentUser>();
        userMock.Setup(u => u.UserId).Returns(1);
        userMock.Setup(u => u.UserName).Returns("admin");
        userMock.Setup(u => u.Role).Returns("Admin");
        userMock.Setup(u => u.Lang).Returns("en");
        _controller.SetCurrentUser(userMock.Object);
    }

    [Fact]
    public void Login_ShouldHaveAllowAnonymousAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.Login));
        var allowAnon = method?.GetCustomAttributes(typeof(AllowAnonymousAttribute), false).FirstOrDefault();

        allowAnon.Should().NotBeNull();
    }

    [Fact]
    public void Register_ShouldRequireAdminRole()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.Register));
        var authAttr = method?.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        authAttr.Should().NotBeNull();
        authAttr!.Roles.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkWithToken()
    {
        var request = new LoginRequest { UserName = "admin", Password = "Password123!" };
        var loginResponse = new LoginResponse
        {
            UserId = 1,
            UserName = "admin",
            FullName = "Administrator",
            Role = "Admin",
            Token = "valid.jwt.token",
            ExpiresAt = DateTime.UtcNow.AddHours(2)
        };

        _authServiceMock.Setup(s => s.Login(request))
            .ReturnsAsync(ServiceResult<LoginResponse>.Ok(loginResponse));

        var result = await _controller.Login(request);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        okResult.Value.Should().BeEquivalentTo(loginResponse);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnOkWithCreatedUser()
    {
        var request = new RegisterRequest
        {
            FullName = "Jane Doe",
            UserName = "janedoe",
            Password = "Password123!",
            Role = "User"
        };
        var loginResponse = new LoginResponse
        {
            UserId = 2,
            UserName = "janedoe",
            FullName = "Jane Doe",
            Role = "User",
            Token = "registered.jwt.token",
            ExpiresAt = DateTime.UtcNow.AddHours(2)
        };

        _authServiceMock.Setup(s => s.Register(request, It.IsAny<long>()))
            .ReturnsAsync(ServiceResult<LoginResponse>.Ok(loginResponse));

        var result = await _controller.Register(request);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        okResult.Value.Should().BeEquivalentTo(loginResponse);
    }
}
