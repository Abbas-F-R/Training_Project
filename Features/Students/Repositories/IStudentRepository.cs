using Training_Project.Features.Students.Dtos;
using Training_Project.Infrastructure.Persistence.Repositories.BaseRepository;

namespace Training_Project.Features.Students.Repositories;

/// <summary>
/// Data-access repository contract for student entity operations.
/// </summary>
public interface IStudentRepository : IBaseRepository<StudentResponse, StudentForm, StudentUpdate, StudentFilter>
{
}
