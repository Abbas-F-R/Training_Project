using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using OC_System_Training.Shared.Base;
using Xunit;

namespace OC_System_Training.Tests.Shared.Base;

public class CurrentUserTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();

    [Fact]
    public void CurrentUser_WithClaims_ResolvesPropertiesCorrectly()
    {
        var claims = new List<Claim>
        {
            new("UserId", "42"),
            new("UserName", "test_admin"),
            new("FullName", "Test Admin User"),
            new(ClaimTypes.Role, "Admin"),
            new("Lang", "ar")
        };
        var identity = new ClaimsIdentity(claims, "AuthenticationTypes.Federation");
        var principal = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = principal };

        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

        var currentUser = new CurrentUser(_httpContextAccessorMock.Object);

        currentUser.IsAuthenticated.Should().BeTrue();
        currentUser.UserId.Should().Be(42);
        currentUser.UserName.Should().Be("test_admin");
        currentUser.FullName.Should().Be("Test Admin User");
        currentUser.Role.Should().Be("Admin");
        currentUser.Lang.Should().Be("ar");
    }

    [Fact]
    public void CurrentUser_WithoutClaims_ReturnsDefaultValues()
    {
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var currentUser = new CurrentUser(_httpContextAccessorMock.Object);

        currentUser.IsAuthenticated.Should().BeFalse();
        currentUser.UserId.Should().Be(0);
        currentUser.UserName.Should().BeEmpty();
        currentUser.FullName.Should().BeEmpty();
        currentUser.Role.Should().Be("User");
        currentUser.Lang.Should().Be("en");
    }
}
