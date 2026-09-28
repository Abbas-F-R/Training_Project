---
name: oc-system-architect
description: Architectural guidance for OC_System (Official Correspondence System). Use when creating new modules, implementing CRUD operations, or replicating the project structure in other .NET projects using Dapper, Stored Procedures, and Feature-Based Architecture.
---

# OC_System Architect (Updated)

This skill provides architectural blueprints, coding conventions, base patterns, and implementation workflows for the modern OC_System architecture.

## Architecture Overview

The system employs a **Feature-Based (Vertical Slice) Architecture**:
1. **Features (`Features/<FeatureName>/`):** Self-contained domain modules containing Controllers, Dtos, Repositories, Services, Validators, and Sql scripts.
2. **Infrastructure (`Infrastructure/`):** Cross-cutting persistence (`DapperContext`, `BaseRepository`, `RepositoryWrapper`), Middleware, and external integrations.
3. **Shared (`Shared/`):** Universal base components (`BaseController`, `GenericController`, `IBaseService`, `CurrentUser`, `ServiceResult<T>`, `ServiceRequest<T>`, `Response<T>`), attributes, and modular pipeline extensions.

## Core Architectural Principles

- **Stored Procedures Only:** Raw SQL queries are not permitted in C# repositories. Every database interaction executes a Stored Procedure via Dapper.
- **Views for Queries:** Every table has a companion `vw_{TableName}` view joining foreign keys into display names. All read queries read from the view.
- **Generics & Base Classes:**
  - `BaseRepository<TView, TForm, TUpdate, TFilter>` provides standard SP-backed CRUD.
  - `GenericController<TView, TForm, TUpdate, TFilter>` exposes standardized REST endpoints.
- **FluentValidation:** Robust, strongly-typed request validation colocated within each feature's `Validators/` directory.
- **Scrutor Auto-DI:** Annotate classes with `[Scoped]`, `[Transient]`, or `[Singleton]`.
- **API Documentation & Testing:** Native integration for both **Swagger UI** and **Scalar API Reference** with Bearer JWT support.

## Reference Guides

- **Architecture Overview:** See [references/architecture.md](references/architecture.md)
- **Project Setup (Configuration & Program.cs):** See [references/setup.md](references/setup.md)
- **Modular Extensions & Pipeline:** See [references/extensions.md](references/extensions.md)
- **SQL (SPs & Views):** See [references/sql.md](references/sql.md)
- **DTOs (Form, Update, Filter, Response):** See [references/dto.md](references/dto.md)
- **Repositories & BaseRepository:** See [references/repository.md](references/repository.md)
- **Services & Results:** See [references/service.md](references/service.md)
- **Controllers & GenericController:** See [references/controller.md](references/controller.md)
- **DI & Security Features:** See [references/di.md](references/di.md)

## Implementation Workflow for New Features

1. **Requirements & Scope:** Determine domain entities, inputs, outputs, and business validations.
2. **Database Table & Constraints:** Create table with auditing columns (`CreatedBy`, `CreatedAt`, `IsDeleted`, etc.), check constraints, and filtered unique indexes (`WHERE IsDeleted = 0`).
3. **Database View (`vw_{Entity}`):** Join related tables to bring readable descriptions. Filter `WHERE IsDeleted = 0`.
4. **Stored Procedures (The 5 CRUD Procs):** Create `{Entity}GetById`, `{Entity}GetAll`, `{Entity}GetAllNotPaged`, `{Entity}Insert`, `{Entity}Update`, and `{Entity}Delete`.
5. **DTOs & Validators:** Create `Response`, `Form`, `Update`, `Filter` classes in `Features/<Entity>/Dtos/` and FluentValidation rules in `Features/<Entity>/Validators/`.
6. **Repository:** Create interface `I{Entity}Repository` and implementation inheriting `BaseRepository`. Register in `IRepositoryWrapper` if needed.
7. **Service:** Create interface `I{Entity}Service` and implementation inheriting `IBaseService`. Return `ServiceResult<T>` or `PagedOk`.
8. **Controller:** Create `[ApiController]` inheriting `GenericController` or `BaseController`. Expose endpoints with `[Authorize]`.
9. **Testing:** Test endpoints via Swagger (`/swagger`) or Scalar (`/scalar/v1`) using JWT bearer token.
