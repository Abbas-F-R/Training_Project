using FluentAssertions;
using Moq;
using OC_System_Training.Features.AuditLogs.Controllers;
using OC_System_Training.Features.AuditLogs.Dtos;
using OC_System_Training.Features.AuditLogs.Repositories;
using OC_System_Training.Features.AuditLogs.Services;
using OC_System_Training.Shared.Base.dto;
using Xunit;

namespace OC_System_Training.Tests;

public class AuditLogTests
{
    [Fact(DisplayName = "1. استعلام سجلات التدقيق يُرجع البيانات المرقمة بنجاح مع TotalCount")]
    public async Task AuditLogService_GetAll_ReturnsPagedResult()
    {
        // Arrange
        var mockRepo = new Mock<IAuditLogRepository>();
        var sampleLogs = new List<AuditLogResponse>
        {
            new() { Id = 1, Action = "INSERT", EntityName = "Students", EntityId = "101", IsSuccess = true },
            new() { Id = 2, Action = "LOGIN_SUCCESS", EntityName = "Auth", EntityId = "admin", IsSuccess = true }
        };

        mockRepo.Setup(r => r.GetAll(It.IsAny<AuditLogFilter>()))
            .ReturnsAsync((sampleLogs, 2));

        var service = new AuditLogService(mockRepo.Object);
        var request = new ServiceRequest<AuditLogFilter>
        {
            Dto = new AuditLogFilter { PageNumber = 1, PageSize = 10 }
        };

        // Act
        var result = await service.GetAll(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact(DisplayName = "2. التحقق من أن متحكم AuditLogController لا يحتوي على دوال تعديل أو حذف (Append-Only)")]
    public void AuditLogController_HasNoUpdateOrDeleteMethods()
    {
        var controllerType = typeof(AuditLogController);
        var methods = controllerType.GetMethods().Select(m => m.Name).ToList();

        methods.Should().NotContain("Update");
        methods.Should().NotContain("Delete");
        methods.Should().NotContain("Put");
    }

    [Fact(DisplayName = "3. التحقق من أن واجهة IAuditLogRepository لا تحتوي على دوال تعديل أو حذف")]
    public void AuditLogRepository_HasNoUpdateOrDeleteMethods()
    {
        var repoType = typeof(IAuditLogRepository);
        var methods = repoType.GetMethods().Select(m => m.Name).ToList();

        methods.Should().NotContain("Update");
        methods.Should().NotContain("Delete");
    }
}
