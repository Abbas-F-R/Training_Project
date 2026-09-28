using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OC_System_Training.Features.Auth.Controllers;
using OC_System_Training.Features.Auth.Dtos;
using OC_System_Training.Features.Auth.Services;
using OC_System_Training.Features.Departments.Controllers;
using OC_System_Training.Features.Departments.Dtos;
using OC_System_Training.Features.Departments.Services;
using OC_System_Training.Features.Students.Controllers;
using OC_System_Training.Features.Students.Dtos;
using OC_System_Training.Features.Students.Services;
using OC_System_Training.Shared.Base;
using OC_System_Training.Shared.Base.dto;
using Xunit;

namespace OC_System_Training.Tests;

public class ControllerTests
{
    private static void SetupControllerContext(BaseController controller, long userId = 1, string userName = "admin", string role = "Admin")
    {
        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(u => u.UserId).Returns(userId);
        currentUserMock.Setup(u => u.UserName).Returns(userName);
        currentUserMock.Setup(u => u.Role).Returns(role);
        currentUserMock.Setup(u => u.Lang).Returns("en");
        currentUserMock.Setup(u => u.IsAuthenticated).Returns(true);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ICurrentUser))).Returns(currentUserMock.Object);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProviderMock.Object
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact(DisplayName = "AuthController.GetMe returns authenticated user identity payload")]
    public void AuthController_GetMe_ReturnsCurrentUserIdentity()
    {
        // Arrange
        var authServiceMock = new Mock<IAuthService>();
        var controller = new AuthController(authServiceMock.Object);
        SetupControllerContext(controller, userId: 42, userName: "john_doe", role: "User");

        // Act
        var result = controller.GetMe();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().NotBeNull();
    }

    [Fact(DisplayName = "StudentController.Get returns student entity when service succeeds")]
    public async Task StudentController_Get_ReturnsOkActionResult()
    {
        // Arrange
        var studentServiceMock = new Mock<IStudentService>();
        var expectedStudent = new StudentResponse { Id = 1, FullName = "Test Student", StudentCode = "STU-001" };

        studentServiceMock.Setup(s => s.Get(It.IsAny<ServiceRequest<long>>()))
            .ReturnsAsync(ServiceResult<StudentResponse>.Ok(expectedStudent));

        var controller = new StudentController(studentServiceMock.Object);
        SetupControllerContext(controller);

        // Act
        var actionResult = await controller.Get(1);

        // Assert
        actionResult.Result.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedStudent);
    }

    [Fact(DisplayName = "StudentController.Add returns created student response")]
    public async Task StudentController_Add_ReturnsOkActionResult()
    {
        // Arrange
        var studentServiceMock = new Mock<IStudentService>();
        var form = new StudentForm { FullName = "New Student", StudentCode = "STU-NEW", DepartmentId = 1 };
        var createdStudent = new StudentResponse { Id = 10, FullName = form.FullName, StudentCode = form.StudentCode };

        studentServiceMock.Setup(s => s.Add(It.IsAny<ServiceRequest<StudentForm>>()))
            .ReturnsAsync(ServiceResult<StudentResponse>.Ok(createdStudent));

        var controller = new StudentController(studentServiceMock.Object);
        SetupControllerContext(controller);

        // Act
        var actionResult = await controller.Add(form);

        // Assert
        actionResult.Result.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(createdStudent);
    }

    [Fact(DisplayName = "DepartmentController.Lookup returns active department list")]
    public async Task DepartmentController_Lookup_ReturnsOkActionResult()
    {
        // Arrange
        var deptServiceMock = new Mock<IDepartmentService>();
        var deptList = new List<DepartmentResponse>
        {
            new() { Id = 1, Name = "Computer Science", Code = "CS" },
            new() { Id = 2, Name = "Software Engineering", Code = "SE" }
        };

        deptServiceMock.Setup(s => s.Lookup())
            .ReturnsAsync(ServiceResult<List<DepartmentResponse>>.Ok(deptList));

        var controller = new DepartmentController(deptServiceMock.Object);
        SetupControllerContext(controller);

        // Act
        var actionResult = await controller.Lookup();

        // Assert
        actionResult.Result.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(deptList);
    }
}
