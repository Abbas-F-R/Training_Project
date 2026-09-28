using System.Security.Claims;
using Training_Project.Shared.Attributes;

namespace Training_Project.Shared.Base;

/// <summary>
/// Exposes security claims and identity details of the currently authenticated user.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    long UserId { get; }
    string UserName { get; }
    string FullName { get; }
    string Role { get; }
    string Lang { get; }
}

/// <summary>
/// Scoped resolution of user claims from the active HttpContext.
/// </summary>
[Scoped]
public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private HttpContext? HttpContext => httpContextAccessor.HttpContext;
    private ClaimsPrincipal? Principal => HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public long UserId
    {
        get
        {
            var val = GetClaim("UserId") ?? GetClaim(ClaimTypes.NameIdentifier);
            return long.TryParse(val, out var id) ? id : 0;
        }
    }

    public string UserName => GetClaim("UserName") ?? GetClaim(ClaimTypes.Name) ?? string.Empty;
    public string FullName => GetClaim("FullName") ?? UserName;
    public string Role => GetClaim(ClaimTypes.Role) ?? GetClaim("Role") ?? "User";
    public string Lang
    {
        get
        {
            var httpContext = HttpContext;
            if (httpContext != null && httpContext.Request.Headers.TryGetValue("Accept-Language", out var acceptHeader) &&
                !string.IsNullOrWhiteSpace(acceptHeader))
            {
                var primary = acceptHeader.ToString().Split(',', ';')[0].Trim().ToLowerInvariant();
                if (primary.StartsWith("ar")) return "ar";
                if (primary.StartsWith("en")) return "en";
            }

            var claimLang = GetClaim("Lang");
            if (!string.IsNullOrWhiteSpace(claimLang))
            {
                var norm = claimLang.Trim().ToLowerInvariant();
                if (norm == "ar" || norm == "en") return norm;
            }

            return "en";
        }
    }

    private string? GetClaim(string claimType)
    {
        return Principal?.FindFirst(c => string.Equals(c.Type, claimType, StringComparison.OrdinalIgnoreCase))?.Value;
    }
}
