using FluentAssertions;
using Moq;
using Training_Project.Features.AuditLogs.Dtos;
using Training_Project.Features.AuditLogs.Repositories;
using Training_Project.Features.AuditLogs.Services;
using Training_Project.Shared.Base.dto;
using Xunit;

namespace Training_Project.Tests.Features.AuditLogs.Services;

public class AuditLogServiceTests
{
    private readonly Mock<IAuditLogRepository> _repoMock = new();
    private readonly AuditLogService _service;

    public AuditLogServiceTests()
    {
        _service = new AuditLogService(_repoMock.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnPagedOkWithAuditLogs()
    {
        var filter = new AuditLogFilter { PageNumber = 1, PageSize = 10, EntityName = "Students" };
        var logs = new List<AuditLogResponse>
        {
            new() { Id = 1, UserId = 1, Action = "INSERT", EntityName = "Students", EntityId = "101", IsSuccess = true },
            new() { Id = 2, UserId = 1, Action = "UPDATE", EntityName = "Students", EntityId = "101", IsSuccess = true }
        };

        _repoMock.Setup(r => r.GetAll(filter)).ReturnsAsync((logs, 2));

        var request = new ServiceRequest<AuditLogFilter>(filter, 1);
        var result = await _service.GetAll(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAll_WithArabicLanguage_ShouldEnrichResponseWithArabicLocalization()
    {
        var filter = new AuditLogFilter { PageNumber = 1, PageSize = 10 };
        var logs = new List<AuditLogResponse>
        {
            new() { Id = 1, UserId = 1, Action = "INSERT", EntityName = "Students", EntityId = "101", IsSuccess = true },
            new() { Id = 2, UserId = 2, Action = "LOGIN_FAILED", EntityName = "Auth", EntityId = "bad_user", IsSuccess = false }
        };

        _repoMock.Setup(r => r.GetAll(filter)).ReturnsAsync((logs, 2));

        var request = new ServiceRequest<AuditLogFilter>(filter, 1, "admin", "Admin", "ar");
        var result = await _service.GetAll(request);

        result.IsSuccess.Should().BeTrue();
        result.Data![0].LocalizedAction.Should().Be("إضافة");
        result.Data[0].LocalizedEntityName.Should().Be("الطلاب");
        result.Data[0].Description.Should().Be("إضافة سجل جديد في الطلاب (معرف: 101)");

        result.Data[1].LocalizedAction.Should().Be("فشل تسجيل الدخول");
        result.Data[1].LocalizedEntityName.Should().Be("المصادقة");
        result.Data[1].Description.Should().Be("محاولة تسجيل دخول فاشلة للمستخدم 'bad_user'");
    }

    [Fact]
    public async Task GetAll_WithEnglishLanguage_ShouldEnrichResponseWithEnglishLocalization()
    {
        var filter = new AuditLogFilter { PageNumber = 1, PageSize = 10 };
        var logs = new List<AuditLogResponse>
        {
            new() { Id = 1, UserId = 1, Action = "UPDATE", EntityName = "Departments", EntityId = "4", IsSuccess = true }
        };

        _repoMock.Setup(r => r.GetAll(filter)).ReturnsAsync((logs, 1));

        var request = new ServiceRequest<AuditLogFilter>(filter, 1, "admin", "Admin", "en");
        var result = await _service.GetAll(request);

        result.IsSuccess.Should().BeTrue();
        result.Data![0].LocalizedAction.Should().Be("Update");
        result.Data[0].LocalizedEntityName.Should().Be("Departments");
        result.Data[0].Description.Should().Be("Updated record in Departments (ID: 4)");
    }

    [Fact]
    public void AuditLogs_ShouldBeAppendOnly_NoUpdateOrDeleteMethods()
    {
        var repoMethods = typeof(IAuditLogRepository).GetMethods().Select(m => m.Name).ToList();

        repoMethods.Should().NotContain("Update");
        repoMethods.Should().NotContain("Delete");
        repoMethods.Should().NotContain("Remove");
    }
}
