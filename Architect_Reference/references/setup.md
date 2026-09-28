# Initial Setup: Configuration & Program.cs

This guide explains how to bootstrap a modern .NET application using the OC_System architecture.

## 1. Application Configuration (appsettings.json)
Configure your application settings in `appsettings.json` (and `appsettings.Development.json`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OC_System_Training_DB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "SecretKey": "SuperSecretKeyForOCSystemTrainingProject2026SecureMin32Bytes!"
  },
  "Swagger": {
    "Enabled": true
  },
  "Scalar": {
    "Enabled": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> [!NOTE]
> For local development, Windows Authentication (`Trusted_Connection=True;TrustServerCertificate=True;`) is recommended.

## 2. Program.cs Modern Integration
`Program.cs` is concise, modular, and delegates all configuration to specialized extension methods in `Shared/Extensions`.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure Infrastructure & Services
builder.Services.AddControllersExtension();
builder.Services.AddSecurityExtension(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddCustomCors();

var app = builder.Build();

// Run seeders
await DatabaseSeeder.SeedAsync(app.Services);

// Run standard application pipeline (Middleware, Auth, Swagger, Scalar, Controllers)
app.UseApplicationPipeline();

app.Run();
```

## 3. Global Usings
Common namespaces are registered globally via `GlobalUsings.cs` to eliminate repetitive using directives across modules:
```csharp
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Authorization;
global using OC_System_Training.Shared.Base;
global using OC_System_Training.Shared.Base.dto;
global using OC_System_Training.Shared.Constants;
global using OC_System_Training.Shared.Enums;
global using OC_System_Training.Shared.Extensions;
global using OC_System_Training.Shared.Helper;
global using OC_System_Training.Shared.Utils;
global using OC_System_Training.Infrastructure.Persistence;
```
