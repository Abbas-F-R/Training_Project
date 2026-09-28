using System.Security.Claims;
using OC_System_Training.Shared.Attributes;

namespace OC_System_Training.Shared.Base;

// تعليق تدريبي: واجهة وكلاس المستخدم الحالي (CurrentUser)
// تتيح لأي كلاس أو خدمة حقن ICurrentUser للوصول لمعلومات المستخدم المتصل (من واقع الـ JWT Token)
// دون الحاجة لتمرير HttpContext يدوياً أو قراءة الـ Claims في كل مكان
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    long UserId { get; }
    string UserName { get; }
    string FullName { get; }
    string Role { get; }
    string Lang { get; }
}

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
    public string Lang => GetClaim("Lang") ?? "ar";

    private string? GetClaim(string claimType)
    {
        return Principal?.FindFirst(c => string.Equals(c.Type, claimType, StringComparison.OrdinalIgnoreCase))?.Value;
    }
}
