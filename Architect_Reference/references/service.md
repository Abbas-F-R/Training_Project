# Service Patterns for OC_System

Services encapsulate business logic, domain validation, and data orchestration.

## 1. Service Interface
Located inside the feature's `Services/` directory:
```csharp
public interface IStudentService : IBaseService<StudentResponse, StudentForm, StudentUpdate, StudentFilter>
{
    // Feature-specific business methods if needed
}
```

## 2. Standard Service Results & Requests
- `ServiceRequest<T>`: Wraps the input DTO with ambient actor data:
  - `Dto`: The payload.
  - `UserId`: ID of the calling user.
  - `UserName`: Username.
  - `Role`: Security role.
  - `Lang`: Preferred language (`ar` / `en`).
- `ServiceResult<T>`: Unifies service outputs:
  - `ServiceResult<T>.Ok(data)`: For single entity success.
  - `ServiceResult<List<T>>.PagedOk(data, totalCount)`: For paginated list success (carries `TotalCount` to `Response<T>`).
  - `ServiceResult<T>.Failure(Messages.ErrorKey)`: Returns failure using an i18n message key.

## 3. Implementation Example
```csharp
[Scoped]
public class StudentService(IStudentRepository repository) : IStudentService
{
    public async Task<ServiceResult<StudentResponse>> Get(ServiceRequest<long> request)
    {
        var item = await repository.Get(request.Dto);
        return item != null
            ? ServiceResult<StudentResponse>.Ok(item)
            : ServiceResult<StudentResponse>.Failure(Messages.RecordNotFound);
    }

    public async Task<ServiceResult<List<StudentResponse>>> GetAll(ServiceRequest<StudentFilter> request)
    {
        var (data, totalCount) = await repository.GetAll(request.Dto);
        return ServiceResult<List<StudentResponse>>.PagedOk(data, totalCount);
    }

    public async Task<ServiceResult<List<StudentResponse>>> GetAllNotPaged(ServiceRequest<StudentFilter> request)
    {
        var data = await repository.GetAllNotPaged(request.Dto);
        return ServiceResult<List<StudentResponse>>.Ok(data);
    }

    public async Task<ServiceResult<StudentResponse>> Add(ServiceRequest<StudentForm> request)
    {
        // Business Rule: Check for duplicate student code or email
        if (await repository.IsDuplicateAsync("StudentCode", request.Dto.StudentCode))
            return ServiceResult<StudentResponse>.Failure(Messages.DuplicateRecord);

        var created = await repository.Add(request.Dto, request.UserId);
        return created != null
            ? ServiceResult<StudentResponse>.Ok(created)
            : ServiceResult<StudentResponse>.Failure(Messages.InsertFailed);
    }

    public async Task<ServiceResult<StudentResponse>> Update(long id, ServiceRequest<StudentUpdate> request)
    {
        if (await repository.IsDuplicateAsync("StudentCode", request.Dto.StudentCode, excludeId: id))
            return ServiceResult<StudentResponse>.Failure(Messages.DuplicateRecord);

        var updated = await repository.Update(id, request.Dto, request.UserId);
        return updated != null
            ? ServiceResult<StudentResponse>.Ok(updated)
            : ServiceResult<StudentResponse>.Failure(Messages.UpdateFailed);
    }

    public async Task<ServiceResult<bool>> Delete(ServiceRequest<long> request)
    {
        var deleted = await repository.Delete(request.Dto, request.UserId);
        return deleted
            ? ServiceResult<bool>.Ok(true)
            : ServiceResult<bool>.Failure(Messages.DeleteFailed);
    }
}
```

## 4. Key Rules
- Always use `PagedOk(data, totalCount)` for paged lists.
- Return message keys from `Messages` constant class rather than hardcoded error strings.
- Pass `request.UserId` to repository write operations for audit tracking.
