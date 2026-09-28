# Repository Patterns for OC_System

All database operations in OC_System use **Dapper** executing **Stored Procedures**. No raw SQL queries are written in C# repositories.

## 1. Repository Interface
Located inside the feature's `Repositories/` directory:
```csharp
public interface IStudentRepository : IBaseRepository<StudentResponse, StudentForm, StudentUpdate, StudentFilter>
{
    // Feature-specific custom queries if needed
}
```

## 2. Repository Implementation
Inherits `BaseRepository` to gain automatic implementation of all standard CRUD operations:
```csharp
[Scoped]
public class StudentRepository(DapperContext context)
    : BaseRepository<StudentResponse, StudentForm, StudentUpdate, StudentFilter>(context, DbConstants.Tables.Students),
      IStudentRepository
{
    // BaseRepository implements Get, GetAll, GetAllNotPaged, Add, Update, Delete, IsDuplicateAsync
}
```

## 3. BaseRepository Full Contract

```csharp
// Lookup helpers
Task<TView?> GetFirstAsync(string columnName, object value, string? viewName = null);
Task<TView?> GetFirstAsync(Expression<Func<TView, bool>> predicate, string? viewName = null, IDbTransaction? transaction = null);

// Read
Task<TView?> Get(long id, string? procedureName = null);
Task<TResult?> Get<TResult>(long id, string? procedureName = null);
Task<(List<TView>? data, int totalCount)> GetAll(TFilter filter, string? procedureName = null);
Task<List<TView>?> GetAllNotPaged(TFilter filter, string? procedureName = null);
Task<List<TView>?> GetAllNotPaged(string? procedureName = null);

// Write (Wrapped in IDbTransaction with automatic rollback on error)
Task<TView?> Add(TForm form, long userId, string? procedureName = null);
Task<IEnumerable<TView>> BulkAdd(IEnumerable<TForm> forms, long userId, string? procedureName = null);
Task<TView?> Update(long id, TUpdate update, long userId, string? procedureName = null);
Task<TView?> Upsert(object dto, long userId, string? procedureName = null);
Task<bool> Delete(long id, long userId, string? procedureName = null);
Task<bool> Delete(object parameters, string? procedureName = null);

// Validation
Task<bool> IsDuplicateAsync(string columnName, object value, long? excludeId = null);
Task<bool> IsDuplicateAsync<T>(Expression<Func<T, bool>> predicate, string viewName);
```

## 4. Key Architectural Behaviors in BaseRepository

1. **Delete returns `Task<bool>`:** Soft delete sets `IsDeleted = 1` and returns `true` on success.
2. **Audit Tracking (`SetAuditUserAsync`):** Sets `sys.sp_set_session_context` for `UserId`, and passes `@CreatedBy` / `@UpdatedBy` to procedures.
3. **Transaction Safety (`RollbackQuietly`):** Catches procedure-level rollbacks gracefully to preserve the original `SqlException` error message without masking it with `InvalidOperationException`.
4. **Foreign Key Enforcement:** Real foreign keys are defined in SQL Server (`CONSTRAINT FK_...`). Invalid foreign keys throw SQL Error 547 (Foreign Key Violation), cleanly captured and mapped by the API exception handler.
5. **`[IgnoreParameter]` Attribute:** Applied to DTO properties (such as calculated fields or navigation properties) that should not be passed to Dapper's `DynamicParameters`.
6. **`GetAll` Two-Set Contract:** `connection.QueryMultipleAsync` first reads `int` (TotalCount), then reads `List<TView>` (Data).

## 5. Repository Wrapper (`IRepositoryWrapper`)
For services coordinating multiple repositories, `RepositoryWrapper` exposes lazily-initialized repositories or resolves them from the DI container:

```csharp
public interface IRepositoryWrapper
{
    IStudentRepository Student { get; }
    IDepartmentRepository Department { get; }
}
```
For single-repository features, direct injection (`IStudentRepository`) into the service is preferred for simplicity and clarity.
