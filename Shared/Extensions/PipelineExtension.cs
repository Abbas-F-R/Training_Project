using Training_Project.Infrastructure.Middleware;
using Scalar.AspNetCore;

namespace Training_Project.Shared.Extensions;

/// <summary>
/// Configures the HTTP request processing pipeline in an engineered sequence:
/// Exception Handling -> CORS -> Interactive API Docs -> Routing -> Authentication -> UserContext -> Authorization -> Controllers.
/// </summary>
public static class PipelineExtension
{
    public static WebApplication UseApplicationPipeline(this WebApplication app)
    {
        // 0. Global Exception Handling: detailed errors in Dev, safe sanitized errors in Prod
        app.UseMiddleware<GlobalExceptionMiddleware>();

        // Apply CORS policy
        app.UseCors(CorsExtension.PolicyName);

        // Swagger and Scalar API documentation
        var isSwaggerEnabled = app.Configuration.GetValue<bool>("Swagger:Enabled", true);

        if (isSwaggerEnabled)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Student Management System API v1");
                c.RoutePrefix = "swagger";
            });

            var isScalarEnabled = app.Configuration.GetValue<bool>("Scalar:Enabled", true);

            if (isScalarEnabled)
            {
                app.MapScalarApiReference(options =>
                {
                    options
                        .WithTitle("Student Management System API — Scalar")
                        .WithOpenApiRoutePattern("/swagger/v1/swagger.json")
                        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
                }).AllowAnonymous();

                // Fallbacks ensuring Scalar static assets and OpenAPI schema resolve seamlessly across all path variants
                app.MapGet("/openapi/v1.json", () => Results.Redirect("/swagger/v1/swagger.json", permanent: true))
                    .AllowAnonymous().ExcludeFromDescription();

                app.MapGet("/scalar/v1/scalar.js", () => Results.Redirect("/scalar/scalar.js", permanent: true))
                    .AllowAnonymous().ExcludeFromDescription();

                app.MapGet("/scalar/v1/scalar.aspnetcore.js", () => Results.Redirect("/scalar/scalar.aspnetcore.js", permanent: true))
                    .AllowAnonymous().ExcludeFromDescription();

                app.MapGet("/scalar/swagger/v1/swagger.json", () => Results.Redirect("/swagger/v1/swagger.json", permanent: true))
                    .AllowAnonymous().ExcludeFromDescription();

                app.MapGet("/scalar/openapi/v1.json", () => Results.Redirect("/swagger/v1/swagger.json", permanent: true))
                    .AllowAnonymous().ExcludeFromDescription();

                app.MapGet("/scalar/v1/openapi/v1.json", () => Results.Redirect("/swagger/v1/swagger.json", permanent: true))
                    .AllowAnonymous().ExcludeFromDescription();
            }
        }

        app.UseRouting();

        // 1. Authenticate JWT Bearer Token
        app.UseAuthentication();
        
        // 2. Validate user identity & populate execution context
        app.UseMiddleware<UserContextMiddleware>();
        
        // 3. Evaluate endpoint authorization rules (RBAC)
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}
