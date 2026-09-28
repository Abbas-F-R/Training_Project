using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Training_Project.Shared.Base;
using Xunit;

namespace Training_Project.Tests.Shared.Base;

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

    [Fact]
    public void CurrentUser_WithAcceptLanguageHeader_ResolvesArabicLanguage()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["Accept-Language"] = "ar,en-US;q=0.9";

        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

        var currentUser = new CurrentUser(_httpContextAccessorMock.Object);

        currentUser.Lang.Should().Be("ar");
    }

    [Fact]
    public void CurrentUser_AcceptLanguageHeaderOverridesJwtClaim()
    {
        var claims = new List<Claim> { new("Lang", "en") };
        var identity = new ClaimsIdentity(claims, "Test");
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        context.Request.Headers["Accept-Language"] = "ar-SA";

        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

        var currentUser = new CurrentUser(_httpContextAccessorMock.Object);

        currentUser.Lang.Should().Be("ar");
    }

    [Fact]
    public void CurrentUser_WithEnglishAcceptLanguageHeader_ResolvesEnglishLanguage()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["Accept-Language"] = "en-US,en;q=0.8";

        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

        var currentUser = new CurrentUser(_httpContextAccessorMock.Object);

        currentUser.Lang.Should().Be("en");
    }
}
