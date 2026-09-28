# Core Architecture & Foundation Components Reference
### Comprehensive Engineering Specification of Base Classes, Shared Utilities, Contexts, and Security Extensions

---

## Component Index

1. [`BaseController.cs`](#1-basecontrollercs)
2. [`GenericController.cs`](#2-genericcontrollercs)
3. [`IBaseService.cs`](#3-ibaseservicecs)
4. [`CurrentUser.cs`](#4-currentusercs)
5. [`BaseFilter.cs`](#5-basefiltercs)
6. [`Response.cs`](#6-responsecs)
7. [`ServiceRequest.cs`](#7-servicerequestcs)
8. [`ServiceResult.cs`](#8-serviceresultcs)
9. [`BaseRepository.cs`](#9-baserepositorycs)
10. [`IBaseRepository.cs`](#10-ibaserepositorycs)
11. [`RepositoryWrapper.cs`](#11-repositorywrappercs)
12. [`DapperContext.cs`](#12-dappercontextcs)
13. [`PipelineExtension.cs`](#13-pipelineextensioncs)
14. [`ApplicationSecurityExtension.cs`](#14-applicationsecurityextensioncs)
15. [`ApplicationServicesExtension.cs`](#15-applicationservicesextensioncs)
16. [`ControllersExtension.cs`](#16-controllersextensioncs)
17. [`DiAttributes.cs`](#17-diattributescs)
18. [`SqidAttribute.cs` & `SqidCodec.cs`](#18-sqidattributecs--sqidcodeccs)
19. [`IgnoreParameterAttribute.cs`](#19-ignoreparameterattributecs)
20. [`ErrorMessagesUtils.cs`](#20-errormessagesutilscs)
21. [`PasswordHasher.cs`](#21-passwordhashercs)

---

## 1. `BaseController.cs`
- **Location:** `Shared/Base/BaseController.cs`
- **Responsibility:** Serves as the abstract root controller for all API endpoints. Exposes user contextual properties (`CurrentUserId`, `CurrentUserName`, `CurrentUserRole`, `Lang`), helper methods for creating standardized `ServiceRequest<T>` containers, and smart `Ok(...)` overloads that evaluate `ServiceResult<T>` and localize errors automatically.
- **Problem Solved:** Eliminates boilerplate claim extraction, manual error checking (`if (!result.IsSuccess)`), and redundant HTTP status code mapping across controllers.
- **Trigger / Lifecycle:** Instantiated per HTTP request by ASP.NET Core MVC routing.
- **Dependencies:** `ICurrentUser`, `ServiceResult<T>`, `Response<T>`, `ErrorMessagesUtils`.
- **Usage Example:**
  ```csharp
  [HttpGet("{id}")]
  public async Task<ActionResult<StudentResponse>> Get(long id) =>
      Ok(await _service.Get(CreateServiceRequest(id)));
  ```

---

## 2. `GenericController.cs`
- **Location:** `Shared/Base/GenericController.cs`
- **Responsibility:** Provides standard implementations for typical CRUD actions (`Get`, `GetAll`, `Add`, `Update`, `Delete`) at the API presentation boundary.
- **Problem Solved:** Drastically reduces controller boilerplate for standard entities while preserving the ability to override or append custom endpoints.
- **Trigger / Lifecycle:** Inherited by entity-specific controllers (e.g., `StudentController`, `DepartmentController`).
- **Dependencies:** `BaseController`, `IBaseService<TView, TForm, TUpdate, TFilter>`.
- **Usage Example:**
  ```csharp
  [ApiController]
  [Route("api/[controller]")]
  public class StudentController(IStudentService service)
      : GenericController<StudentResponse, StudentForm, StudentUpdate, StudentFilter>(service)
  {
      [HttpGet]
      public async Task<ActionResult<Response<StudentResponse>>> GetAll([FromQuery] StudentFilter filter)
          => await BaseGetAll(filter);
  }
  ```

---

## 3. `IBaseService.cs`
- **Location:** `Shared/Base/IBaseService.cs`
- **Responsibility:** Declares the contract for standard business CRUD operations across domain features using generic type parameters.
- **Problem Solved:** Standardizes naming conventions, method signatures, and return contracts (`ServiceResult<T>`) across the entire service layer.
- **Trigger / Lifecycle:** Implemented by domain services and consumed by controllers and test harnesses.
- **Dependencies:** `ServiceResult<T>`, `ServiceRequest<T>`, `Response<T>`.
- **Design Feature:** Utilizes C# default interface methods to throw `NotSupportedException` for optional actions, avoiding forced stub implementations.

---

## 4. `CurrentUser.cs`
- **Location:** `Shared/Base/CurrentUser.cs`
- **Responsibility:** Scoped service implementing `ICurrentUser`, encapsulating extraction of claims (`UserId`, `UserName`, `FullName`, `Role`, `Lang`) from the active `HttpContext.User`.
- **Problem Solved:** Prevents direct coupling between domain services and the low-level `HttpContext`, supporting clean unit test mocking.
- **Trigger / Lifecycle:** Registered with scoped lifetime (`[Scoped]`); populated per HTTP request by `UserContextMiddleware`.
- **Dependencies:** `IHttpContextAccessor`, `ClaimsPrincipal`.

---

## 5. `BaseFilter.cs`
- **Location:** `Shared/Base/BaseFilter.cs`
- **Responsibility:** Provides core pagination properties (`PageNumber`, `PageSize`) and query state flags for all paged query filters.
- **Problem Solved:** Standardizes paged list requests across endpoints and SQL queries.
- **Usage Example:**
  ```csharp
  public class StudentFilter : BaseFilter
  {
      public string? FullName { get; set; }
      public int? Stage { get; set; }
  }
  ```

---

## 6. `Response.cs`
- **Location:** `Shared/Base/Response.cs`
- **Responsibility:** Standard envelope model for paginated API responses containing `Data` list, `PagesCount`, `CurrentPage`, `TotalCount`, and `IsLast`.
- **Problem Solved:** Provides consistent, predictable pagination contracts across all list endpoints.
- **Usage Example:**
  ```csharp
  var response = new Response<StudentResponse>(students, totalCount, filter.PageSize, filter.PageNumber);
  ```

---

## 7. `ServiceRequest.cs`
- **Location:** `Shared/Base/ServiceRequest.cs`
- **Responsibility:** Generic carrier envelope bundling the input payload (`Model`) with caller contextual metadata (`UserId`, `Lang`).
- **Problem Solved:** Ensures domain operations always receive authenticated user context without modifying method signatures whenever new metadata is introduced.

---

## 8. `ServiceResult.cs`
- **Location:** `Shared/Base/ServiceResult.cs`
- **Responsibility:** Implements the Result Pattern to return operational outcomes without throwing domain-level exceptions for expected failure conditions.
- **Problem Solved:** Prevents performance penalties associated with exception throwing for flow control and provides explicit, type-safe success/failure states.
- **Usage Example:**
  ```csharp
  if (isDuplicate)
      return ServiceResult<StudentResponse>.Failure(Messages.DuplicateStudentCode);

  return ServiceResult<StudentResponse>.Ok(student);
  ```

---

## 9. `BaseRepository.cs`
- **Location:** `Shared/Base/BaseRepository.cs`
- **Responsibility:** Abstract foundation for Dapper-based persistence providing execution abstractions for stored procedures, transactions, and duplicate verification.
- **Problem Solved:** Eliminates boilerplate connection management, command timeout handling, and Dapper parameter construction.
- **Key Methods:**
  - `ExecuteStoredProcedureAsync<T>`
  - `ExecuteStoredProcedureAsync`
  - `IsDuplicateAsync(tableName, columnName, value, excludeId)`

---

## 10. `IBaseRepository.cs`
- **Location:** `Shared/Base/IBaseRepository.cs`
- **Responsibility:** Generic interface declaring standard persistence operations (`Get`, `GetAll`, `Add`, `Update`, `Delete`) using Stored Procedures.
- **Problem Solved:** Defines a uniform database abstraction layer for all domain repositories.

---

## 11. `RepositoryWrapper.cs`
- **Location:** `Infrastructure/Persistence/RepositoryWrapper.cs`
- **Responsibility:** Coordinates access to all domain repositories under a single injectable wrapper and manages transaction scopes.
- **Problem Solved:** Prevents constructor pollution across services requiring multiple repositories and allows multi-repository atomic transactions.
- **Usage Example:**
  ```csharp
  await wrapper.BeginTransactionAsync();
  // perform coordinated repository operations
  await wrapper.CommitTransactionAsync();
  ```

---

## 12. `DapperContext.cs`
- **Location:** `Infrastructure/Persistence/DapperContext.cs`
- **Responsibility:** Manages the lifecycle and instantiation of `SqlConnection` instances against the configured connection string.
- **Problem Solved:** Centralizes connection string retrieval and database connection creation across all repositories.

---

## 13. `PipelineExtension.cs`
- **Location:** `Infrastructure/Extensions/PipelineExtension.cs`
- **Responsibility:** Configures the ASP.NET Core HTTP request processing pipeline in strict chronological order:
  1. Exception Handling
  2. Routing
  3. CORS
  4. Authentication
  5. `UserContextMiddleware`
  6. Authorization
  7. FastEndpoints / Controllers
  8. OpenAPI / Scalar Documentation

---

## 14. `ApplicationSecurityExtension.cs`
- **Location:** `Infrastructure/Extensions/ApplicationSecurityExtension.cs`
- **Responsibility:** Registers security infrastructure services, including JWT Bearer authentication, token validation parameters, and authorization policies.
- **Configuration:** Enforces signing key validation, issuer/audience verification, and lifetime checks.

---

## 15. `ApplicationServicesExtension.cs`
- **Location:** `Infrastructure/Extensions/ApplicationServicesExtension.cs`
- **Responsibility:** Automatically scans assemblies and registers application services, repositories, validators, and context providers matching DI lifecycle attributes (`[Scoped]`, `[Transient]`, `[Singleton]`).
- **Problem Solved:** Eliminates manual service registration in `Program.cs`.

---

## 16. `ControllersExtension.cs`
- **Location:** `Infrastructure/Extensions/ControllersExtension.cs`
- **Responsibility:** Configures ASP.NET Core controllers, JSON serialization options, custom model binders (`SqidModelBinderProvider`), and automatic FluentValidation filters.

---

## 17. `DiAttributes.cs`
- **Location:** `Shared/Attributes/DiAttributes.cs`
- **Responsibility:** Provides declarative marker attributes (`[Scoped]`, `[Transient]`, `[Singleton]`) placed on service and repository classes for automatic dependency injection discovery.
- **Usage Example:**
  ```csharp
  [Scoped]
  public class StudentService(IRepositoryWrapper wrapper) : IStudentService { ... }
  ```

---

## 18. `SqidAttribute.cs` & `SqidCodec.cs`
- **Location:** `Shared/Attributes/SqidAttribute.cs`, `Shared/Helpers/SqidCodec.cs`
- **Responsibility:** Enables ID obfuscation using YouTube-style alphanumeric Sqids (Hashids). Prevents primary key enumeration attacks in URLs while preserving clean internal 64-bit integer IDs in the database.
- **Usage Example:**
  - Route: `/api/student/UkLWZg9D` -> Decoded by `SqidModelBinder` to numeric ID `1`.
  - JSON Serializer: Encodes `long` properties marked with `[Sqid]` into string tokens.

---

## 19. `IgnoreParameterAttribute.cs`
- **Location:** `Shared/Attributes/IgnoreParameterAttribute.cs`
- **Responsibility:** Prevents specific DTO properties from being forwarded as dynamic parameters to underlying stored procedures.

---

## 20. `ErrorMessagesUtils.cs`
- **Location:** `Shared/Helpers/ErrorMessagesUtils.cs`
- **Responsibility:** Centralized utility translating application error enum values into localized, user-friendly error messages (supporting both English and Arabic).

---

## 21. `PasswordHasher.cs`
- **Location:** `Shared/Helpers/PasswordHasher.cs`
- **Responsibility:** Cryptographic helper utilizing the industry-standard BCrypt algorithm with salted iterations for password hashing and verification.
- **Problem Solved:** Guarantees passwords are never stored in plaintext and protects against precomputed rainbow table attacks.
- **Usage Example:**
  ```csharp
  string hash = PasswordHasher.Hash("SecureP@ss123");
  bool isValid = PasswordHasher.Verify("SecureP@ss123", hash);
  ```
