using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using OC_System_Training.Features.AuditLogs.Repositories;
using OC_System_Training.Features.Auth.Dtos;
using OC_System_Training.Features.Auth.Repositories;
using OC_System_Training.Features.Auth.Services;
using OC_System_Training.Shared.Constants;
using OC_System_Training.Shared.Utils;
using Xunit;

namespace OC_System_Training.Tests;

public class AuthenticationTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IAuditLogRepository> _auditLogRepoMock = new();
    private readonly IConfiguration _configuration;

    public AuthenticationTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:SecretKey", "SuperSecretKeyForStudentManagementSystem2026SecureMin32Bytes!" }
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact(DisplayName = "Login with valid credentials returns JWT token and records LOGIN_SUCCESS audit log")]
    public async Task Login_WithValidCredentials_ReturnsToken_And_LogsSuccess()
    {
        // Arrange
        var password = "SecurePassword123!";
        var passwordHash = PasswordHasher.Hash(password);
        var user = new UserDto
        {
            Id = 1,
            FullName = "Ahmed Ali",
            UserName = "ahmed",
            PasswordHash = passwordHash,
            Role = "Admin",
            IsActive = true
        };

        _userRepoMock.Setup(r => r.GetByUserName("ahmed"))
            .ReturnsAsync(user);

        var service = new AuthService(_userRepoMock.Object, _auditLogRepoMock.Object, _configuration);
        var request = new LoginRequest { UserName = "ahmed", Password = password };

        // Act
        var result = await service.Login(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().NotBeNullOrWhiteSpace();
        result.Data.Role.Should().Be("Admin");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Data.Token);
        jwt.Claims.First(c => c.Type == "UserName").Value.Should().Be("ahmed");
        jwt.Claims.First(c => c.Type == "Role").Value.Should().Be("Admin");

        _auditLogRepoMock.Verify(a => a.LogAsync(
            1,
            "LOGIN_SUCCESS",
            "Auth",
            "ahmed",
            It.IsAny<string>(),
            null,
            null,
            true), Times.Once);
    }

    [Fact(DisplayName = "Login with wrong password fails and logs LOGIN_FAILED without leaking password")]
    public async Task Login_WithWrongPassword_Fails_And_LogsFailed()
    {
        // Arrange
        var user = new UserDto
        {
            Id = 2,
            FullName = "Sara Hassan",
            UserName = "sara",
            PasswordHash = PasswordHasher.Hash("CorrectPassword123!"),
            Role = "User",
            IsActive = true
        };

        _userRepoMock.Setup(r => r.GetByUserName("sara")).ReturnsAsync(user);

        var service = new AuthService(_userRepoMock.Object, _auditLogRepoMock.Object, _configuration);
        var request = new LoginRequest { UserName = "sara", Password = "WrongPassword999!" };

        // Act
        var result = await service.Login(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.InvalidCredentials);

        _auditLogRepoMock.Verify(a => a.LogAsync(
            2,
            "LOGIN_FAILED",
            "Auth",
            "sara",
            It.Is<string>(c => !c.Contains("WrongPassword999!")),
            null,
            null,
            false), Times.Once);
    }

    [Fact(DisplayName = "Login for non-existent user returns generic InvalidCredentials and logs failed audit")]
    public async Task Login_WithNonExistentUser_FailsGeneric_And_LogsFailed()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByUserName("unknown_user")).ReturnsAsync((UserDto?)null);

        var service = new AuthService(_userRepoMock.Object, _auditLogRepoMock.Object, _configuration);
        var request = new LoginRequest { UserName = "unknown_user", Password = "AnyPassword" };

        // Act
        var result = await service.Login(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.InvalidCredentials);

        _auditLogRepoMock.Verify(a => a.LogAsync(
            null,
            "LOGIN_FAILED",
            "Auth",
            "unknown_user",
            It.IsAny<string>(),
            null,
            null,
            false), Times.Once);
    }

    [Fact(DisplayName = "Login with inactive account returns UserInactive failure")]
    public async Task Login_WithInactiveAccount_ReturnsUserInactive()
    {
        // Arrange
        var user = new UserDto
        {
            Id = 3,
            FullName = "Disabled User",
            UserName = "disabled_user",
            PasswordHash = PasswordHasher.Hash("Pass123!"),
            Role = "User",
            IsActive = false
        };

        _userRepoMock.Setup(r => r.GetByUserName("disabled_user")).ReturnsAsync(user);

        var service = new AuthService(_userRepoMock.Object, _auditLogRepoMock.Object, _configuration);
        var request = new LoginRequest { UserName = "disabled_user", Password = "Pass123!" };

        // Act
        var result = await service.Login(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.UserInactive);

        _auditLogRepoMock.Verify(a => a.LogAsync(
            3,
            "LOGIN_FAILED",
            "Auth",
            "disabled_user",
            It.Is<string>(c => c.Contains("UserInactive")),
            null,
            null,
            false), Times.Once);
    }

    [Fact(DisplayName = "Register new user succeeds when username is unique")]
    public async Task Register_WithUniqueUsername_ReturnsToken()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FullName = "New Registrar",
            UserName = "registrar",
            Password = "Password123!",
            Role = "User"
        };

        _userRepoMock.Setup(r => r.IsUserNameTaken("registrar")).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.Add(It.IsAny<UserDto>(), It.IsAny<long?>()))
            .ReturnsAsync((UserDto dto, long? creator) =>
            {
                dto.Id = 10;
                return dto;
            });

        var service = new AuthService(_userRepoMock.Object, _auditLogRepoMock.Object, _configuration);

        // Act
        var result = await service.Register(request, creatorId: 1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.UserName.Should().Be("registrar");
        result.Data.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "Register fails when username already exists")]
    public async Task Register_WithDuplicateUsername_ReturnsDuplicateRecord()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FullName = "Duplicate User",
            UserName = "existing_user",
            Password = "Password123!",
            Role = "User"
        };

        _userRepoMock.Setup(r => r.IsUserNameTaken("existing_user")).ReturnsAsync(true);

        var service = new AuthService(_userRepoMock.Object, _auditLogRepoMock.Object, _configuration);

        // Act
        var result = await service.Register(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.DuplicateRecord);
    }
}
