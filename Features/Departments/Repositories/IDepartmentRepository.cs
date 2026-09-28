using Training_Project.Features.Departments.Dtos;
using Training_Project.Infrastructure.Persistence.Repositories.BaseRepository;

namespace Training_Project.Features.Departments.Repositories;

/// <summary>
/// Data-access repository contract for department entities and dropdown lookup.
/// </summary>
public interface IDepartmentRepository : IBaseRepository<DepartmentResponse, DepartmentForm, DepartmentUpdate, DepartmentFilter>
{
    Task<List<DepartmentResponse>> Lookup();
}
