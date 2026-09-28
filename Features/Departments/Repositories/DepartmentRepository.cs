using System.Data;
using Dapper;
using OC_System_Training.Features.Departments.Dtos;
using OC_System_Training.Infrastructure.Persistence;
using OC_System_Training.Infrastructure.Persistence.Repositories.BaseRepository;
using OC_System_Training.Shared.Attributes;
using OC_System_Training.Shared.Constants;

namespace OC_System_Training.Features.Departments.Repositories;

/// <summary>
/// Department repository implementing base CRUD and custom lookup queries using Dapper.
/// </summary>
[Scoped]
public class DepartmentRepository(DapperContext context)
    : BaseRepository<DepartmentResponse, DepartmentForm, DepartmentUpdate, DepartmentFilter>(context, DbConstants.Tables.Departments),
      IDepartmentRepository
{
    public async Task<List<DepartmentResponse>> Lookup()
    {
        using var connection = Context.CreateConnection();
        return (await connection.QueryAsync<DepartmentResponse>(
            "DepartmentsLookup",
            commandType: CommandType.StoredProcedure
        )).ToList();
    }
}
