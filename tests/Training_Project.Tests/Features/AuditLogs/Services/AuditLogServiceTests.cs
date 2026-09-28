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
    public void AuditLogs_ShouldBeAppendOnly_NoUpdateOrDeleteMethods()
    {
        var repoMethods = typeof(IAuditLogRepository).GetMethods().Select(m => m.Name).ToList();

        repoMethods.Should().NotContain("Update");
        repoMethods.Should().NotContain("Delete");
        repoMethods.Should().NotContain("Remove");
    }
}
