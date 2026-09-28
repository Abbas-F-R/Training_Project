using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Training_Project.Features.AuditLogs.Controllers;
using Training_Project.Features.AuditLogs.Dtos;
using Training_Project.Features.AuditLogs.Services;
using Training_Project.Shared.Base;
using Training_Project.Shared.Base.dto;
using Xunit;

namespace Training_Project.Tests.Features.AuditLogs.Controllers;

public class AuditLogControllerTests
{
    private readonly Mock<IAuditLogService> _serviceMock = new();
    private readonly AuditLogController _controller;

    public AuditLogControllerTests()
    {
        _controller = new AuditLogController(_serviceMock.Object);

        var userMock = new Mock<ICurrentUser>();
        userMock.Setup(u => u.UserId).Returns(1);
        userMock.Setup(u => u.UserName).Returns("admin");
        userMock.Setup(u => u.Role).Returns("Admin");
        userMock.Setup(u => u.Lang).Returns("en");
        _controller.SetCurrentUser(userMock.Object);
    }

    [Fact]
    public void Controller_ShouldHaveAdminAuthorizeAttribute()
    {
        var authAttr = typeof(AuditLogController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        authAttr.Should().NotBeNull();
        authAttr!.Roles.Should().Be("Admin");
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithPaginatedLogs()
    {
        var filter = new AuditLogFilter { PageNumber = 1, PageSize = 10, Action = "INSERT" };
        var logs = new List<AuditLogResponse>
        {
            new() { Id = 1, Action = "INSERT", EntityName = "Students", EntityId = "10", IsSuccess = true }
        };

        _serviceMock.Setup(s => s.GetAll(It.IsAny<ServiceRequest<AuditLogFilter>>()))
            .ReturnsAsync(ServiceResult<List<AuditLogResponse>>.PagedOk(logs, 1));

        var result = await _controller.GetAll(filter);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        okResult.Value.Should().BeOfType<Response<AuditLogResponse>>();
        var response = (Response<AuditLogResponse>)okResult.Value!;
        response.Data.Should().BeEquivalentTo(logs);
        response.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAll_ShouldPassCallerLanguageToServiceRequest()
    {
        var filter = new AuditLogFilter { PageNumber = 1, PageSize = 10 };
        var logs = new List<AuditLogResponse>
        {
            new()
            {
                Id = 1,
                Action = "INSERT",
                LocalizedAction = "إضافة",
                EntityName = "Students",
                LocalizedEntityName = "الطلاب",
                Description = "إضافة سجل جديد في الطلاب (معرف: 10)",
                EntityId = "10",
                IsSuccess = true
            }
        };

        ServiceRequest<AuditLogFilter>? capturedRequest = null;
        _serviceMock.Setup(s => s.GetAll(It.IsAny<ServiceRequest<AuditLogFilter>>()))
            .Callback<ServiceRequest<AuditLogFilter>>(req => capturedRequest = req)
            .ReturnsAsync(ServiceResult<List<AuditLogResponse>>.PagedOk(logs, 1));

        var userMock = new Mock<ICurrentUser>();
        userMock.Setup(u => u.UserId).Returns(5);
        userMock.Setup(u => u.UserName).Returns("admin");
        userMock.Setup(u => u.Role).Returns("Admin");
        userMock.Setup(u => u.Lang).Returns("ar");
        _controller.SetCurrentUser(userMock.Object);

        var result = await _controller.GetAll(filter);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Lang.Should().Be("ar");
        capturedRequest.UserId.Should().Be(5);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        var response = (Response<AuditLogResponse>)okResult.Value!;
        response.Data[0].LocalizedAction.Should().Be("إضافة");
        response.Data[0].LocalizedEntityName.Should().Be("الطلاب");
        response.Data[0].Description.Should().Be("إضافة سجل جديد في الطلاب (معرف: 10)");
    }
}
