using OC_System_Training.Features.Departments.Dtos;
using OC_System_Training.Shared.Base;
using OC_System_Training.Shared.Base.dto;

namespace OC_System_Training.Features.Departments.Services;

/// <summary>
/// Service contract for department business operations and lookup functionality.
/// </summary>
public interface IDepartmentService : IBaseService<DepartmentResponse, DepartmentForm, DepartmentUpdate, DepartmentFilter>
{
    Task<ServiceResult<List<DepartmentResponse>>> Lookup();
}
