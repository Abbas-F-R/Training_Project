using OC_System_Training.Shared.Helper;

namespace OC_System_Training.Shared.Extensions;

// تعليق تدريبي: امتداد تهيئة المتحكمات (ControllersExtension)
// يربط محولات تشفير الـ Sqids في الـ JSON والـ ModelBinding، ويضبط مسارات الـ URLs لتكون بالحروف الصغيرة
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
            // إدراج مزود فك تشفير المعرفات Sqid للـ Route و Query Parameters
            options.ModelBinderProviders.Insert(0, new SqidModelBinderProvider());
        })
        .AddJsonOptions(options =>
        {
            // إدراج محول تشفير وفك تشفير المعرفات Sqid داخل نصوص الـ JSON
            options.JsonSerializerOptions.Converters.Add(new SqidJsonConverterFactory());
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

        return services;
    }
}
