using OC_System_Training.Features.Students.Dtos;
using OC_System_Training.Infrastructure.Persistence.Repositories.BaseRepository;

namespace OC_System_Training.Features.Students.Repositories;

// تعليق تدريبي: واجهة الـ Repository الخاصة بالطلاب
public interface IStudentRepository : IBaseRepository<StudentResponse, StudentForm, StudentUpdate, StudentFilter>
{
}
