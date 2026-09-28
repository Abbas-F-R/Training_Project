using FluentAssertions;
using Training_Project.Shared.Constants;
using Training_Project.Shared.Utils;
using Xunit;

namespace Training_Project.Tests.Shared.Utils;

public class ErrorMessagesUtilsTests
{
    [Fact]
    public void GetMessage_English_ReturnsEnglishTranslation()
    {
        var msg = Messages.InvalidCredentials.GetMessage("en");
        msg.Should().Be("Invalid username or password.");
    }

    [Fact]
    public void GetMessage_Arabic_ReturnsArabicTranslation()
    {
        var msg = Messages.InvalidCredentials.GetMessage("ar");
        msg.Should().Be("اسم المستخدم أو كلمة المرور غير صحيحة.");
    }

    [Fact]
    public void GetMessage_AuditLogNotFound_ReturnsTranslations()
    {
        Messages.AuditLogNotFound.GetMessage("en").Should().Be("The requested audit log was not found.");
        Messages.AuditLogNotFound.GetMessage("ar").Should().Be("سجل التدقيق المطلوب غير موجود.");
    }

    [Fact]
    public void GetMessage_UnknownLanguage_DefaultsToEnglish()
    {
        var msg = Messages.DepartmentNotFound.GetMessage("fr");
        msg.Should().Be("The specified department was not found.");
    }

    [Fact]
    public void GetMessage_UnknownKey_ReturnsKeyItself()
    {
        var unknownKey = "NON_EXISTENT_ERROR_KEY";
        var msg = unknownKey.GetMessage("en");
        msg.Should().Be(unknownKey);
    }
}
