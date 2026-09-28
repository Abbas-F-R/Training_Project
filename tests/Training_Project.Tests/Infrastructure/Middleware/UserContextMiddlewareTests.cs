using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Training_Project.Infrastructure.Middleware;
using Xunit;

namespace Training_Project.Tests.Infrastructure.Middleware;

public class UserContextMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithoutUserIdClaim_Returns401Unauthorized()
    {
        var context = new DefaultHttpContext();
        var claims = new List<Claim> { new(ClaimTypes.Name, "user_without_id") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var middleware = new UserContextMiddleware(
            _ => Task.CompletedTask,
            NullLogger<UserContextMiddleware>.Instance
        );

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task InvokeAsync_WithUserIdClaim_ProceedsToNextMiddleware()
    {
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new("UserId", "42"),
            new(ClaimTypes.Name, "valid_user")
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var nextExecuted = false;
        var middleware = new UserContextMiddleware(
            _ =>
            {
                nextExecuted = true;
                return Task.CompletedTask;
            },
            NullLogger<UserContextMiddleware>.Instance
        );

        await middleware.InvokeAsync(context);

        nextExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_UnauthenticatedRequest_BypassesCheckAndProceeds()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity()); // IsAuthenticated = false

        var nextExecuted = false;
        var middleware = new UserContextMiddleware(
            _ =>
            {
                nextExecuted = true;
                return Task.CompletedTask;
            },
            NullLogger<UserContextMiddleware>.Instance
        );

        await middleware.InvokeAsync(context);

        nextExecuted.Should().BeTrue();
    }
}
