using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OC_System_Training.Shared.Extensions;

// تعليق تدريبي: امتداد الأمان والمصادقة (ApplicationSecurityExtension)
// مسؤول عن:
// 1. إعداد مصادقة JWT Bearer Token وقراءة المفتاح السري من appsettings.json
// 2. إعداد توثيق Swagger وScalar لدعم تمرير توكن الـ Bearer عبر زر Authorize
public static class ApplicationSecurityExtension
{
    public static IServiceCollection AddSecurityExtension(this IServiceCollection services, IConfiguration config)
    {
        var secretKey = config["Jwt:SecretKey"]
                        ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                        ?? "SuperSecretKeyForOCSystemTrainingProject2026SecureMin32Bytes!";

        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var symmetricKey = new SymmetricSecurityKey(keyBytes);

        // إعداد Swagger لدعم إدخال الـ Bearer Token وإظهار القفل فقط على المسارات المحمية
        services.AddSwaggerGen(options =>
        {
            var scheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "أدخل رمز JWT مباشرة في خانة القيمة"
            };

            options.AddSecurityDefinition("Bearer", scheme);

            // تطبيق القفل فقط على المسارات التي تحمل [Authorize] واستثناء [AllowAnonymous]
            options.OperationFilter<AuthorizeOperationFilter>();
        });

        // إعداد JWT Bearer Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = symmetricKey,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }
}

/// <summary>
/// فلتر Swagger لتطبيق القفل الأمني وحفظ الـ Token فقط على الـ Endpoints المحمية بـ [Authorize]
/// واستثناء الـ Endpoints المفتوحة للعموم مثل [AllowAnonymous] كمسار تسجيل الدخول Login
/// </summary>
public class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAllowAnonymous = context.MethodInfo.GetCustomAttributes(true).OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>().Any()
                                || (context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>().Any() ?? false);

        if (hasAllowAnonymous)
            return;

        var hasAuthorize = context.MethodInfo.GetCustomAttributes(true).OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any()
                           || (context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any() ?? false);

        if (hasAuthorize)
        {
            operation.Responses ??= [];
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "غير مصرح - Unauthorized" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "محظور - Forbidden" });

            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer")] = []
            });
        }
    }
}
