namespace OC_System_Training.Shared.Utils;

// تعليق تدريبي: أداة تشفير وفحص كلمات المرور باستخدام خوارزمية BCrypt الآمنة
public static class PasswordHasher
{
    /// <summary>
    /// تشفير كلمة المرور بنظام BCrypt مع إضافة Salt عشوائي تلقائياً
    /// </summary>
    public static string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
    }

    /// <summary>
    /// مطابقة كلمة المرور المدخلة مع الـ Hash المخزن في قاعدة البيانات
    /// </summary>
    public static bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
