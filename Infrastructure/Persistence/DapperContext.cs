using System.Data;
using Microsoft.Data.SqlClient;

namespace OC_System_Training.Infrastructure.Persistence;

// تعليق تدريبي: مدير اتصالات Dapper (DapperContext)
// مسؤول عن إنشاء اتصالات IDbConnection بقاعدة بيانات SQL Server
// يقرأ نص الاتصال من appsettings.json تحت قسم ConnectionStrings:DefaultConnection
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
    /// إنشاء اتصال جديد بقاعدة البيانات
    /// </summary>
    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
