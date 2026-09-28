using Training_Project.Features.AuditLogs.Dtos;

namespace Training_Project.Features.AuditLogs.Utils;

/// <summary>
/// Central utility for localizing audit log action names, entity names, and human-readable event descriptions.
/// Supports bilingual translations for Arabic (ar) and English (en).
/// </summary>
public static class AuditLogLocalizer
{
    private static readonly Dictionary<string, (string Ar, string En)> ActionTranslations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["INSERT"] = ("إضافة", "Insert"),
        ["UPDATE"] = ("تعديل", "Update"),
        ["DELETE"] = ("حذف", "Delete"),
        ["LOGIN_SUCCESS"] = ("تسجيل دخول ناجح", "Successful Login"),
        ["LOGIN_FAILED"] = ("فشل تسجيل الدخول", "Failed Login"),
        ["LOGOUT"] = ("تسجيل خروج", "Logout")
    };

    private static readonly Dictionary<string, (string Ar, string En)> EntityTranslations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Students"] = ("الطلاب", "Students"),
        ["Student"] = ("طالب", "Student"),
        ["Departments"] = ("الأقسام", "Departments"),
        ["Department"] = ("قسم", "Department"),
        ["Users"] = ("المستخدمين", "Users"),
        ["User"] = ("مستخدم", "User"),
        ["Auth"] = ("المصادقة", "Authentication"),
        ["AuditLogs"] = ("سجلات التدقيق", "Audit Logs"),
        ["AuditLog"] = ("سجل التدقيق", "Audit Log")
    };

    /// <summary>
    /// Translates an action type code into localized text.
    /// </summary>
    public static string GetLocalizedAction(string? action, string? lang = "en")
    {
        if (string.IsNullOrWhiteSpace(action)) return string.Empty;
        var isArabic = string.Equals(lang?.Trim(), "ar", StringComparison.OrdinalIgnoreCase);

        if (ActionTranslations.TryGetValue(action, out var trans))
        {
            return isArabic ? trans.Ar : trans.En;
        }

        return action;
    }

    /// <summary>
    /// Translates a database entity name into localized text.
    /// </summary>
    public static string GetLocalizedEntity(string? entityName, string? lang = "en")
    {
        if (string.IsNullOrWhiteSpace(entityName)) return string.Empty;
        var isArabic = string.Equals(lang?.Trim(), "ar", StringComparison.OrdinalIgnoreCase);

        if (EntityTranslations.TryGetValue(entityName, out var trans))
        {
            return isArabic ? trans.Ar : trans.En;
        }

        return entityName;
    }

    /// <summary>
    /// Enriches a single AuditLogResponse record with localized fields.
    /// </summary>
    public static AuditLogResponse Localize(this AuditLogResponse response, string? lang = "en")
    {
        var isArabic = string.Equals(lang?.Trim(), "ar", StringComparison.OrdinalIgnoreCase);

        response.LocalizedAction = GetLocalizedAction(response.Action, lang);
        response.LocalizedEntityName = GetLocalizedEntity(response.EntityName, lang);
        response.Description = GenerateDescription(response, isArabic);

        return response;
    }

    /// <summary>
    /// Enriches an entire collection of AuditLogResponse records with localized fields in-place.
    /// </summary>
    public static List<AuditLogResponse> Localize(this List<AuditLogResponse> responses, string? lang = "en")
    {
        foreach (var item in responses)
        {
            item.Localize(lang);
        }
        return responses;
    }

    private static string GenerateDescription(AuditLogResponse response, bool isArabic)
    {
        var actionUpper = response.Action?.Trim().ToUpperInvariant();
        var hasEntityId = !string.IsNullOrWhiteSpace(response.EntityId);
        var targetEntity = !string.IsNullOrWhiteSpace(response.LocalizedEntityName) ? response.LocalizedEntityName : response.EntityName;

        if (actionUpper == "LOGIN_SUCCESS")
        {
            if (isArabic)
                return hasEntityId ? $"تسجيل دخول ناجح للمستخدم '{response.EntityId}'" : "تسجيل دخول ناجح";
            return hasEntityId ? $"Successful login for user '{response.EntityId}'" : "Successful user login";
        }

        if (actionUpper == "LOGIN_FAILED")
        {
            if (isArabic)
                return hasEntityId ? $"محاولة تسجيل دخول فاشلة للمستخدم '{response.EntityId}'" : "محاولة تسجيل دخول فاشلة";
            return hasEntityId ? $"Failed login attempt for user '{response.EntityId}'" : "Failed login attempt";
        }

        if (actionUpper == "INSERT")
        {
            if (isArabic)
                return hasEntityId ? $"إضافة سجل جديد في {targetEntity} (معرف: {response.EntityId})" : $"إضافة سجل جديد في {targetEntity}";
            return hasEntityId ? $"Created new record in {targetEntity} (ID: {response.EntityId})" : $"Created new record in {targetEntity}";
        }

        if (actionUpper == "UPDATE")
        {
            if (isArabic)
                return hasEntityId ? $"تعديل سجل في {targetEntity} (معرف: {response.EntityId})" : $"تعديل سجل في {targetEntity}";
            return hasEntityId ? $"Updated record in {targetEntity} (ID: {response.EntityId})" : $"Updated record in {targetEntity}";
        }

        if (actionUpper == "DELETE")
        {
            if (isArabic)
                return hasEntityId ? $"حذف سجل من {targetEntity} (معرف: {response.EntityId})" : $"حذف سجل من {targetEntity}";
            return hasEntityId ? $"Deleted record from {targetEntity} (ID: {response.EntityId})" : $"Deleted record from {targetEntity}";
        }

        var targetAction = !string.IsNullOrWhiteSpace(response.LocalizedAction) ? response.LocalizedAction : response.Action;
        if (isArabic)
            return hasEntityId ? $"{targetAction} في {targetEntity} (معرف: {response.EntityId})" : $"{targetAction} في {targetEntity}";
        return hasEntityId ? $"{targetAction} on {targetEntity} (ID: {response.EntityId})" : $"{targetAction} on {targetEntity}";
    }
}
