using FluentValidation;
using FluentValidation.AspNetCore;
using OC_System_Training.Infrastructure.Persistence;
using OC_System_Training.Shared.Attributes;

namespace OC_System_Training.Shared.Extensions;

// تعليق تدريبي: امتداد تسجيل الخدمات وحقن التبعيات التلقائي (ApplicationServicesExtension)
// يستخدم Scrutor لتسجيل الخدمات تلقائياً، ويسجل FluentValidation لفحص المدخلات
public static class ApplicationServicesExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();

        // 1. تسجيل DapperContext كـ Singleton
        services.AddSingleton<DapperContext>();

        // 2. تسجيل جميع مدققات FluentValidation في الـ Assembly تلقائياً
        var assembly = typeof(Program).Assembly;
        services.AddValidatorsFromAssembly(assembly);
        services.AddFluentValidationAutoValidation();

        // 3. تسجيل الخدمات ومستودعات البيانات تلقائياً عبر Scrutor
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            // تسجيل كلاسات [Scoped]
            .AddClasses(classes => classes.WithAttribute<ScopedAttribute>())
            .AsImplementedInterfaces()
            .AsSelf()
            .WithScopedLifetime()

            // تسجيل كلاسات [Transient]
            .AddClasses(classes => classes.WithAttribute<TransientAttribute>())
            .AsImplementedInterfaces()
            .AsSelf()
            .WithTransientLifetime()

            // تسجيل كلاسات [Singleton]
            .AddClasses(classes => classes.WithAttribute<SingletonAttribute>())
            .AsImplementedInterfaces()
            .AsSelf()
            .WithSingletonLifetime()
        );

        return services;
    }
}
