using Dapper;

namespace OC_System_Training.Infrastructure.Persistence;

/// <summary>
/// Database seeder to ensure initial administrator and standard staff users exist upon startup.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DapperContext>();
        using var connection = context.CreateConnection();

        // 1. Seed default Administrator user
        var adminExists = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Users WHERE UserName = 'admin' AND IsDeleted = 0"
        );

        if (adminExists == 0)
        {
            var adminHash = PasswordHasher.Hash("Admin@12345");
            await connection.ExecuteAsync(
                @"INSERT INTO Users (FullName, UserName, PasswordHash, Role, IsActive, IsDeleted, CreatedAt)
                  VALUES (N'System Administrator', 'admin', @Hash, 'Admin', 1, 0, GETDATE())",
                new { Hash = adminHash }
            );
        }

        // 2. Seed default Standard user (to test RBAC and 403 Forbidden policies)
        var userExists = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Users WHERE UserName = 'user' AND IsDeleted = 0"
        );

        if (userExists == 0)
        {
            var userHash = PasswordHasher.Hash("User@12345");
            await connection.ExecuteAsync(
                @"INSERT INTO Users (FullName, UserName, PasswordHash, Role, IsActive, IsDeleted, CreatedAt)
                  VALUES (N'Academic Registrar User', 'user', @Hash, 'User', 1, 0, GETDATE())",
                new { Hash = userHash }
            );
        }
    }
}
