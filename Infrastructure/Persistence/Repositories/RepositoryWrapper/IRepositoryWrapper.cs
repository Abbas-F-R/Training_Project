using OC_System_Training.Features.AuditLogs.Repositories;
using OC_System_Training.Features.Auth.Repositories;
using OC_System_Training.Features.Departments.Repositories;
using OC_System_Training.Features.Students.Repositories;

namespace OC_System_Training.Infrastructure.Persistence.Repositories.RepositoryWrapper;

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
