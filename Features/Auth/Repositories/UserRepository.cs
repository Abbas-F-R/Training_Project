using System.Data;
using Dapper;
using OC_System_Training.Features.Auth.Dtos;
using OC_System_Training.Infrastructure.Persistence;
using OC_System_Training.Shared.Attributes;

namespace OC_System_Training.Features.Auth.Repositories;

/// <summary>
/// User repository implementation using Dapper and Stored Procedures.
/// </summary>
[Scoped]
public class UserRepository(DapperContext context) : IUserRepository
{
    public async Task<UserDto?> GetByUserName(string userName)
    {
        using var connection = context.CreateConnection();
        return (await connection.QueryAsync<UserDto>(
            "UsersGetByUserName",
            new { UserName = userName },
            commandType: CommandType.StoredProcedure
        )).FirstOrDefault();
    }

    public async Task<UserDto?> GetById(long id)
    {
        using var connection = context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<UserDto>(
            "SELECT * FROM Users WHERE Id = @Id AND IsDeleted = 0",
            new { Id = id }
        );
    }

    public async Task<UserDto?> Add(UserDto user, long? createdBy = null)
    {
        using var connection = context.CreateConnection();
        return (await connection.QueryAsync<UserDto>(
            "UsersInsert",
            new
            {
                FullName = user.FullName,
                UserName = user.UserName,
                PasswordHash = user.PasswordHash,
                Role = user.Role,
                CreatedBy = createdBy
            },
            commandType: CommandType.StoredProcedure
        )).FirstOrDefault();
    }

    public async Task<bool> IsUserNameTaken(string userName)
    {
        using var connection = context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Users WHERE UserName = @UserName AND IsDeleted = 0",
            new { UserName = userName }
        );
        return count > 0;
    }
}
