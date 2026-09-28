using OC_System_Training.Features.Departments.Dtos;
using OC_System_Training.Infrastructure.Persistence.Repositories.BaseRepository;

namespace OC_System_Training.Features.Departments.Repositories;

/// <summary>
/// Data-access repository contract for department entities and dropdown lookup.
/// </summary>
public interface IDepartmentRepository : IBaseRepository<DepartmentResponse, DepartmentForm, DepartmentUpdate, DepartmentFilter>
{
    Task<List<DepartmentResponse>> Lookup();
}
