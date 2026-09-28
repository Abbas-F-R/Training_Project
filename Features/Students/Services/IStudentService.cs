using OC_System_Training.Features.Students.Dtos;

namespace OC_System_Training.Features.Students.Services;

// تعليق تدريبي: واجهة خدمة الطلاب (IStudentService)
public interface IStudentService : IBaseService<StudentResponse, StudentForm, StudentUpdate, StudentFilter>
{
}
