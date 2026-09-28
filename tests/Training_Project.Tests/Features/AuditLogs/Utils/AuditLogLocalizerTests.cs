using FluentAssertions;
using Training_Project.Features.AuditLogs.Dtos;
using Training_Project.Features.AuditLogs.Utils;
using Xunit;

namespace Training_Project.Tests.Features.AuditLogs.Utils;

public class AuditLogLocalizerTests
{
    [Fact]
    public void Localize_English_EnrichesAuditLogWithEnglishTranslations()
    {
        var log = new AuditLogResponse
        {
            Id = 1,
            Action = "INSERT",
            EntityName = "Students",
            EntityId = "10"
        };

        log.Localize("en");

        log.Action.Should().Be("INSERT");
        log.EntityName.Should().Be("Students");
        log.LocalizedAction.Should().Be("Insert");
        log.LocalizedEntityName.Should().Be("Students");
        log.Description.Should().Be("Created new record in Students (ID: 10)");
    }

    [Fact]
    public void Localize_Arabic_EnrichesAuditLogWithArabicTranslations()
    {
        var log = new AuditLogResponse
        {
            Id = 1,
            Action = "INSERT",
            EntityName = "Students",
            EntityId = "10"
        };

        log.Localize("ar");

        log.Action.Should().Be("INSERT");
        log.EntityName.Should().Be("Students");
        log.LocalizedAction.Should().Be("إضافة");
        log.LocalizedEntityName.Should().Be("الطلاب");
        log.Description.Should().Be("إضافة سجل جديد في الطلاب (معرف: 10)");
    }

    [Theory]
    [InlineData("en", "Successful Login", "Authentication", "Successful login for user 'admin'")]
    [InlineData("ar", "تسجيل دخول ناجح", "المصادقة", "تسجيل دخول ناجح للمستخدم 'admin'")]
    public void Localize_LoginSuccess_GeneratesProperDescription(string lang, string expectedAction, string expectedEntity, string expectedDesc)
    {
        var log = new AuditLogResponse
        {
            Id = 1,
            Action = "LOGIN_SUCCESS",
            EntityName = "Auth",
            EntityId = "admin"
        };

        log.Localize(lang);

        log.LocalizedAction.Should().Be(expectedAction);
        log.LocalizedEntityName.Should().Be(expectedEntity);
        log.Description.Should().Be(expectedDesc);
    }

    [Theory]
    [InlineData("en", "Failed Login", "Authentication", "Failed login attempt for user 'attacker'")]
    [InlineData("ar", "فشل تسجيل الدخول", "المصادقة", "محاولة تسجيل دخول فاشلة للمستخدم 'attacker'")]
    public void Localize_LoginFailed_GeneratesProperDescription(string lang, string expectedAction, string expectedEntity, string expectedDesc)
    {
        var log = new AuditLogResponse
        {
            Id = 1,
            Action = "LOGIN_FAILED",
            EntityName = "Auth",
            EntityId = "attacker"
        };

        log.Localize(lang);

        log.LocalizedAction.Should().Be(expectedAction);
        log.LocalizedEntityName.Should().Be(expectedEntity);
        log.Description.Should().Be(expectedDesc);
    }

    [Theory]
    [InlineData("UPDATE", "Departments", "5", "en", "Update", "Departments", "Updated record in Departments (ID: 5)")]
    [InlineData("UPDATE", "Departments", "5", "ar", "تعديل", "الأقسام", "تعديل سجل في الأقسام (معرف: 5)")]
    [InlineData("DELETE", "Students", "12", "en", "Delete", "Students", "Deleted record from Students (ID: 12)")]
    [InlineData("DELETE", "Students", "12", "ar", "حذف", "الطلاب", "حذف سجل من الطلاب (معرف: 12)")]
    public void Localize_UpdateAndDelete_GeneratesProperDescription(
        string action, string entity, string entityId, string lang,
        string expectedAction, string expectedEntity, string expectedDesc)
    {
        var log = new AuditLogResponse
        {
            Id = 1,
            Action = action,
            EntityName = entity,
            EntityId = entityId
        };

        log.Localize(lang);

        log.LocalizedAction.Should().Be(expectedAction);
        log.LocalizedEntityName.Should().Be(expectedEntity);
        log.Description.Should().Be(expectedDesc);
    }

    [Fact]
    public void Localize_WithoutEntityId_GeneratesDescriptionWithoutId()
    {
        var logEn = new AuditLogResponse { Action = "INSERT", EntityName = "Students", EntityId = null };
        logEn.Localize("en");
        logEn.Description.Should().Be("Created new record in Students");

        var logAr = new AuditLogResponse { Action = "INSERT", EntityName = "Students", EntityId = null };
        logAr.Localize("ar");
        logAr.Description.Should().Be("إضافة سجل جديد في الطلاب");
    }

    [Fact]
    public void Localize_UnknownActionAndEntity_FallsBackToRawValues()
    {
        var log = new AuditLogResponse
        {
            Action = "EXPORT",
            EntityName = "CustomReport",
            EntityId = "42"
        };

        log.Localize("ar");

        log.LocalizedAction.Should().Be("EXPORT");
        log.LocalizedEntityName.Should().Be("CustomReport");
        log.Description.Should().Be("EXPORT في CustomReport (معرف: 42)");
    }

    [Fact]
    public void Localize_List_EnrichesAllElementsInPlace()
    {
        var list = new List<AuditLogResponse>
        {
            new() { Id = 1, Action = "INSERT", EntityName = "Departments", EntityId = "1" },
            new() { Id = 2, Action = "UPDATE", EntityName = "Departments", EntityId = "1" }
        };

        list.Localize("ar");

        list[0].LocalizedAction.Should().Be("إضافة");
        list[0].LocalizedEntityName.Should().Be("الأقسام");
        list[1].LocalizedAction.Should().Be("تعديل");
        list[1].LocalizedEntityName.Should().Be("الأقسام");
    }

    [Fact]
    public void DirectTranslations_ReturnCorrectValues()
    {
        AuditLogLocalizer.GetLocalizedAction("INSERT", "ar").Should().Be("إضافة");
        AuditLogLocalizer.GetLocalizedAction("INSERT", "en").Should().Be("Insert");
        AuditLogLocalizer.GetLocalizedEntity("Students", "ar").Should().Be("الطلاب");
        AuditLogLocalizer.GetLocalizedEntity("Students", "en").Should().Be("Students");
    }
}
