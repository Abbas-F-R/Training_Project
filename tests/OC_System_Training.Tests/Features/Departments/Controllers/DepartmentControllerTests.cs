using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OC_System_Training.Features.Departments.Controllers;
using OC_System_Training.Features.Departments.Dtos;
using OC_System_Training.Features.Departments.Services;
using OC_System_Training.Shared.Base.dto;
using Xunit;

namespace OC_System_Training.Tests.Features.Departments.Controllers;

public class DepartmentControllerTests
{
    private readonly Mock<IDepartmentService> _serviceMock = new();
    private readonly DepartmentController _controller;

    public DepartmentControllerTests()
    {
        _controller = new DepartmentController(_serviceMock.Object);
    }

    [Fact]
    public void Controller_ShouldRequireAuthorization()
    {
        var authAttr = typeof(DepartmentController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .FirstOrDefault();

        authAttr.Should().NotBeNull();
    }

    [Theory]
    [InlineData(nameof(DepartmentController.Add))]
    [InlineData(nameof(DepartmentController.Update))]
    [InlineData(nameof(DepartmentController.Delete))]
    public void AdminEndpoints_ShouldRequireAdminRole(string methodName)
    {
        var method = typeof(DepartmentController).GetMethods()
            .FirstOrDefault(m => m.Name == methodName);

        var authAttr = method?.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        authAttr.Should().NotBeNull();
        authAttr!.Roles.Should().Be("Admin");
    }

    [Fact]
    public async Task Lookup_ShouldReturnOkWithDepartmentsList()
    {
        var departments = new List<DepartmentResponse>
        {
            new() { Id = 1, Name = "Computer Science", Code = "CS" },
            new() { Id = 2, Name = "Software Engineering", Code = "SE" }
        };

        _serviceMock.Setup(s => s.Lookup())
            .ReturnsAsync(ServiceResult<List<DepartmentResponse>>.Ok(departments));

        var result = await _controller.Lookup();

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        okResult.Value.Should().BeEquivalentTo(departments);
    }
}
