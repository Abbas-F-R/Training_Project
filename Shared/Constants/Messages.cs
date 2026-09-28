namespace OC_System_Training.Shared.Constants;

// تعليق تدريبي: مفاتيح رسائل الخطأ والنجاح الموحدة.
// تُرجع الـ Services هذه المفاتيح، بينما يتولى الـ Controller ترجمتها للغة المطلوبة.
public static class Messages
{
    public const string RecordNotFound = "RecordNotFound";
    public const string InsertFailed = "InsertFailed";
    public const string UpdateFailed = "UpdateFailed";
    public const string DeleteFailed = "DeleteFailed";
    public const string DuplicateRecord = "DuplicateRecord";
    public const string DuplicateStudentCode = "DuplicateStudentCode";
    public const string DuplicateDepartmentCode = "DuplicateDepartmentCode";
    public const string InvalidCredentials = "InvalidCredentials";
    public const string UserNotFound = "UserNotFound";
    public const string UserInactive = "UserInactive";
    public const string Unauthorized = "Unauthorized";
    public const string DepartmentNotFound = "DepartmentNotFound";
}
