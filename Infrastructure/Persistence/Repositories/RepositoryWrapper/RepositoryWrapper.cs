using OC_System_Training.Features.AuditLogs.Repositories;
using OC_System_Training.Features.Auth.Repositories;
using OC_System_Training.Features.Departments.Repositories;
using OC_System_Training.Features.Students.Repositories;
using OC_System_Training.Shared.Attributes;

namespace OC_System_Training.Infrastructure.Persistence.Repositories.RepositoryWrapper;

// تعليق تدريبي: تطبيق الـ RepositoryWrapper
// يقوم بحقن وتوفير الـ Repositories المتاحة في النظام لخدمة الـ Services
[Scoped]
public class RepositoryWrapper(
    IDepartmentRepository department,
    IStudentRepository student,
    IUserRepository user,
    IAuditLogRepository auditLog) : IRepositoryWrapper
{
    public IDepartmentRepository Department => department;
    public IStudentRepository Student => student;
    public IUserRepository User => user;
    public IAuditLogRepository AuditLog => auditLog;
}
