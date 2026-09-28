using OC_System_Training.Features.Students.Dtos;
using OC_System_Training.Infrastructure.Persistence.Repositories.BaseRepository;

namespace OC_System_Training.Features.Students.Repositories;

/// <summary>
/// Data-access repository contract for student entity operations.
/// </summary>
public interface IStudentRepository : IBaseRepository<StudentResponse, StudentForm, StudentUpdate, StudentFilter>
{
}
