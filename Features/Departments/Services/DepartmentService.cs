using OC_System_Training.Features.Departments.Dtos;
using OC_System_Training.Features.Departments.Repositories;
using OC_System_Training.Shared.Attributes;
using OC_System_Training.Shared.Base.dto;
using OC_System_Training.Shared.Constants;

namespace OC_System_Training.Features.Departments.Services;

/// <summary>
/// Department service managing department business operations, code uniqueness validation, and lookup retrieval.
/// </summary>
[Scoped]
public class DepartmentService(IDepartmentRepository repository) : IDepartmentService
{
    public async Task<ServiceResult<DepartmentResponse>> Get(ServiceRequest<long> request)
    {
        var item = await repository.Get(request.Dto);
        return item != null
            ? ServiceResult<DepartmentResponse>.Ok(item)
            : ServiceResult<DepartmentResponse>.Failure(Messages.RecordNotFound);
    }

    public async Task<ServiceResult<List<DepartmentResponse>>> GetAll(ServiceRequest<DepartmentFilter> request)
    {
        var (data, totalCount) = await repository.GetAll(request.Dto);
        return ServiceResult<List<DepartmentResponse>>.PagedOk(data, totalCount);
    }

    public async Task<ServiceResult<List<DepartmentResponse>>> Lookup()
    {
        var data = await repository.Lookup();
        return ServiceResult<List<DepartmentResponse>>.Ok(data);
    }

    public async Task<ServiceResult<DepartmentResponse>> Add(ServiceRequest<DepartmentForm> request)
    {
        // Enforce uniqueness of department code
        if (await repository.IsDuplicateAsync("Code", request.Dto.Code))
            return ServiceResult<DepartmentResponse>.Failure(Messages.DuplicateDepartmentCode);

        var created = await repository.Add(request.Dto, request.UserId);
        return created != null
            ? ServiceResult<DepartmentResponse>.Ok(created)
            : ServiceResult<DepartmentResponse>.Failure(Messages.InsertFailed);
    }

    public async Task<ServiceResult<DepartmentResponse>> Update(long id, ServiceRequest<DepartmentUpdate> request)
    {
        // Enforce uniqueness of department code excluding current record
        if (await repository.IsDuplicateAsync("Code", request.Dto.Code, excludeId: id))
            return ServiceResult<DepartmentResponse>.Failure(Messages.DuplicateDepartmentCode);

        var updated = await repository.Update(id, request.Dto, request.UserId);
        return updated != null
            ? ServiceResult<DepartmentResponse>.Ok(updated)
            : ServiceResult<DepartmentResponse>.Failure(Messages.UpdateFailed);
    }

    public async Task<ServiceResult<bool>> Delete(ServiceRequest<long> request)
    {
        var success = await repository.Delete(request.Dto, request.UserId);
        return success
            ? ServiceResult<bool>.Ok(true)
            : ServiceResult<bool>.Failure(Messages.DeleteFailed);
    }
}
