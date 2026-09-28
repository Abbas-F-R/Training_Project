using Microsoft.AspNetCore.Authorization;

namespace OC_System_Training.Infrastructure.Middleware;

/// <summary>
/// Pipeline middleware positioned between Authentication and Authorization.
/// Ensures authenticated requests contain an extracted UserId claim and validates context integrity.
/// </summary>
public sealed class UserContextMiddleware(RequestDelegate next, ILogger<UserContextMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext http)
    {
        // Skip validation for public or anonymous endpoints
        var endpoint = http.GetEndpoint();
        var anonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null;

        if (anonymous || http.User.Identity?.IsAuthenticated != true)
        {
            await next(http);
            return;
        }

        // Extract user identifier claim from token
        var userIdClaim = http.User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            logger.LogWarning("[UserContext] A validated token reached {Path} carrying no UserId", http.Request.Path);
            http.Response.StatusCode = StatusCodes.Status401Unauthorized;
            http.Response.ContentType = "application/json; charset=utf-8";
            await http.Response.WriteAsJsonAsync(new
            {
                Message = "The provided token does not contain a valid user identifier (UserId). Please authenticate again."
            });
            return;
        }

        await next(http);
    }
}
