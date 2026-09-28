using Training_Project.Features.AuditLogs.Repositories;
using Training_Project.Features.Auth.Repositories;
using Training_Project.Features.Departments.Repositories;
using Training_Project.Features.Students.Repositories;
using Training_Project.Shared.Attributes;

namespace Training_Project.Infrastructure.Persistence.Repositories.RepositoryWrapper;

/// <summary>
/// Implements repository wrapper pattern to coordinate repositories across features.
/// </summary>
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
