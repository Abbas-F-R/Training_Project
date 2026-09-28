using Dapper;

namespace OC_System_Training.Infrastructure.Persistence;

// تعليق تدريبي: مهيئ البيانات الأولية (DatabaseSeeder)
// يضمن عند تشغيل المشروع وجود حسابات المشرف Admin والمستخدم العادي User بكلمات مرور مشفرة بشكل سليم
public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DapperContext>();
        using var connection = context.CreateConnection();

        // 1. حساب المشرف (Admin)
        var adminExists = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Users WHERE UserName = 'admin' AND IsDeleted = 0"
        );

        if (adminExists == 0)
        {
            var adminHash = PasswordHasher.Hash("Admin@12345");
            await connection.ExecuteAsync(
                @"INSERT INTO Users (FullName, UserName, PasswordHash, Role, IsActive, IsDeleted, CreatedAt)
                  VALUES (N'مدير النظام التدريبي', 'admin', @Hash, 'Admin', 1, 0, GETDATE())",
                new { Hash = adminHash }
            );
        }

        // 2. حساب المستخدم العادي (User / Clerk) لاختبار الصلاحيات ورمز 403 Forbidden
        var userExists = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Users WHERE UserName = 'user' AND IsDeleted = 0"
        );

        if (userExists == 0)
        {
            var userHash = PasswordHasher.Hash("User@12345");
            await connection.ExecuteAsync(
                @"INSERT INTO Users (FullName, UserName, PasswordHash, Role, IsActive, IsDeleted, CreatedAt)
                  VALUES (N'المستخدم التجريبي (موظف تسجيل)', 'user', @Hash, 'User', 1, 0, GETDATE())",
                new { Hash = userHash }
            );
        }
    }
}
