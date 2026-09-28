using FluentAssertions;
using Moq;
using OC_System_Training.Features.Departments.Dtos;
using OC_System_Training.Features.Departments.Repositories;
using OC_System_Training.Features.Departments.Validators;
using OC_System_Training.Features.Students.Dtos;
using OC_System_Training.Features.Students.Repositories;
using OC_System_Training.Features.Students.Services;
using OC_System_Training.Features.Students.Validators;
using OC_System_Training.Infrastructure.Persistence.Repositories.RepositoryWrapper;
using OC_System_Training.Shared.Base.dto;
using OC_System_Training.Shared.Constants;
using Xunit;

namespace OC_System_Training.Tests;

public class ValidationTests
{
    private readonly StudentFormValidator _studentValidator = new();
    private readonly DepartmentFormValidator _departmentValidator = new();

    [Fact(DisplayName = "StudentForm with valid complete data passes validation")]
    public void StudentForm_WithValidData_PassesValidation()
    {
        var form = new StudentForm
        {
            FullName = "Karrar Haider Jassim",
            StudentCode = "STU-2026-099",
            Email = "karrar@univ.edu",
            PhoneNumber = "07701234567",
            DepartmentId = 1,
            Stage = 2,
            BirthDate = new DateTime(2003, 5, 10)
        };

        var result = _studentValidator.Validate(form);
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "StudentForm with missing required fields fails validation")]
    public void StudentForm_WithEmptyRequiredFields_FailsValidation()
    {
        var form = new StudentForm
        {
            FullName = "",
            StudentCode = "",
            DepartmentId = 0
        };

        var result = _studentValidator.Validate(form);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(StudentForm.FullName));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(StudentForm.StudentCode));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(StudentForm.DepartmentId));
    }

    [Fact(DisplayName = "StudentForm with stage out of range 1-6 fails validation")]
    public void StudentForm_WithInvalidStage_FailsValidation()
    {
        var form = new StudentForm
        {
            FullName = "Haider Jawad",
            StudentCode = "STU-999",
            DepartmentId = 1,
            Stage = 7
        };

        var result = _studentValidator.Validate(form);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(StudentForm.Stage));
    }

    [Fact(DisplayName = "StudentForm with future birth date fails validation")]
    public void StudentForm_WithFutureBirthDate_FailsValidation()
    {
        var form = new StudentForm
        {
            FullName = "Zainab Ali",
            StudentCode = "STU-100",
            DepartmentId = 1,
            BirthDate = DateTime.Today.AddDays(5)
        };

        var result = _studentValidator.Validate(form);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(StudentForm.BirthDate));
    }

    [Fact(DisplayName = "DepartmentForm with whitespace or code under 2 characters fails validation")]
    public void DepartmentForm_WithInvalidCode_FailsValidation()
    {
        var form = new DepartmentForm
        {
            Name = "Invalid Department",
            Code = "C S"
        };

        var result = _departmentValidator.Validate(form);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DepartmentForm.Code));
    }

    [Fact(DisplayName = "StudentService rejects student creation with duplicate student code")]
    public async Task StudentService_WithDuplicateStudentCode_ReturnsFailure()
    {
        // Arrange
        var wrapperMock = new Mock<IRepositoryWrapper>();
        var departmentRepoMock = new Mock<IDepartmentRepository>();
        var studentRepoMock = new Mock<IStudentRepository>();

        departmentRepoMock.Setup(d => d.Get(1, null)).ReturnsAsync(new DepartmentResponse { Id = 1, Name = "CS" });
        studentRepoMock.Setup(s => s.IsDuplicateAsync("StudentCode", "STU-DUPLICATE", null)).ReturnsAsync(true);

        wrapperMock.Setup(w => w.Department).Returns(departmentRepoMock.Object);
        wrapperMock.Setup(w => w.Student).Returns(studentRepoMock.Object);

        var service = new StudentService(wrapperMock.Object);
        var request = new ServiceRequest<StudentForm>
        {
            Dto = new StudentForm
            {
                FullName = "Duplicate Student",
                StudentCode = "STU-DUPLICATE",
                DepartmentId = 1
            },
            UserId = 1
        };

        // Act
        var result = await service.Add(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.DuplicateStudentCode);
    }

    [Fact(DisplayName = "StudentService rejects student creation when target department does not exist")]
    public async Task StudentService_WithNonExistentDepartment_ReturnsFailure()
    {
        // Arrange
        var wrapperMock = new Mock<IRepositoryWrapper>();
        var departmentRepoMock = new Mock<IDepartmentRepository>();
        var studentRepoMock = new Mock<IStudentRepository>();

        departmentRepoMock.Setup(d => d.Get(999, null)).ReturnsAsync((DepartmentResponse?)null);

        wrapperMock.Setup(w => w.Department).Returns(departmentRepoMock.Object);
        wrapperMock.Setup(w => w.Student).Returns(studentRepoMock.Object);

        var service = new StudentService(wrapperMock.Object);
        var request = new ServiceRequest<StudentForm>
        {
            Dto = new StudentForm
            {
                FullName = "Missing Dept Student",
                StudentCode = "STU-VALID",
                DepartmentId = 999
            },
            UserId = 1
        };

        // Act
        var result = await service.Add(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Messages.DepartmentNotFound);
    }
}
