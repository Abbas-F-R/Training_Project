using OC_System_Training.Features.Students.Dtos;

namespace OC_System_Training.Features.Students.Services;

/// <summary>
/// Service contract for student business operations.
/// </summary>
public interface IStudentService : IBaseService<StudentResponse, StudentForm, StudentUpdate, StudentFilter>
{
}
