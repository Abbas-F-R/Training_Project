using OC_System_Training.Features.Students.Dtos;
using OC_System_Training.Infrastructure.Persistence.Repositories.BaseRepository;
using OC_System_Training.Shared.Attributes;

namespace OC_System_Training.Features.Students.Repositories;

/// <summary>
/// Student repository implementation delegating to base stored procedure operations.
/// </summary>
[Scoped]
public class StudentRepository(DapperContext context)
    : BaseRepository<StudentResponse, StudentForm, StudentUpdate, StudentFilter>(context, DbConstants.Tables.Students),
      IStudentRepository
{
}
