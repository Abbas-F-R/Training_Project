using System.Data;
using Microsoft.Data.SqlClient;

namespace OC_System_Training.Infrastructure.Persistence;

/// <summary>
/// Manages SQL Server database connections using Dapper.
/// Resolves connection string from configuration or environment variables.
/// </summary>
public class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
                            ?? "Server=localhost;Database=OC_System_Training_DB;Trusted_Connection=True;TrustServerCertificate=True;";
    }

    /// <summary>
    /// Creates and returns an active SQL Server database connection.
    /// </summary>
    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
