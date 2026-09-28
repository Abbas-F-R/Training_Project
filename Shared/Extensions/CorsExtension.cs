namespace OC_System_Training.Shared.Extensions;

// تعليق تدريبي: امتداد سياسات مشاركة الموارد (CORS Policy)
public static class CorsExtension
{
    public const string PolicyName = "AllowSpecificOrigin";

    public static IServiceCollection AddCustomCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        return services;
    }
}
