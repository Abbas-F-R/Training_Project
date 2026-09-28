# OC_System Architecture: Feature-Based & Vertical Slices

OC_System has evolved from a traditional flat layered structure into a **Feature-Based (Vertical Slice) Architecture**.

## Directory Structure

```text
OC_System/
├── Features/                  <-- High-cohesion domain features (Vertical Slices)
│   ├── Auth/
│   ├── Document/
│   ├── AdministrativeStructure/
│   ├── EmployeeProfile/
│   └── ... (each feature self-contained)
│       ├── Controllers/       <-- API endpoints for this feature
│       ├── Dtos/              <-- Form, Update, Filter, Response DTOs
│       ├── Repositories/      <-- Data access & interfaces (Dapper / SPs)
│       ├── Services/          <-- Business logic & interfaces
│       ├── Validators/        <-- FluentValidation rules
│       └── Mappings/          <-- AutoMapper profiles (if needed)
├── Infrastructure/            <-- Cross-cutting infrastructure implementations
│   ├── ExternalServices/      <-- S3, Firebase, Meilisearch, Redis
│   ├── Hubs/                  <-- SignalR hubs
│   ├── Middleware/            <-- Custom ASP.NET Core middleware
│   └── Persistence/           <-- DapperContext, BaseRepository, RepositoryWrapper
└── Shared/                    <-- Reusable core building blocks
    ├── Attributes/            <-- [Scoped], [Sqid], [IgnoreParameter], etc.
    ├── Base/                  <-- BaseController, GenericController, IBaseService, CurrentUser, dto/
    ├── Constants/             <-- Table names, message keys, route constants
    ├── Enums/                 <-- Shared enumerations
    ├── Extensions/            <-- Pipeline, Security, Services, Controllers, Cors extensions
    ├── Helper/                <-- Security, token, date helpers
    └── Utils/                 <-- Error translation, Sqid encoding, string formatting
```

## Why Feature-Based?

1. **High Cohesion:** Everything related to a specific domain (e.g., `Document` or `Auth`) lives together in one place. Developers do not jump across distant folders.
2. **Low Coupling:** Changes to one feature don't impact others.
3. **Scalability:** Teams can work on different features concurrently without merge conflicts.
4. **Preserved Consistency:** All features share the exact same `BaseRepository`, `GenericController`, `ServiceResult`, and conventions from `Shared/` and `Infrastructure/`.
