using Training_Project.Features.AuditLogs.Repositories;
using Training_Project.Features.Auth.Repositories;
using Training_Project.Features.Departments.Repositories;
using Training_Project.Features.Students.Repositories;

namespace Training_Project.Infrastructure.Persistence.Repositories.RepositoryWrapper;

/// <summary>
/// Aggregates all domain repositories to provide unified transactional and cross-feature data access.
/// </summary>
public interface IRepositoryWrapper
{
    IDepartmentRepository Department { get; }
    IStudentRepository Student { get; }
    IUserRepository User { get; }
    IAuditLogRepository AuditLog { get; }
}
