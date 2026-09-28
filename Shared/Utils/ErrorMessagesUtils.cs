namespace OC_System_Training.Shared.Utils;

// تعليق تدريبي: قاموس ترجمة رسائل النظام المركزية إلى العربية والإنجليزية
public static class ErrorMessagesUtils
{
    private static readonly Dictionary<string, Dictionary<string, string>> MessagesDict = new()
    {
        [Messages.RecordNotFound] = new()
        {
            ["ar"] = "السجل المطلوب غير موجود.",
            ["en"] = "The requested record was not found."
        },
        [Messages.InsertFailed] = new()
        {
            ["ar"] = "فشلت عملية الإضافة، يرجى المحاولة مرة أخرى.",
            ["en"] = "Failed to insert record, please try again."
        },
        [Messages.UpdateFailed] = new()
        {
            ["ar"] = "فشلت عملية التعديل، يرجى التأكد من صحة البيانات.",
            ["en"] = "Failed to update record, please verify input data."
        },
        [Messages.DeleteFailed] = new()
        {
            ["ar"] = "فشلت عملية الحذف.",
            ["en"] = "Failed to delete record."
        },
        [Messages.DuplicateRecord] = new()
        {
            ["ar"] = "هذا السجل موجود مسبقاً في النظام.",
            ["en"] = "This record already exists in the system."
        },
        [Messages.DuplicateStudentCode] = new()
        {
            ["ar"] = "الرقم الجامعي للطالب مسجل مسبقاً لطالب آخر.",
            ["en"] = "Student code already belongs to another student."
        },
        [Messages.DuplicateDepartmentCode] = new()
        {
            ["ar"] = "رمز القسم مسجل مسبقاً لقسم آخر.",
            ["en"] = "Department code already exists."
        },
        [Messages.InvalidCredentials] = new()
        {
            ["ar"] = "اسم المستخدم أو كلمة المرور غير صحيحة.",
            ["en"] = "Invalid username or password."
        },
        [Messages.UserNotFound] = new()
        {
            ["ar"] = "المستخدم غير موجود.",
            ["en"] = "User was not found."
        },
        [Messages.UserInactive] = new()
        {
            ["ar"] = "حساب المستخدم معطل حالياً.",
            ["en"] = "User account is currently disabled."
        },
        [Messages.Unauthorized] = new()
        {
            ["ar"] = "ليس لديك صلاحية لتنفيذ هذا الإجراء.",
            ["en"] = "You are not authorized to perform this action."
        },
        [Messages.DepartmentNotFound] = new()
        {
            ["ar"] = "القسم الدراسي المحدد غير موجود.",
            ["en"] = "The specified department was not found."
        }
    };

    /// <summary>
    /// تحويل مفتاح الرسالة إلى نص مقروء باللغة المطلوبة
    /// </summary>
    public static string GetMessage(this string key, string? lang = "ar")
    {
        var targetLang = string.IsNullOrWhiteSpace(lang) ? "ar" : lang.Trim().ToLowerInvariant();
        if (targetLang != "ar" && targetLang != "en") targetLang = "ar";

        if (MessagesDict.TryGetValue(key, out var translations))
        {
            if (translations.TryGetValue(targetLang, out var message))
                return message;
            if (translations.TryGetValue("ar", out var fallback))
                return fallback;
        }

        return key; // إرجاع المفتاح نفسه إذا لم تتوفر ترجمة
    }
}
