using OC_System_Training.Features.AuditLogs.Repositories;
using OC_System_Training.Features.Auth.Repositories;
using OC_System_Training.Features.Departments.Repositories;
using OC_System_Training.Features.Students.Repositories;

namespace OC_System_Training.Infrastructure.Persistence.Repositories.RepositoryWrapper;

// تعليق تدريبي: غلاف الـ Repositories الموحد (IRepositoryWrapper)
// يجمع كافة الـ Repositories في مكان واحد لمنح الـ Services وصولاً مركزياً لها
public interface IRepositoryWrapper
{
    IDepartmentRepository Department { get; }
    IStudentRepository Student { get; }
    IUserRepository User { get; }
    IAuditLogRepository AuditLog { get; }
}
