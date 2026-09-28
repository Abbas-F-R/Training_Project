using FluentAssertions;
using Moq;
using OC_System_Training.Features.Departments.Dtos;
using OC_System_Training.Features.Departments.Repositories;
using OC_System_Training.Features.Students.Dtos;
using OC_System_Training.Features.Students.Repositories;
using OC_System_Training.Features.Students.Services;
using OC_System_Training.Infrastructure.Persistence.Repositories.RepositoryWrapper;
using OC_System_Training.Shared.Base.dto;
using OC_System_Training.Shared.Constants;
using Xunit;

namespace OC_System_Training.Tests;

public class StudentServiceTests
{
    private readonly Mock<IRepositoryWrapper> _wrapperMock = new();
    private readonly Mock<IStudentRepository> _studentRepoMock = new();
    private readonly Mock<IDepartmentRepository> _departmentRepoMock = new();
    private readonly StudentService _service;

    public StudentServiceTests()
    {
        _wrapperMock.Setup(w => w.Student).Returns(_studentRepoMock.Object);
        _wrapperMock.Setup(w => w.Department).Returns(_departmentRepoMock.Object);
        _service = new StudentService(_wrapperMock.Object);
    }

    [Fact(DisplayName = "Get returns StudentResponse when student exists")]
    public async Task Get_ExistingStudent_ReturnsOkWithData()
    {
        // Arrange
        var student = new StudentResponse
        {
            Id = 1,
            FullName = "John Doe",
            StudentCode = "STU-001",
            DepartmentId = 1,
            DepartmentName = "Computer Science",
            Stage = 3
        };

        _studentRepoMock.Setup(r => r.Get(1, null)).ReturnsAsync(student);

        // Act
        var result = await _service.Get(new ServiceRequest<long>(1, 10, "admin", "Admin", "en"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.StudentCode.Should().Be("STU-001");
    }

    [Fact(DisplayName = "Get returns RecordNotFound when student does not exist")]
    public async Task Get_NonExistingStudent_ReturnsNotFound()
    {
        // Arrange
        _studentRepoMock.Setup(r => r.Get(99, null)).ReturnsAsync((StudentResponse?)null);

        // Act
        var result = await _service.Get(new ServiceRequest<long>(99, 10, "admin", "Admin", "en"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.RecordNotFound);
    }

    [Fact(DisplayName = "GetAll returns paginated list of students with total count")]
    public async Task GetAll_ReturnsPagedStudents()
    {
        // Arrange
        var list = new List<StudentResponse>
        {
            new() { Id = 1, FullName = "Student A", StudentCode = "STU-A", DepartmentId = 1 },
            new() { Id = 2, FullName = "Student B", StudentCode = "STU-B", DepartmentId = 1 }
        };

        var filter = new StudentFilter { PageNumber = 1, PageSize = 10 };
        _studentRepoMock.Setup(r => r.GetAll(filter, null)).ReturnsAsync((list, 2));

        // Act
        var result = await _service.GetAll(new ServiceRequest<StudentFilter>(filter, 10, "admin", "Admin", "en"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact(DisplayName = "Add succeeds when department exists and student code is unique")]
    public async Task Add_ValidStudent_ReturnsOk()
    {
        // Arrange
        var form = new StudentForm
        {
            FullName = "Alice Smith",
            StudentCode = "STU-2026-001",
            DepartmentId = 1,
            Stage = 1
        };

        _departmentRepoMock.Setup(d => d.Get(1, null)).ReturnsAsync(new DepartmentResponse { Id = 1, Name = "CS" });
        _studentRepoMock.Setup(s => s.IsDuplicateAsync("StudentCode", "STU-2026-001", null)).ReturnsAsync(false);
        _studentRepoMock.Setup(s => s.Add(form, 10, null)).ReturnsAsync(new StudentResponse
        {
            Id = 5,
            FullName = form.FullName,
            StudentCode = form.StudentCode,
            DepartmentId = 1
        });

        // Act
        var result = await _service.Add(new ServiceRequest<StudentForm>(form, 10, "admin", "Admin", "en"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(5);
    }

    [Fact(DisplayName = "Add fails with InsertFailed when repository insert returns null")]
    public async Task Add_RepositoryFails_ReturnsInsertFailed()
    {
        // Arrange
        var form = new StudentForm { FullName = "Bob", StudentCode = "STU-002", DepartmentId = 1 };
        _departmentRepoMock.Setup(d => d.Get(1, null)).ReturnsAsync(new DepartmentResponse { Id = 1 });
        _studentRepoMock.Setup(s => s.IsDuplicateAsync("StudentCode", "STU-002", null)).ReturnsAsync(false);
        _studentRepoMock.Setup(s => s.Add(form, 10, null)).ReturnsAsync((StudentResponse?)null);

        // Act
        var result = await _service.Add(new ServiceRequest<StudentForm>(form, 10, "admin", "Admin", "en"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.InsertFailed);
    }

    [Fact(DisplayName = "Update succeeds when department exists and updated student code is unique")]
    public async Task Update_ValidStudent_ReturnsOk()
    {
        // Arrange
        var update = new StudentUpdate
        {
            FullName = "Alice Updated",
            StudentCode = "STU-2026-001",
            DepartmentId = 2,
            Stage = 2
        };

        _departmentRepoMock.Setup(d => d.Get(2, null)).ReturnsAsync(new DepartmentResponse { Id = 2, Name = "Software Engineering" });
        _studentRepoMock.Setup(s => s.IsDuplicateAsync("StudentCode", "STU-2026-001", 5)).ReturnsAsync(false);
        _studentRepoMock.Setup(s => s.Update(5, update, 10, null)).ReturnsAsync(new StudentResponse
        {
            Id = 5,
            FullName = update.FullName,
            StudentCode = update.StudentCode,
            DepartmentId = 2
        });

        // Act
        var result = await _service.Update(5, new ServiceRequest<StudentUpdate>(update, 10, "admin", "Admin", "en"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.FullName.Should().Be("Alice Updated");
    }

    [Fact(DisplayName = "Update fails when target department does not exist")]
    public async Task Update_NonExistingDepartment_ReturnsDepartmentNotFound()
    {
        // Arrange
        var update = new StudentUpdate { FullName = "Alice", StudentCode = "STU-001", DepartmentId = 99 };
        _departmentRepoMock.Setup(d => d.Get(99, null)).ReturnsAsync((DepartmentResponse?)null);

        // Act
        var result = await _service.Update(5, new ServiceRequest<StudentUpdate>(update, 10, "admin", "Admin", "en"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.DepartmentNotFound);
    }

    [Fact(DisplayName = "Update fails when student code belongs to another student")]
    public async Task Update_DuplicateStudentCode_ReturnsDuplicateStudentCode()
    {
        // Arrange
        var update = new StudentUpdate { FullName = "Alice", StudentCode = "STU-TAKEN", DepartmentId = 1 };
        _departmentRepoMock.Setup(d => d.Get(1, null)).ReturnsAsync(new DepartmentResponse { Id = 1 });
        _studentRepoMock.Setup(s => s.IsDuplicateAsync("StudentCode", "STU-TAKEN", 5)).ReturnsAsync(true);

        // Act
        var result = await _service.Update(5, new ServiceRequest<StudentUpdate>(update, 10, "admin", "Admin", "en"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.DuplicateStudentCode);
    }

    [Fact(DisplayName = "Delete succeeds when student exists")]
    public async Task Delete_ExistingStudent_ReturnsOk()
    {
        // Arrange
        _studentRepoMock.Setup(s => s.Delete(5, 10, null)).ReturnsAsync(true);

        // Act
        var result = await _service.Delete(new ServiceRequest<long>(5, 10, "admin", "Admin", "en"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact(DisplayName = "Delete fails with DeleteFailed when repository delete fails")]
    public async Task Delete_RepositoryFails_ReturnsDeleteFailed()
    {
        // Arrange
        _studentRepoMock.Setup(s => s.Delete(99, 10, null)).ReturnsAsync(false);

        // Act
        var result = await _service.Delete(new ServiceRequest<long>(99, 10, "admin", "Admin", "en"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.DeleteFailed);
    }
}
