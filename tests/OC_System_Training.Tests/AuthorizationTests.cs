using System.Reflection;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using OC_System_Training.Features.AuditLogs.Controllers;
using OC_System_Training.Features.Auth.Controllers;
using OC_System_Training.Features.Departments.Controllers;
using OC_System_Training.Features.Students.Controllers;
using OC_System_Training.Infrastructure.Middleware;
using Xunit;

namespace OC_System_Training.Tests;

public class AuthorizationTests
{
    [Fact(DisplayName = "DepartmentController write endpoints require Admin role authorization")]
    public void DepartmentController_WriteEndpoints_RequireAdminRole()
    {
        var controllerType = typeof(DepartmentController);

        var addMethod = controllerType.GetMethod(nameof(DepartmentController.Add));
        var updateMethod = controllerType.GetMethod(nameof(DepartmentController.Update));
        var deleteMethod = controllerType.GetMethod(nameof(DepartmentController.Delete));

        var addAuth = addMethod?.GetCustomAttribute<AuthorizeAttribute>();
        var updateAuth = updateMethod?.GetCustomAttribute<AuthorizeAttribute>();
        var deleteAuth = deleteMethod?.GetCustomAttribute<AuthorizeAttribute>();

        addAuth.Should().NotBeNull();
        addAuth!.Roles.Should().Be("Admin");

        updateAuth.Should().NotBeNull();
        updateAuth!.Roles.Should().Be("Admin");

        deleteAuth.Should().NotBeNull();
        deleteAuth!.Roles.Should().Be("Admin");
    }

    [Fact(DisplayName = "StudentController delete endpoint requires Admin role authorization")]
    public void StudentController_DeleteEndpoint_RequiresAdminRole()
    {
        var controllerType = typeof(StudentController);
        var deleteMethod = controllerType.GetMethod(nameof(StudentController.Delete));

        var deleteAuth = deleteMethod?.GetCustomAttribute<AuthorizeAttribute>();

        deleteAuth.Should().NotBeNull();
        deleteAuth!.Roles.Should().Be("Admin");
    }

    [Fact(DisplayName = "AuditLogController class level requires Admin role authorization")]
    public void AuditLogController_RequiresAdminRole()
    {
        var controllerType = typeof(AuditLogController);
        var classAuth = controllerType.GetCustomAttribute<AuthorizeAttribute>();

        classAuth.Should().NotBeNull();
        classAuth!.Roles.Should().Be("Admin");
    }

    [Fact(DisplayName = "AuthController register endpoint requires Admin role authorization")]
    public void AuthController_RegisterEndpoint_RequiresAdminRole()
    {
        var controllerType = typeof(AuthController);
        var registerMethod = controllerType.GetMethod(nameof(AuthController.Register));

        var authAttr = registerMethod?.GetCustomAttribute<AuthorizeAttribute>();

        authAttr.Should().NotBeNull();
        authAttr!.Roles.Should().Be("Admin");
    }

    [Fact(DisplayName = "UserContextMiddleware returns 401 Unauthorized if authenticated token lacks UserId claim")]
    public async Task UserContextMiddleware_WithoutUserIdClaim_Returns401()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var claims = new List<Claim> { new(ClaimTypes.Name, "user_without_id") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var middleware = new UserContextMiddleware(
            _ => Task.CompletedTask,
            NullLogger<UserContextMiddleware>.Instance
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }
}
