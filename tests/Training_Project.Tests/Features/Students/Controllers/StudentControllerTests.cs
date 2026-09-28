using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Training_Project.Features.Students.Controllers;
using Training_Project.Features.Students.Dtos;
using Training_Project.Features.Students.Services;
using Training_Project.Shared.Base;
using Training_Project.Shared.Base.dto;
using Xunit;

namespace Training_Project.Tests.Features.Students.Controllers;

public class StudentControllerTests
{
    private readonly Mock<IStudentService> _serviceMock = new();
    private readonly StudentController _controller;

    public StudentControllerTests()
    {
        _controller = new StudentController(_serviceMock.Object);

        var userMock = new Mock<ICurrentUser>();
        userMock.Setup(u => u.UserId).Returns(1);
        userMock.Setup(u => u.UserName).Returns("admin");
        userMock.Setup(u => u.Role).Returns("Admin");
        userMock.Setup(u => u.Lang).Returns("en");
        _controller.SetCurrentUser(userMock.Object);
    }

    [Fact]
    public void Controller_ShouldRequireAuthorization()
    {
        var authAttr = typeof(StudentController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .FirstOrDefault();

        authAttr.Should().NotBeNull();
    }

    [Fact]
    public void Delete_ShouldRequireAdminRole()
    {
        var method = typeof(StudentController).GetMethod(nameof(StudentController.Delete));
        var authAttr = method?.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        authAttr.Should().NotBeNull();
        authAttr!.Roles.Should().Be("Admin");
    }

    [Fact]
    public async Task Get_ShouldReturnOkWithStudent()
    {
        var student = new StudentResponse
        {
            Id = 1,
            FullName = "John Student",
            StudentCode = "STU-001",
            DepartmentId = 1,
            Stage = 2
        };

        _serviceMock.Setup(s => s.Get(It.IsAny<ServiceRequest<long>>()))
            .ReturnsAsync(ServiceResult<StudentResponse>.Ok(student));

        var result = await _controller.Get(1);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        okResult.Value.Should().BeEquivalentTo(student);
    }
}
