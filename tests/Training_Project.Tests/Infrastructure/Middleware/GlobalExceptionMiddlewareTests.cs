using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Training_Project.Infrastructure.Middleware;
using Xunit;

namespace Training_Project.Tests.Infrastructure.Middleware;

public class GlobalExceptionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_InDevelopment_ReturnsDetailedErrorResponse()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var envMock = new Mock<IHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);

        var middleware = new GlobalExceptionMiddleware(
            _ => throw new InvalidOperationException("Detailed dev database error occurred!"),
            NullLogger<GlobalExceptionMiddleware>.Instance,
            envMock.Object
        );

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().StartWith("application/json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();

        using var jsonDoc = JsonDocument.Parse(responseBody);
        var root = jsonDoc.RootElement;

        root.GetProperty("status").GetInt32().Should().Be(500);
        root.GetProperty("message").GetString().Should().Be("Detailed dev database error occurred!");
        root.GetProperty("exceptionType").GetString().Should().Be("System.InvalidOperationException");
        root.GetProperty("errorType").GetString().Should().Be("Unhandled");
        root.TryGetProperty("stackTrace", out var stackTrace).Should().BeTrue();
        stackTrace.GetString().Should().NotBeNull();
    }

    [Fact]
    public async Task InvokeAsync_InProduction_ReturnsSanitizedErrorResponse()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var envMock = new Mock<IHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        var middleware = new GlobalExceptionMiddleware(
            _ => throw new InvalidOperationException("Sensitive internal database connection string error!"),
            NullLogger<GlobalExceptionMiddleware>.Instance,
            envMock.Object
        );

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().StartWith("application/json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();

        using var jsonDoc = JsonDocument.Parse(responseBody);
        var root = jsonDoc.RootElement;

        root.GetProperty("status").GetInt32().Should().Be(500);
        root.GetProperty("message").GetString().Should().NotContain("Sensitive internal database");
        root.GetProperty("message").GetString().Should().Contain("unexpected error occurred");
        root.TryGetProperty("stackTrace", out _).Should().BeFalse();
        root.TryGetProperty("exceptionType", out _).Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_PassesThroughWithoutWritingError()
    {
        var context = new DefaultHttpContext();
        var nextExecuted = false;

        var envMock = new Mock<IHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);

        var middleware = new GlobalExceptionMiddleware(
            _ =>
            {
                nextExecuted = true;
                return Task.CompletedTask;
            },
            NullLogger<GlobalExceptionMiddleware>.Instance,
            envMock.Object
        );

        await middleware.InvokeAsync(context);

        nextExecuted.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}
