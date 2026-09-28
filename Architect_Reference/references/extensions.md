# OC_System Extensions Reference

In OC_System, all service configuration and middleware pipeline logic are encapsulated into clean extension methods located in `Shared/Extensions`.

## 1. PipelineExtension.cs (`UseApplicationPipeline`)
**Purpose:** Replaces the messy middleware setup in `Program.cs` with an ordered, load-bearing pipeline.
**Key Duties:**
- Global exception handling (`UseExceptionHandler`).
- Response compression & CORS policy.
- Swagger & Scalar UI mapping:
  - `app.UseSwagger()` & `app.UseSwaggerUI()`
  - `app.MapScalarApiReference(options => options.WithTitle(...))`
- Security pipeline order (strict!):
  1. `app.UseAuthentication()`
  2. `app.UseMiddleware<UserContextMiddleware>()` (resolves `CurrentUser` / `AccountContext` once per request)
  3. `app.UseAuthorization()`
- Health check endpoints (`/health`).
- SignalR Hub mapping (`app.MapHub()`).
- Controller mapping (`app.MapControllers()`).

## 2. ControllersExtension.cs (`AddControllersExtension`)
**Purpose:** Configures MVC controllers, JSON serializers, and model binders.
- Adds `SqidModelBinderProvider` so route and query parameters decorated with `[Sqid]` automatically decode.
- Configures JSON options to use `SqidJsonConverter` for automatic encoding/decoding of entity IDs.
- Enforces lowercase routing conventions.

## 3. ApplicationSecurityExtension.cs (`AddSecurityExtension`)
**Purpose:** Configures JWT Authentication and Swagger/OpenAPI security definitions.
- Reads `JWT_SECRET_KEY` from environment or configuration.
- Sets up `JwtBearerOptions` with TokenValidationParameters (Issuer, Audience, Lifetime, SecurityKey).
- Adds Swagger security definition (`Bearer`) so developers can test authenticated endpoints in Swagger UI and Scalar.

## 4. ApplicationServicesExtension.cs (`AddApplicationServices`)
**Purpose:** Registers core services and automatic dependency injection.
- Registers `DapperContext` as a Singleton.
- Registers AutoMapper & FluentValidation from the current assembly.
- Uses **Scrutor** to scan the assembly and automatically register classes decorated with `[Scoped]`, `[Transient]`, or `[Singleton]`.

## 5. CorsExtension.cs (`AddCustomCors`)
**Purpose:** Centralizes CORS policies for frontend clients (development and production origins).

## 6. KeysHandler.cs
**Purpose:** Helper for extracting symmetric security keys for JWT signing and token verification.
