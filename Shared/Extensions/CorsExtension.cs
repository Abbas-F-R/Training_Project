namespace OC_System_Training.Shared.Extensions;

/// <summary>
/// Cross-Origin Resource Sharing (CORS) policy configuration.
/// </summary>
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
