using FluentAssertions;
using OC_System_Training.Shared.Base.dto;
using OC_System_Training.Shared.Constants;
using OC_System_Training.Shared.Helper;
using OC_System_Training.Shared.Utils;
using Xunit;

namespace OC_System_Training.Tests;

public class SecurityAndHelperTests
{
    [Fact(DisplayName = "PasswordHasher produces verifiable BCrypt hash with salt")]
    public void PasswordHasher_HashAndVerify_MatchesSuccessfully()
    {
        // Arrange
        const string password = "MySecretPassword2026!";

        // Act
        var hash = PasswordHasher.Hash(password);

        // Assert
        hash.Should().StartWith("$2");
        PasswordHasher.Verify(password, hash).Should().BeTrue();
    }

    [Fact(DisplayName = "PasswordHasher rejects mismatched password")]
    public void PasswordHasher_VerifyWrongPassword_ReturnsFalse()
    {
        // Arrange
        var hash = PasswordHasher.Hash("OriginalPass123");

        // Act
        var result = PasswordHasher.Verify("WrongPass123", hash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory(DisplayName = "PasswordHasher rejects null, empty, or whitespace inputs")]
    [InlineData(null, "some_hash")]
    [InlineData("", "some_hash")]
    [InlineData("   ", "some_hash")]
    [InlineData("pass", null)]
    [InlineData("pass", "")]
    [InlineData("pass", "invalid_hash_structure")]
    public void PasswordHasher_VerifyInvalidInput_ReturnsFalse(string? pass, string? hash)
    {
        PasswordHasher.Verify(pass!, hash!).Should().BeFalse();
    }

    [Fact(DisplayName = "SqidCodec encodes long integer to at least 8 characters and decodes accurately")]
    public void SqidCodec_EncodeAndDecode_RoundtripsSuccessfully()
    {
        // Arrange
        const long id = 123456789;

        // Act
        var encoded = SqidCodec.Encode(id);
        var decoded = SqidCodec.TryDecode(encoded);

        // Assert
        encoded.Length.Should().BeGreaterOrEqualTo(8);
        decoded.Should().Be(id);
    }

    [Fact(DisplayName = "SqidCodec decodes plain numeric strings directly")]
    public void SqidCodec_TryDecode_PlainNumberString_ReturnsParsedNumber()
    {
        var decoded = SqidCodec.TryDecode("987654");
        decoded.Should().Be(987654);
    }

    [Theory(DisplayName = "SqidCodec returns null for invalid or empty strings")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SqidCodec_TryDecode_InvalidOrEmpty_ReturnsNull(string? input)
    {
        SqidCodec.TryDecode(input).Should().BeNull();
    }

    [Fact(DisplayName = "ErrorMessagesUtils returns localized English message by default")]
    public void ErrorMessagesUtils_ReturnsEnglishMessage()
    {
        var msg = Messages.RecordNotFound.GetMessage("en");
        msg.Should().Be("The requested record was not found.");
    }

    [Fact(DisplayName = "ErrorMessagesUtils returns localized Arabic message when requested")]
    public void ErrorMessagesUtils_ReturnsArabicMessage()
    {
        var msg = Messages.RecordNotFound.GetMessage("ar");
        msg.Should().Be("السجل المطلوب غير موجود.");
    }

    [Fact(DisplayName = "Response envelope correctly computes PagesCount and IsLast")]
    public void ResponseEnvelope_ComputesPaginationCorrectly()
    {
        var items = new List<string> { "item1", "item2" };
        
        // Page 1 of 5 (total 50 items, pageSize 10)
        var responsePage1 = new Response<string>(items, currentPage: 1, totalCount: 50, pageSize: 10);
        responsePage1.PagesCount.Should().Be(5);
        responsePage1.IsLast.Should().BeFalse();

        // Page 5 of 5
        var responsePage5 = new Response<string>(items, currentPage: 5, totalCount: 50, pageSize: 10);
        responsePage5.PagesCount.Should().Be(5);
        responsePage5.IsLast.Should().BeTrue();
    }

    [Fact(DisplayName = "ServiceResult.Ok constructs success result")]
    public void ServiceResult_Ok_ConstructsValidResult()
    {
        var result = ServiceResult<string>.Ok("SuccessData");
        result.IsSuccess.Should().BeTrue();
        result.Success.Should().BeTrue();
        result.Data.Should().Be("SuccessData");
        result.Error.Should().BeNull();
    }

    [Fact(DisplayName = "ServiceResult.Failure constructs failure result with error key")]
    public void ServiceResult_Failure_ConstructsValidResult()
    {
        var result = ServiceResult<string>.Failure(Messages.DuplicateRecord);
        result.IsSuccess.Should().BeFalse();
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Error.Should().Be(Messages.DuplicateRecord);
    }
}
