using Training_Project.Features.Students.Dtos;

namespace Training_Project.Features.Students.Services;

/// <summary>
/// Service contract for student business operations.
/// </summary>
public interface IStudentService : IBaseService<StudentResponse, StudentForm, StudentUpdate, StudentFilter>
{
}
