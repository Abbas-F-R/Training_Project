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
        msg.Should().Be("Ø§Ø³Ù… Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù… Ø£Ùˆ ÙƒÙ„Ù…Ø© Ø§Ù„Ù…Ø±ÙˆØ± ØºÙŠØ± ØµØ­ÙŠØ­Ø©.");
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
