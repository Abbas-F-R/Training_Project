using System.Data;
using Dapper;
using Training_Project.Features.Departments.Dtos;
using Training_Project.Infrastructure.Persistence;
using Training_Project.Infrastructure.Persistence.Repositories.BaseRepository;
using Training_Project.Shared.Attributes;
using Training_Project.Shared.Constants;

namespace Training_Project.Features.Departments.Repositories;

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
