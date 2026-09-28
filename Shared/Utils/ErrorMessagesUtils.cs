namespace Training_Project.Shared.Utils;

/// <summary>
/// Central localized error and operational messages dictionary supporting Arabic and English.
/// </summary>
public static class ErrorMessagesUtils
{
    private static readonly Dictionary<string, Dictionary<string, string>> MessagesDict = new()
    {
        [Messages.RecordNotFound] = new()
        {
            ["ar"] = "Ø§Ù„Ø³Ø¬Ù„ Ø§Ù„Ù…Ø·Ù„ÙˆØ¨ ØºÙŠØ± Ù…ÙˆØ¬ÙˆØ¯.",
            ["en"] = "The requested record was not found."
        },
        [Messages.InsertFailed] = new()
        {
            ["ar"] = "ÙØ´Ù„Øª Ø¹Ù…Ù„ÙŠØ© Ø§Ù„Ø¥Ø¶Ø§ÙØ©ØŒ ÙŠØ±Ø¬Ù‰ Ø§Ù„Ù…Ø­Ø§ÙˆÙ„Ø© Ù…Ø±Ø© Ø£Ø®Ø±Ù‰.",
            ["en"] = "Failed to insert record, please try again."
        },
        [Messages.UpdateFailed] = new()
        {
            ["ar"] = "ÙØ´Ù„Øª Ø¹Ù…Ù„ÙŠØ© Ø§Ù„ØªØ¹Ø¯ÙŠÙ„ØŒ ÙŠØ±Ø¬Ù‰ Ø§Ù„ØªØ£ÙƒØ¯ Ù…Ù† ØµØ­Ø© Ø§Ù„Ø¨ÙŠØ§Ù†Ø§Øª.",
            ["en"] = "Failed to update record, please verify input data."
        },
        [Messages.DeleteFailed] = new()
        {
            ["ar"] = "ÙØ´Ù„Øª Ø¹Ù…Ù„ÙŠØ© Ø§Ù„Ø­Ø°Ù.",
            ["en"] = "Failed to delete record."
        },
        [Messages.DuplicateRecord] = new()
        {
            ["ar"] = "Ù‡Ø°Ø§ Ø§Ù„Ø³Ø¬Ù„ Ù…ÙˆØ¬ÙˆØ¯ Ù…Ø³Ø¨Ù‚Ø§Ù‹ ÙÙŠ Ø§Ù„Ù†Ø¸Ø§Ù….",
            ["en"] = "This record already exists in the system."
        },
        [Messages.DuplicateStudentCode] = new()
        {
            ["ar"] = "Ø§Ù„Ø±Ù‚Ù… Ø§Ù„Ø¬Ø§Ù…Ø¹ÙŠ Ù„Ù„Ø·Ø§Ù„Ø¨ Ù…Ø³Ø¬Ù„ Ù…Ø³Ø¨Ù‚Ø§Ù‹ Ù„Ø·Ø§Ù„Ø¨ Ø¢Ø®Ø±.",
            ["en"] = "Student code already belongs to another student."
        },
        [Messages.DuplicateDepartmentCode] = new()
        {
            ["ar"] = "Ø±Ù…Ø² Ø§Ù„Ù‚Ø³Ù… Ù…Ø³Ø¬Ù„ Ù…Ø³Ø¨Ù‚Ø§Ù‹ Ù„Ù‚Ø³Ù… Ø¢Ø®Ø±.",
            ["en"] = "Department code already exists."
        },
        [Messages.InvalidCredentials] = new()
        {
            ["ar"] = "Ø§Ø³Ù… Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù… Ø£Ùˆ ÙƒÙ„Ù…Ø© Ø§Ù„Ù…Ø±ÙˆØ± ØºÙŠØ± ØµØ­ÙŠØ­Ø©.",
            ["en"] = "Invalid username or password."
        },
        [Messages.UserNotFound] = new()
        {
            ["ar"] = "Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù… ØºÙŠØ± Ù…ÙˆØ¬ÙˆØ¯.",
            ["en"] = "User was not found."
        },
        [Messages.UserInactive] = new()
        {
            ["ar"] = "Ø­Ø³Ø§Ø¨ Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù… Ù…Ø¹Ø·Ù„ Ø­Ø§Ù„ÙŠØ§Ù‹.",
            ["en"] = "User account is currently disabled."
        },
        [Messages.Unauthorized] = new()
        {
            ["ar"] = "Ù„ÙŠØ³ Ù„Ø¯ÙŠÙƒ ØµÙ„Ø§Ø­ÙŠØ© Ù„ØªÙ†ÙÙŠØ° Ù‡Ø°Ø§ Ø§Ù„Ø¥Ø¬Ø±Ø§Ø¡.",
            ["en"] = "You are not authorized to perform this action."
        },
        [Messages.DepartmentNotFound] = new()
        {
            ["ar"] = "Ø§Ù„Ù‚Ø³Ù… Ø§Ù„Ø¯Ø±Ø§Ø³ÙŠ Ø§Ù„Ù…Ø­Ø¯Ø¯ ØºÙŠØ± Ù…ÙˆØ¬ÙˆØ¯.",
            ["en"] = "The specified department was not found."
        }
    };

    /// <summary>
    /// Translates a message key into localized text based on the requested language code.
    /// </summary>
    public static string GetMessage(this string key, string? lang = "en")
    {
        var targetLang = string.IsNullOrWhiteSpace(lang) ? "en" : lang.Trim().ToLowerInvariant();
        if (targetLang != "ar" && targetLang != "en") targetLang = "en";

        if (MessagesDict.TryGetValue(key, out var translations))
        {
            if (translations.TryGetValue(targetLang, out var message))
                return message;
            if (translations.TryGetValue("en", out var fallbackEn))
                return fallbackEn;
            if (translations.TryGetValue("ar", out var fallbackAr))
                return fallbackAr;
        }

        return key;
    }
}
