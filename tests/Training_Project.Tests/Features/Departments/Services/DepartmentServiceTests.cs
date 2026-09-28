using FluentAssertions;
using Moq;
using Training_Project.Features.Departments.Dtos;
using Training_Project.Features.Departments.Repositories;
using Training_Project.Features.Departments.Services;
using Training_Project.Shared.Base.dto;
using Training_Project.Shared.Constants;
using Xunit;

namespace Training_Project.Tests.Features.Departments.Services;

public class DepartmentServiceTests
{
    private readonly Mock<IDepartmentRepository> _deptRepoMock = new();
    private readonly DepartmentService _service;

    public DepartmentServiceTests()
    {
        _service = new DepartmentService(_deptRepoMock.Object);
    }

    [Fact]
    public async Task Get_ExistingDepartment_ReturnsSuccess()
    {
        var dept = new DepartmentResponse { Id = 1, Name = "Computer Science", Code = "CS" };
        _deptRepoMock.Setup(r => r.Get(1)).ReturnsAsync(dept);

        var request = new ServiceRequest<long>(1, 1);
        var result = await _service.Get(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(dept);
    }

    [Fact]
    public async Task Get_NonExistingDepartment_ReturnsRecordNotFound()
    {
        _deptRepoMock.Setup(r => r.Get(99)).ReturnsAsync((DepartmentResponse?)null);

        var request = new ServiceRequest<long>(99, 1);
        var result = await _service.Get(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.RecordNotFound);
    }

    [Fact]
    public async Task GetAll_ReturnsPagedDepartments()
    {
        var filter = new DepartmentFilter { PageNumber = 1, PageSize = 10 };
        var depts = new List<DepartmentResponse>
        {
            new() { Id = 1, Name = "Computer Science", Code = "CS" },
            new() { Id = 2, Name = "Software Engineering", Code = "SE" }
        };

        _deptRepoMock.Setup(r => r.GetAll(filter)).ReturnsAsync((depts, 2));

        var request = new ServiceRequest<DepartmentFilter>(filter, 1);
        var result = await _service.GetAll(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Lookup_ReturnsActiveDepartments()
    {
        var lookups = new List<DepartmentResponse>
        {
            new() { Id = 1, Name = "Computer Science", Code = "CS" }
        };

        _deptRepoMock.Setup(r => r.Lookup()).ReturnsAsync(lookups);

        var result = await _service.Lookup();

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(lookups);
    }

    [Fact]
    public async Task Add_UniqueCode_Succeeds()
    {
        var form = new DepartmentForm { Name = "Cybersecurity", Code = "CYBER" };
        var created = new DepartmentResponse { Id = 10, Name = form.Name, Code = form.Code };

        _deptRepoMock.Setup(r => r.IsDuplicateAsync("Code", "CYBER", null)).ReturnsAsync(false);
        _deptRepoMock.Setup(r => r.Add(form, 1)).ReturnsAsync(created);

        var request = new ServiceRequest<DepartmentForm>(form, 1);
        var result = await _service.Add(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task Add_DuplicateCode_FailsWithDuplicateDepartmentCode()
    {
        var form = new DepartmentForm { Name = "Cybersecurity", Code = "CS" };

        _deptRepoMock.Setup(r => r.IsDuplicateAsync("Code", "CS", null)).ReturnsAsync(true);

        var request = new ServiceRequest<DepartmentForm>(form, 1);
        var result = await _service.Add(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.DuplicateDepartmentCode);
    }

    [Fact]
    public async Task Update_ExistingDepartment_Succeeds()
    {
        var update = new DepartmentUpdate { Name = "CS Updated", Code = "CSU" };
        var updated = new DepartmentResponse { Id = 1, Name = update.Name, Code = update.Code };

        _deptRepoMock.Setup(r => r.IsDuplicateAsync("Code", "CSU", 1)).ReturnsAsync(false);
        _deptRepoMock.Setup(r => r.Update(1, update, 1)).ReturnsAsync(updated);

        var request = new ServiceRequest<DepartmentUpdate>(update, 1);
        var result = await _service.Update(1, request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(updated);
    }

    [Fact]
    public async Task Update_NonExistingDepartment_Fails()
    {
        var update = new DepartmentUpdate { Name = "CS Updated", Code = "CSU" };
        _deptRepoMock.Setup(r => r.IsDuplicateAsync("Code", "CSU", 99)).ReturnsAsync(false);
        _deptRepoMock.Setup(r => r.Update(99, update, 1)).ReturnsAsync((DepartmentResponse?)null);

        var request = new ServiceRequest<DepartmentUpdate>(update, 1);
        var result = await _service.Update(99, request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.UpdateFailed);
    }

    [Fact]
    public async Task Delete_ExistingDepartment_Succeeds()
    {
        _deptRepoMock.Setup(r => r.Delete(1, 1)).ReturnsAsync(true);

        var request = new ServiceRequest<long>(1, 1);
        var result = await _service.Delete(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_NonExistingDepartment_Fails()
    {
        _deptRepoMock.Setup(r => r.Delete(99, 1)).ReturnsAsync(false);

        var request = new ServiceRequest<long>(99, 1);
        var result = await _service.Delete(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.DeleteFailed);
    }
}
