using FluentValidation;
using FluentValidation.AspNetCore;
using OC_System_Training.Infrastructure.Persistence;
using OC_System_Training.Shared.Attributes;

namespace OC_System_Training.Shared.Extensions;

/// <summary>
/// Registers application services, persistence context, FluentValidation, and Scrutor automated DI scanning.
/// </summary>
public static class ApplicationServicesExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();

        // 1. Register DapperContext as Singleton
        services.AddSingleton<DapperContext>();

        // 2. Discover and register all FluentValidation validators
        var assembly = typeof(Program).Assembly;
        services.AddValidatorsFromAssembly(assembly);
        services.AddFluentValidationAutoValidation();

        // 3. Scan and register services automatically using Scrutor attributes
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            // Register [Scoped] types
            .AddClasses(classes => classes.WithAttribute<ScopedAttribute>())
            .AsImplementedInterfaces()
            .AsSelf()
            .WithScopedLifetime()

            // Register [Transient] types
            .AddClasses(classes => classes.WithAttribute<TransientAttribute>())
            .AsImplementedInterfaces()
            .AsSelf()
            .WithTransientLifetime()

            // Register [Singleton] types
            .AddClasses(classes => classes.WithAttribute<SingletonAttribute>())
            .AsImplementedInterfaces()
            .AsSelf()
            .WithSingletonLifetime()
        );

        return services;
    }
}
