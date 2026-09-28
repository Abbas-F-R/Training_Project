using FluentAssertions;
using Moq;
using OC_System_Training.Features.Departments.Dtos;
using OC_System_Training.Features.Departments.Repositories;
using OC_System_Training.Features.Departments.Services;
using OC_System_Training.Shared.Base.dto;
using OC_System_Training.Shared.Constants;
using Xunit;

namespace OC_System_Training.Tests;

public class DepartmentServiceTests
{
    private readonly Mock<IDepartmentRepository> _repoMock = new();
    private readonly DepartmentService _service;

    public DepartmentServiceTests()
    {
        _service = new DepartmentService(_repoMock.Object);
    }

    [Fact(DisplayName = "Get returns DepartmentResponse when department exists")]
    public async Task Get_ExistingDepartment_ReturnsOk()
    {
        // Arrange
        var dept = new DepartmentResponse { Id = 1, Name = "Computer Science", Code = "CS" };
        _repoMock.Setup(r => r.Get(1, null)).ReturnsAsync(dept);

        // Act
        var result = await _service.Get(new ServiceRequest<long>(1, 10, "admin", "Admin", "en"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().Be("CS");
    }

    [Fact(DisplayName = "Get returns RecordNotFound when department does not exist")]
    public async Task Get_NonExistingDepartment_ReturnsNotFound()
    {
        // Arrange
        _repoMock.Setup(r => r.Get(99, null)).ReturnsAsync((DepartmentResponse?)null);

        // Act
        var result = await _service.Get(new ServiceRequest<long>(99, 10, "admin", "Admin", "en"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.RecordNotFound);
    }

    [Fact(DisplayName = "GetAll returns paginated list of departments")]
    public async Task GetAll_ReturnsPagedDepartments()
    {
        // Arrange
        var list = new List<DepartmentResponse>
        {
            new() { Id = 1, Name = "CS", Code = "CS" },
            new() { Id = 2, Name = "SE", Code = "SE" }
        };
        var filter = new DepartmentFilter { PageNumber = 1, PageSize = 10 };
        _repoMock.Setup(r => r.GetAll(filter, null)).ReturnsAsync((list, 2));

        // Act
        var result = await _service.GetAll(new ServiceRequest<DepartmentFilter>(filter, 10, "admin", "Admin", "en"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact(DisplayName = "Lookup returns list of all active departments")]
    public async Task Lookup_ReturnsAllActiveDepartments()
    {
        // Arrange
        var list = new List<DepartmentResponse>
        {
            new() { Id = 1, Name = "CS", Code = "CS" },
            new() { Id = 2, Name = "SE", Code = "SE" }
        };
        _repoMock.Setup(r => r.Lookup()).ReturnsAsync(list);

        // Act
        var result = await _service.Lookup();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Add succeeds when department code is unique")]
    public async Task Add_ValidDepartment_ReturnsOk()
    {
        // Arrange
        var form = new DepartmentForm { Name = "Cyber Security", Code = "CYBER" };
        _repoMock.Setup(r => r.IsDuplicateAsync("Code", "CYBER", null)).ReturnsAsync(false);
        _repoMock.Setup(r => r.Add(form, 10, null)).ReturnsAsync(new DepartmentResponse
        {
            Id = 3,
            Name = form.Name,
            Code = form.Code
        });

        // Act
        var result = await _service.Add(new ServiceRequest<DepartmentForm>(form, 10, "admin", "Admin", "en"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be(3);
    }

    [Fact(DisplayName = "Add fails when department code is duplicate")]
    public async Task Add_DuplicateDepartmentCode_ReturnsDuplicateDepartmentCode()
    {
        // Arrange
        var form = new DepartmentForm { Name = "Computer Science Duplicate", Code = "CS" };
        _repoMock.Setup(r => r.IsDuplicateAsync("Code", "CS", null)).ReturnsAsync(true);

        // Act
        var result = await _service.Add(new ServiceRequest<DepartmentForm>(form, 10, "admin", "Admin", "en"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.DuplicateDepartmentCode);
    }

    [Fact(DisplayName = "Add fails with InsertFailed when repository returns null")]
    public async Task Add_RepositoryFails_ReturnsInsertFailed()
    {
        // Arrange
        var form = new DepartmentForm { Name = "AI", Code = "AI" };
        _repoMock.Setup(r => r.IsDuplicateAsync("Code", "AI", null)).ReturnsAsync(false);
        _repoMock.Setup(r => r.Add(form, 10, null)).ReturnsAsync((DepartmentResponse?)null);

        // Act
        var result = await _service.Add(new ServiceRequest<DepartmentForm>(form, 10, "admin", "Admin", "en"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.InsertFailed);
    }

    [Fact(DisplayName = "Update succeeds when code is unique for this department")]
    public async Task Update_ValidDepartment_ReturnsOk()
    {
        // Arrange
        var update = new DepartmentUpdate { Name = "Software Engineering & Dev", Code = "SE" };
        _repoMock.Setup(r => r.IsDuplicateAsync("Code", "SE", 2)).ReturnsAsync(false);
        _repoMock.Setup(r => r.Update(2, update, 10, null)).ReturnsAsync(new DepartmentResponse
        {
            Id = 2,
            Name = update.Name,
            Code = update.Code
        });

        // Act
        var result = await _service.Update(2, new ServiceRequest<DepartmentUpdate>(update, 10, "admin", "Admin", "en"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Software Engineering & Dev");
    }

    [Fact(DisplayName = "Update fails when code belongs to another department")]
    public async Task Update_DuplicateCode_ReturnsDuplicateDepartmentCode()
    {
        // Arrange
        var update = new DepartmentUpdate { Name = "SE", Code = "CS" };
        _repoMock.Setup(r => r.IsDuplicateAsync("Code", "CS", 2)).ReturnsAsync(true);

        // Act
        var result = await _service.Update(2, new ServiceRequest<DepartmentUpdate>(update, 10, "admin", "Admin", "en"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.DuplicateDepartmentCode);
    }

    [Fact(DisplayName = "Delete succeeds when department is removed")]
    public async Task Delete_ExistingDepartment_ReturnsOk()
    {
        // Arrange
        _repoMock.Setup(r => r.Delete(2, 10, null)).ReturnsAsync(true);

        // Act
        var result = await _service.Delete(new ServiceRequest<long>(2, 10, "admin", "Admin", "en"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact(DisplayName = "Delete fails with DeleteFailed when repository delete fails")]
    public async Task Delete_RepositoryFails_ReturnsDeleteFailed()
    {
        // Arrange
        _repoMock.Setup(r => r.Delete(99, 10, null)).ReturnsAsync(false);

        // Act
        var result = await _service.Delete(new ServiceRequest<long>(99, 10, "admin", "Admin", "en"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.DeleteFailed);
    }
}
