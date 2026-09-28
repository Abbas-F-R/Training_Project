using Microsoft.AspNetCore.Authorization;

namespace OC_System_Training.Infrastructure.Middleware;

// تعليق تدريبي: الوسيط البرمجي لسياق المستخدم (UserContextMiddleware)
// يقع موقعاً استراتيجياً في مسار المعالجة بين UseAuthentication() و UseAuthorization():
// 1. يتحقق من أن الطلبات الموثقة تحمل معرف المستخدم المطلوب
// 2. يضمن تجهيز سياق المستخدم قبل وصول الطلب للمتحكمات
public sealed class UserContextMiddleware(RequestDelegate next, ILogger<UserContextMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext http)
    {
        // استثناء الـ Endpoints التي تسمح بالوصول المجهول [AllowAnonymous] مثل تسجيل الدخول
        var endpoint = http.GetEndpoint();
        var anonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null;

        if (anonymous || http.User.Identity?.IsAuthenticated != true)
        {
            await next(http);
            return;
        }

        // قراءة معرف المستخدم من الـ Claims
        var userIdClaim = http.User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            logger.LogWarning("[UserContext] A validated token reached {Path} carrying no UserId", http.Request.Path);
            http.Response.StatusCode = StatusCodes.Status401Unauthorized;
            http.Response.ContentType = "application/json; charset=utf-8";
            await http.Response.WriteAsJsonAsync(new
            {
                Message = "التوكن المستخدم لا يحمل معرّف المستخدم (UserId)، يرجى تسجيل الدخول مجدداً."
            });
            return;
        }

        await next(http);
    }
}
