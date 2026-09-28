var builder = WebApplication.CreateBuilder(args);

// 1. Controller registration with Sqids encoding & FluentValidation
builder.Services.AddControllersExtension();

// 2. Authentication, JWT Bearer configuration & API documentation (Swagger/Scalar)
builder.Services.AddSecurityExtension(builder.Configuration);

// 3. Application services, persistence layer & Scrutor Auto-DI
builder.Services.AddApplicationServices(builder.Configuration);

// 4. CORS configuration
builder.Services.AddCustomCors();

var app = builder.Build();

// 5. Seed default administrative & testing user accounts
await DatabaseSeeder.SeedAsync(app.Services);

// 6. Application pipeline middleware configuration
app.UseApplicationPipeline();

app.Run();
