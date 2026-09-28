using Training_Project.Features.Students.Dtos;
using Training_Project.Infrastructure.Persistence.Repositories.BaseRepository;
using Training_Project.Shared.Attributes;

namespace Training_Project.Features.Students.Repositories;

/// <summary>
/// Student repository implementation delegating to base stored procedure operations.
/// </summary>
[Scoped]
public class StudentRepository(DapperContext context)
    : BaseRepository<StudentResponse, StudentForm, StudentUpdate, StudentFilter>(context, DbConstants.Tables.Students),
      IStudentRepository
{
}
