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

namespace OC_System_Training.Tests.Features.Students.Services;

public class StudentServiceTests
{
    private readonly Mock<IRepositoryWrapper> _wrapperMock = new();
    private readonly Mock<IStudentRepository> _studentRepoMock = new();
    private readonly Mock<IDepartmentRepository> _deptRepoMock = new();
    private readonly StudentService _service;

    public StudentServiceTests()
    {
        _wrapperMock.Setup(w => w.Student).Returns(_studentRepoMock.Object);
        _wrapperMock.Setup(w => w.Department).Returns(_deptRepoMock.Object);
        _service = new StudentService(_wrapperMock.Object);
    }

    [Fact]
    public async Task Get_ExistingStudent_ReturnsSuccess()
    {
        var student = new StudentResponse { Id = 1, FullName = "Ali Ahmed", StudentCode = "STU-001" };
        _studentRepoMock.Setup(r => r.Get(1)).ReturnsAsync(student);

        var request = new ServiceRequest<long>(1, 1);
        var result = await _service.Get(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(student);
    }

    [Fact]
    public async Task Get_NonExistingStudent_ReturnsRecordNotFound()
    {
        _studentRepoMock.Setup(r => r.Get(99)).ReturnsAsync((StudentResponse?)null);

        var request = new ServiceRequest<long>(99, 1);
        var result = await _service.Get(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.RecordNotFound);
    }

    [Fact]
    public async Task GetAll_ReturnsPagedStudents()
    {
        var filter = new StudentFilter { PageNumber = 1, PageSize = 10 };
        var students = new List<StudentResponse>
        {
            new() { Id = 1, FullName = "Ali", StudentCode = "STU-001" },
            new() { Id = 2, FullName = "Sara", StudentCode = "STU-002" }
        };

        _studentRepoMock.Setup(r => r.GetAll(filter)).ReturnsAsync((students, 2));

        var request = new ServiceRequest<StudentFilter>(filter, 1);
        var result = await _service.GetAll(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Add_ValidStudent_Succeeds()
    {
        var form = new StudentForm
        {
            FullName = "New Student",
            StudentCode = "STU-100",
            DepartmentId = 1,
            Stage = 1
        };
        var created = new StudentResponse
        {
            Id = 10,
            FullName = form.FullName,
            StudentCode = form.StudentCode,
            DepartmentId = 1
        };

        _deptRepoMock.Setup(r => r.Get(1)).ReturnsAsync(new DepartmentResponse { Id = 1 });
        _studentRepoMock.Setup(r => r.IsDuplicateAsync("StudentCode", "STU-100", null)).ReturnsAsync(false);
        _studentRepoMock.Setup(r => r.Add(form, 1)).ReturnsAsync(created);

        var request = new ServiceRequest<StudentForm>(form, 1);
        var result = await _service.Add(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task Add_NonExistingDepartment_FailsWithDepartmentNotFound()
    {
        var form = new StudentForm
        {
            FullName = "New Student",
            StudentCode = "STU-100",
            DepartmentId = 999,
            Stage = 1
        };

        _deptRepoMock.Setup(r => r.Get(999)).ReturnsAsync((DepartmentResponse?)null);

        var request = new ServiceRequest<StudentForm>(form, 1);
        var result = await _service.Add(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.DepartmentNotFound);
    }

    [Fact]
    public async Task Add_DuplicateStudentCode_FailsWithDuplicateStudentCode()
    {
        var form = new StudentForm
        {
            FullName = "New Student",
            StudentCode = "STU-001",
            DepartmentId = 1,
            Stage = 1
        };

        _deptRepoMock.Setup(r => r.Get(1)).ReturnsAsync(new DepartmentResponse { Id = 1 });
        _studentRepoMock.Setup(r => r.IsDuplicateAsync("StudentCode", "STU-001", null)).ReturnsAsync(true);

        var request = new ServiceRequest<StudentForm>(form, 1);
        var result = await _service.Add(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.DuplicateStudentCode);
    }

    [Fact]
    public async Task Update_ExistingStudent_Succeeds()
    {
        var update = new StudentUpdate
        {
            FullName = "Updated Student",
            StudentCode = "STU-001-U",
            DepartmentId = 1,
            Stage = 2
        };
        var updated = new StudentResponse
        {
            Id = 1,
            FullName = update.FullName,
            StudentCode = update.StudentCode,
            DepartmentId = 1
        };

        _deptRepoMock.Setup(r => r.Get(1)).ReturnsAsync(new DepartmentResponse { Id = 1 });
        _studentRepoMock.Setup(r => r.IsDuplicateAsync("StudentCode", "STU-001-U", 1)).ReturnsAsync(false);
        _studentRepoMock.Setup(r => r.Update(1, update, 1)).ReturnsAsync(updated);

        var request = new ServiceRequest<StudentUpdate>(update, 1);
        var result = await _service.Update(1, request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(updated);
    }

    [Fact]
    public async Task Update_NonExistingStudent_FailsWithUpdateFailed()
    {
        var update = new StudentUpdate { FullName = "Updated", StudentCode = "STU-99", DepartmentId = 1 };
        _deptRepoMock.Setup(r => r.Get(1)).ReturnsAsync(new DepartmentResponse { Id = 1 });
        _studentRepoMock.Setup(r => r.IsDuplicateAsync("StudentCode", "STU-99", 99)).ReturnsAsync(false);
        _studentRepoMock.Setup(r => r.Update(99, update, 1)).ReturnsAsync((StudentResponse?)null);

        var request = new ServiceRequest<StudentUpdate>(update, 1);
        var result = await _service.Update(99, request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.UpdateFailed);
    }

    [Fact]
    public async Task Delete_ExistingStudent_Succeeds()
    {
        _studentRepoMock.Setup(r => r.Delete(1, 1)).ReturnsAsync(true);

        var request = new ServiceRequest<long>(1, 1);
        var result = await _service.Delete(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_NonExistingStudent_FailsWithDeleteFailed()
    {
        _studentRepoMock.Setup(r => r.Delete(99, 1)).ReturnsAsync(false);

        var request = new ServiceRequest<long>(99, 1);
        var result = await _service.Delete(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.DeleteFailed);
    }
}
