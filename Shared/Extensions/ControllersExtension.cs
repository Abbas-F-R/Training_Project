using Training_Project.Shared.Helper;

namespace Training_Project.Shared.Extensions;

/// <summary>
/// Registers API controllers with lowercase URL routing, Sqids model binding, and camelCase JSON serialization.
/// </summary>
public static class ControllersExtension
{
    public static IServiceCollection AddControllersExtension(this IServiceCollection services)
    {
        services.AddRouting(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = false;
        });

        services.AddControllers(options =>
        {
            // Register model binder provider to decode Sqids in route and query parameters
            options.ModelBinderProviders.Insert(0, new SqidModelBinderProvider());
        })
        .AddJsonOptions(options =>
        {
            // Register JSON converter to encode/decode Sqids properties in request/response payloads
            options.JsonSerializerOptions.Converters.Add(new SqidJsonConverterFactory());
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

        return services;
    }
}
