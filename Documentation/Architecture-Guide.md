# Architecture Guide

### Vertical Slice Architecture, In-Transaction Auditing, and Clean Layered Design

---

## 1. Architectural Overview

The system implements **Vertical Slice Architecture** (also known as Feature-Based Architecture). Rather than splitting code across distant horizontal folders (placing all controllers in one folder, all services in another, etc.), every feature is self-contained under `Features/<FeatureName>/`.

A single feature folder encapsulates everything required to deliver that slice of business capability: Controllers, DTOs, Services, Repositories, Validators, and feature-specific SQL scripts.

```mermaid
graph TD
    Client["Client / Consumer"] --> Pipeline["HTTP Request Pipeline"]
    Pipeline --> Auth["Authentication (JWT Bearer)"]
    Auth --> UserCtx["User Context Middleware (Claims Extraction)"]
    UserCtx --> Authorize["Authorization Filter (Role: Admin / User)"]
    Authorize --> Validator["Input Validation (FluentValidation)"]
    Validator --> Controller["Controller (GenericController / BaseController)"]
    Controller --> Service["Domain Service (Business Rules & Validation)"]
    Service --> Repos["Data Access (BaseRepository / RepositoryWrapper)"]
    Repos --> Dapper["Connection Management (DapperContext)"]
    Dapper --> DB[("SQL Server (Views, SPs & In-Transaction Audit)")]
```

---

## 2. Directory & Component Layout

```text
Training_Project/
â”œâ”€â”€ Features/                              <-- Vertical Slice Modules
â”‚   â”œâ”€â”€ AuditLogs/                         <-- Immutable Audit Trail Feature
â”‚   â”‚   â”œâ”€â”€ Controllers/                   <-- AuditLogController [Authorize(Roles = "Admin")]
â”‚   â”‚   â”œâ”€â”€ Dtos/                          <-- AuditLogFilter, AuditLogResponse
â”‚   â”‚   â”œâ”€â”€ Repositories/                  <-- IAuditLogRepository, AuditLogRepository
â”‚   â”‚   â”œâ”€â”€ Services/                      <-- IAuditLogService, AuditLogService
â”‚   â”‚   â””â”€â”€ Sql/                           <-- Tables, indexes, and stored procedures
â”‚   â”œâ”€â”€ Auth/                              <-- Authentication & Identity Feature
â”‚   â”‚   â”œâ”€â”€ Controllers/                   <-- AuthController
â”‚   â”‚   â”œâ”€â”€ Dtos/                          <-- LoginRequest, LoginResponse, RegisterRequest, UserDto
â”‚   â”‚   â”œâ”€â”€ Repositories/                  <-- IUserRepository, UserRepository
â”‚   â”‚   â”œâ”€â”€ Services/                      <-- IAuthService, AuthService
â”‚   â”‚   â”œâ”€â”€ Sql/                           <-- Users tables, constraints, indexes, SPs
â”‚   â”‚   â””â”€â”€ Validators/                    <-- LoginRequestValidator, RegisterRequestValidator
â”‚   â”œâ”€â”€ Departments/                       <-- Academic Departments Feature
â”‚   â”‚   â”œâ”€â”€ Controllers/                   <-- DepartmentController (CRUD + Lookup)
â”‚   â”‚   â”œâ”€â”€ Dtos/                          <-- DepartmentForm, DepartmentUpdate, DepartmentFilter, DepartmentResponse
â”‚   â”‚   â”œâ”€â”€ Repositories/                  <-- IDepartmentRepository, DepartmentRepository
â”‚   â”‚   â”œâ”€â”€ Services/                      <-- IDepartmentService, DepartmentService
â”‚   â”‚   â”œâ”€â”€ Sql/                           <-- Tables, views, and stored procedures
â”‚   â”‚   â””â”€â”€ Validators/                    <-- DepartmentFormValidator, DepartmentUpdateValidator
â”‚   â””â”€â”€ Students/                          <-- Student Information Feature
â”‚       â”œâ”€â”€ Controllers/                   <-- StudentController
â”‚       â”œâ”€â”€ Dtos/                          <-- StudentForm, StudentUpdate, StudentFilter, StudentResponse
â”‚       â”œâ”€â”€ Repositories/                  <-- IStudentRepository, StudentRepository
â”‚       â”œâ”€â”€ Services/                      <-- IStudentService, StudentService
â”‚       â”œâ”€â”€ Sql/                           <-- Tables, views, constraints, and stored procedures
â”‚       â””â”€â”€ Validators/                    <-- StudentFormValidator, StudentUpdateValidator, StudentFilterValidator
â”œâ”€â”€ Infrastructure/                        <-- Shared Infrastructure & Persistence
â”‚   â”œâ”€â”€ Middleware/                        <-- UserContextMiddleware
â”‚   â””â”€â”€ Persistence/                       <-- DapperContext, DatabaseSeeder, BaseRepository, RepositoryWrapper
â”‚       â””â”€â”€ Sql/                           <-- 00_Base_Procedures.sql, 01_SeedData.sql
â”œâ”€â”€ Shared/                                <-- Shared Architectural Building Blocks
â”‚   â”œâ”€â”€ Attributes/                        <-- [Scoped], [Transient], [Singleton], [Sqid], [IgnoreParameter]
â”‚   â”œâ”€â”€ Base/                              <-- BaseController, GenericController, IBaseService, CurrentUser, dto/
â”‚   â”œâ”€â”€ Constants/                         <-- DbConstants, Messages
â”‚   â”œâ”€â”€ Enums/                             <-- LanguageType
â”‚   â”œâ”€â”€ Extensions/                        <-- Pipeline, Security, Services, Controllers, Cors
â”‚   â””â”€â”€ Utils/                             <-- ErrorMessagesUtils, PasswordHasher, SqidCodec
â”œâ”€â”€ tests/                                 <-- Automated Test Suite (78 Tests)
â”‚   â””â”€â”€ Training_Project.Tests/
â”œâ”€â”€ Documentation/                         <-- Technical Architecture Guides
â”œâ”€â”€ sql/                                   <-- MasterMigration.sql (Consolidated DB script)
â”œâ”€â”€ appsettings.json                       <-- Configuration
â””â”€â”€ Program.cs                             <-- Minimalist composition root
```

---

## 3. Core Architectural Layers & Responsibilities

### A. Immutable Audit Trail Layer
- The `AuditLogs` table follows an **Append-Only** pattern (no updates or deletes permitted).
- Database mutations (`INSERT`, `UPDATE`, `DELETE`) are audited within the exact same database transaction as the business operation, guaranteeing absolute atomicity.
- Authentication events (`LOGIN_SUCCESS`, `LOGIN_FAILED`) are logged via `AuthService` with sensitive credentials strictly excluded.

### B. High-Performance Stored Procedures & Views
- **Views for Queries:** All entity queries target relational views (`vw_Students`, `vw_Departments`) which join foreign keys into descriptive labels and filter soft-deleted rows (`WHERE IsDeleted = 0`).
- **Stored Procedures for Mutations:** All data modifications are executed through dedicated Stored Procedures, preventing SQL injection and maximizing query plan reuse.
- **Lookup Optimization:** Dropdown lists use optimized lookup endpoints (`GET /api/department/lookup`) rather than unbounded unpaginated queries.

### C. Role-Based Access Control (RBAC)
- Clearly separated roles: `Admin` (full administrative permissions including deletions, user provisioning, and audit logs) and `User` (standard academic registrar access).
- Strict enforcement of HTTP status codes: `401 Unauthorized` for missing/invalid authentication and `403 Forbidden` for authenticated requests lacking necessary roles.

### D. Three-Tier Defense-in-Depth Validation
1. **FluentValidation:** Validates input structure, ranges, formats, and required fields before reaching business services.
2. **Service Layer:** Enforces domain invariants, entity uniqueness via `IsDuplicateAsync`, and relational existence (e.g., verifying referenced department exists).
3. **Database Constraints:** Guarantees data integrity at rest using CHECK constraints, foreign keys, and unique filtered indexes (`WHERE IsDeleted = 0`).
