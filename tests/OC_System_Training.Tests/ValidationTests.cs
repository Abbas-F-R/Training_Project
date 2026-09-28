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

    [Fact(DisplayName = "1. نموذج طالب ببيانات مكتملة وصحيحة يجتاز التحقق")]
    public void StudentForm_WithValidData_PassesValidation()
    {
        var form = new StudentForm
        {
            FullName = "كرار حيدر جاسم",
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

    [Fact(DisplayName = "2. نموذج طالب بدون اسم أو برقم جامعي فارغ يفشل في التحقق")]
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

    [Fact(DisplayName = "3. المرحلة الدراسية خارج النطاق 1-6 تفشل في التحقق")]
    public void StudentForm_WithInvalidStage_FailsValidation()
    {
        var form = new StudentForm
        {
            FullName = "حيدر جواد",
            StudentCode = "STU-999",
            DepartmentId = 1,
            Stage = 7 // خارج النطاق
        };

        var result = _studentValidator.Validate(form);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(StudentForm.Stage));
    }

    [Fact(DisplayName = "4. تاريخ ميلاد الطالب في المستقبل يفشل في التحقق")]
    public void StudentForm_WithFutureBirthDate_FailsValidation()
    {
        var form = new StudentForm
        {
            FullName = "زينب علي",
            StudentCode = "STU-100",
            DepartmentId = 1,
            BirthDate = DateTime.Today.AddDays(5) // تاريخ مستقبلي
        };

        var result = _studentValidator.Validate(form);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(StudentForm.BirthDate));
    }

    [Fact(DisplayName = "5. رمز القسم إذا احتوى على مسافات أو كان أقل من حرفين يفشل في التحقق")]
    public void DepartmentForm_WithInvalidCode_FailsValidation()
    {
        var form = new DepartmentForm
        {
            Name = "قسم غير صالح",
            Code = "C S" // يحتوي مسافة
        };

        var result = _departmentValidator.Validate(form);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DepartmentForm.Code));
    }

    [Fact(DisplayName = "6. إضافة طالب برقم جامعي مكرر تفشل على مستوى منطق العمل في الخدمة")]
    public async Task StudentService_WithDuplicateStudentCode_ReturnsFailure()
    {
        // Arrange
        var wrapperMock = new Mock<IRepositoryWrapper>();
        var departmentRepoMock = new Mock<IDepartmentRepository>();
        var studentRepoMock = new Mock<IStudentRepository>();

        // القسم موجود
        departmentRepoMock.Setup(d => d.Get(1, null)).ReturnsAsync(new DepartmentResponse { Id = 1, Name = "CS" });
        // الكود مكرر
        studentRepoMock.Setup(s => s.IsDuplicateAsync("StudentCode", "STU-DUPLICATE", null)).ReturnsAsync(true);

        wrapperMock.Setup(w => w.Department).Returns(departmentRepoMock.Object);
        wrapperMock.Setup(w => w.Student).Returns(studentRepoMock.Object);

        var service = new StudentService(wrapperMock.Object);
        var request = new ServiceRequest<StudentForm>
        {
            Dto = new StudentForm
            {
                FullName = "طالب مكرر",
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

    [Fact(DisplayName = "7. إضافة طالب لقسم دراسي غير موجود تفشل في الخدمة مع إرجاع DepartmentNotFound")]
    public async Task StudentService_WithNonExistentDepartment_ReturnsFailure()
    {
        // Arrange
        var wrapperMock = new Mock<IRepositoryWrapper>();
        var departmentRepoMock = new Mock<IDepartmentRepository>();
        var studentRepoMock = new Mock<IStudentRepository>();

        // القسم غير موجود
        departmentRepoMock.Setup(d => d.Get(999, null)).ReturnsAsync((DepartmentResponse?)null);

        wrapperMock.Setup(w => w.Department).Returns(departmentRepoMock.Object);
        wrapperMock.Setup(w => w.Student).Returns(studentRepoMock.Object);

        var service = new StudentService(wrapperMock.Object);
        var request = new ServiceRequest<StudentForm>
        {
            Dto = new StudentForm
            {
                FullName = "طالب لقسم مفقود",
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
