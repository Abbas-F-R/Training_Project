using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Training_Project.Features.AuditLogs.Repositories;
using Training_Project.Features.Auth.Dtos;
using Training_Project.Features.Auth.Repositories;
using Training_Project.Features.Auth.Services;
using Training_Project.Shared.Constants;
using Training_Project.Shared.Utils;
using Xunit;

namespace Training_Project.Tests.Features.Auth.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IAuditLogRepository> _auditRepoMock = new();
    private readonly IConfiguration _config;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:SecretKey", "ThisIsASecretKeyForTestingPurposesOnly123456789Min32Bytes!" },
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" },
            { "Jwt:DurationInMinutes", "60" }
        };
        _config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

        _authService = new AuthService(_userRepoMock.Object, _auditRepoMock.Object, _config);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSuccessAndJwtToken()
    {
        var password = "ValidPassword123!";
        var user = new UserDto
        {
            Id = 1,
            UserName = "admin",
            FullName = "Admin User",
            PasswordHash = PasswordHasher.Hash(password),
            Role = "Admin",
            IsActive = true
        };

        _userRepoMock.Setup(r => r.GetByUserName("admin")).ReturnsAsync(user);

        var request = new LoginRequest { UserName = "admin", Password = password };
        var result = await _authService.Login(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().NotBeNullOrWhiteSpace();
        result.Data.UserName.Should().Be("admin");
        result.Data.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsFailure()
    {
        var user = new UserDto
        {
            Id = 1,
            UserName = "admin",
            FullName = "Admin User",
            PasswordHash = PasswordHasher.Hash("CorrectPassword!"),
            Role = "Admin",
            IsActive = true
        };

        _userRepoMock.Setup(r => r.GetByUserName("admin")).ReturnsAsync(user);

        var request = new LoginRequest { UserName = "admin", Password = "WrongPassword!" };
        var result = await _authService.Login(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.InvalidCredentials);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsFailure()
    {
        _userRepoMock.Setup(r => r.GetByUserName("unknown_user")).ReturnsAsync((UserDto?)null);

        var request = new LoginRequest { UserName = "unknown_user", Password = "AnyPassword!" };
        var result = await _authService.Login(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.InvalidCredentials);
    }

    [Fact]
    public async Task Login_WithInactiveUser_ReturnsAccountDisabled()
    {
        var password = "ValidPassword123!";
        var user = new UserDto
        {
            Id = 1,
            UserName = "inactive_user",
            FullName = "Inactive User",
            PasswordHash = PasswordHasher.Hash(password),
            Role = "User",
            IsActive = false
        };

        _userRepoMock.Setup(r => r.GetByUserName("inactive_user")).ReturnsAsync(user);

        var request = new LoginRequest { UserName = "inactive_user", Password = password };
        var result = await _authService.Login(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.UserInactive);
    }

    [Fact]
    public async Task Register_WithNewUser_SucceedsAndReturnsToken()
    {
        _userRepoMock.Setup(r => r.IsUserNameTaken("newuser")).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.Add(It.IsAny<UserDto>(), It.IsAny<long?>()))
            .ReturnsAsync((UserDto dto, long? creator) =>
            {
                dto.Id = 99;
                return dto;
            });

        var request = new RegisterRequest
        {
            FullName = "New User",
            UserName = "newuser",
            Password = "Password123!",
            Role = "User"
        };

        var result = await _authService.Register(request, creatorId: 1);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().NotBeNullOrWhiteSpace();
        result.Data.UserName.Should().Be("newuser");
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsFailure()
    {
        _userRepoMock.Setup(r => r.IsUserNameTaken("existinguser")).ReturnsAsync(true);

        var request = new RegisterRequest
        {
            FullName = "Duplicate User",
            UserName = "existinguser",
            Password = "Password123!",
            Role = "User"
        };

        var result = await _authService.Register(request, creatorId: 1);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.DuplicateRecord);
    }
}
