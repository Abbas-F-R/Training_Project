# Dependency Injection & Cross-Cutting Features

## 1. Auto-DI via Attributes (Scrutor)
OC_System relies on **Scrutor** assembly scanning. Instead of manually registering every service and repository in `Program.cs`, decorate classes with lifetime attributes:
- `[Scoped]` (Most common: Services, Repositories, RepositoryWrapper)
- `[Transient]` (Lightweight helpers)
- `[Singleton]` (Stateless utilities, `DapperContext`, options)

```csharp
[Scoped]
public class StudentService(IStudentRepository repository) : IStudentService { }

[Scoped]
public class StudentRepository(DapperContext context) : ..., IStudentRepository { }
```

## 2. CurrentUser Injection (`ICurrentUser`)
Instead of extracting JWT claims manually across controllers or passing `HttpContext` to services, inject `ICurrentUser`:
```csharp
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    long? UserId { get; }
    string UserName { get; }
    string FullName { get; }
    string Role { get; }
}
```
`BaseController` also automatically populates `ServiceRequest<T>` using `CurrentUser`.

## 3. Swagger & Scalar API Reference
Modern OC_System exposes two interactive API exploration tools:
1. **Swagger UI:** Accessible at `/swagger`
2. **Scalar API Reference:** Accessible at `/scalar/v1` (modern interactive API client)

Both interfaces support JWT authentication: click `Authorize`, type `Bearer YOUR_TOKEN`, and execute endpoints directly from the browser.
