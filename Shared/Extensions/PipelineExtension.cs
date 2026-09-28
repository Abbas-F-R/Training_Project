using OC_System_Training.Infrastructure.Middleware;
using Scalar.AspNetCore;

namespace OC_System_Training.Shared.Extensions;

// تعليق تدريبي: امتداد مسار المعالجة (PipelineExtension)
// يجمع كافة الـ Middlewares في مكان واحد مرتب بدقة هندسية:
// 1. Exception Handler
// 2. CORS
// 3. Swagger & Scalar
// 4. Authentication -> UserContextMiddleware -> Authorization
// 5. Controllers Mapping
public static class PipelineExtension
{
    public static WebApplication UseApplicationPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // تطبيق سياسة CORS
        app.UseCors(CorsExtension.PolicyName);

        // قراءة تفعيل Swagger و Scalar من appsettings.json
        var isSwaggerEnabled = app.Configuration.GetValue<bool>("Swagger:Enabled", true);

        if (isSwaggerEnabled)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OC System Training API v1");
                c.RoutePrefix = "swagger";
            });

            var isScalarEnabled = app.Configuration.GetValue<bool>("Scalar:Enabled", true);

            if (isScalarEnabled)
            {
                app.MapScalarApiReference(options =>
                {
                    options
                        .WithTitle("OC System Training API — Scalar")
                        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
                }).AllowAnonymous();
            }
        }

        app.UseRouting();

        // الترتيب هنا جوهري وحاسم:
        // 1. التحقق من هوية صاحب الطلب (Authentication)
        // 2. التحقق من سلامة سياق المستخدم في النظام (UserContextMiddleware)
        // 3. تطبيق قواعد الصلاحيات والمنع (Authorization)
        app.UseAuthentication();
        app.UseMiddleware<UserContextMiddleware>();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}
