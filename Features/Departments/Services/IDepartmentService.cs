using Training_Project.Features.Departments.Dtos;
using Training_Project.Shared.Base;
using Training_Project.Shared.Base.dto;

namespace Training_Project.Features.Departments.Services;

/// <summary>
/// Service contract for department business operations and lookup functionality.
/// </summary>
public interface IDepartmentService : IBaseService<DepartmentResponse, DepartmentForm, DepartmentUpdate, DepartmentFilter>
{
    Task<ServiceResult<List<DepartmentResponse>>> Lookup();
}
