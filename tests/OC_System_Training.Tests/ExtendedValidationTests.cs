using FluentAssertions;
using OC_System_Training.Features.Auth.Dtos;
using OC_System_Training.Features.Auth.Validators;
using OC_System_Training.Features.Departments.Dtos;
using OC_System_Training.Features.Departments.Validators;
using OC_System_Training.Features.Students.Dtos;
using OC_System_Training.Features.Students.Validators;
using Xunit;

namespace OC_System_Training.Tests;

public class ExtendedValidationTests
{
    private readonly StudentUpdateValidator _studentUpdateValidator = new();
    private readonly StudentFilterValidator _studentFilterValidator = new();
    private readonly DepartmentUpdateValidator _deptUpdateValidator = new();
    private readonly LoginRequestValidator _loginValidator = new();
    private readonly RegisterRequestValidator _registerValidator = new();

    [Fact(DisplayName = "StudentUpdateValidator passes with complete valid update data")]
    public void StudentUpdateValidator_WithValidData_Passes()
    {
        var model = new StudentUpdate
        {
            FullName = "Robert Martin",
            StudentCode = "STU-2026-042",
            DepartmentId = 2,
            Stage = 4,
            Email = "robert@clean-code.com",
            PhoneNumber = "07709876543",
            BirthDate = new DateTime(2002, 1, 15)
        };

        var result = _studentUpdateValidator.Validate(model);
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "StudentUpdateValidator fails when FullName or StudentCode is empty")]
    public void StudentUpdateValidator_WithEmptyFields_Fails()
    {
        var model = new StudentUpdate
        {
            FullName = "",
            StudentCode = "",
            DepartmentId = 0,
            Stage = 10
        };

        var result = _studentUpdateValidator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(StudentUpdate.FullName));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(StudentUpdate.StudentCode));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(StudentUpdate.DepartmentId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(StudentUpdate.Stage));
    }

    [Fact(DisplayName = "StudentFilterValidator accepts valid pagination and stage")]
    public void StudentFilterValidator_WithValidBounds_Passes()
    {
        var filter = new StudentFilter { PageNumber = 2, PageSize = 25, Stage = 3 };
        var result = _studentFilterValidator.Validate(filter);
        result.IsValid.Should().BeTrue();
    }

    [Theory(DisplayName = "StudentFilterValidator rejects invalid pagination or stage values")]
    [InlineData(0, 10, null)]
    [InlineData(1, 0, null)]
    [InlineData(1, 150, null)]
    [InlineData(1, 10, 7)]
    [InlineData(1, 10, 0)]
    public void StudentFilterValidator_WithInvalidValues_Fails(int pageNumber, int pageSize, int? stage)
    {
        var filter = new StudentFilter { PageNumber = pageNumber, PageSize = pageSize, Stage = stage };
        var result = _studentFilterValidator.Validate(filter);
        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "DepartmentUpdateValidator passes with valid name and uppercase code")]
    public void DepartmentUpdateValidator_WithValidData_Passes()
    {
        var model = new DepartmentUpdate { Name = "Information Systems", Code = "IS" };
        var result = _deptUpdateValidator.Validate(model);
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "DepartmentUpdateValidator rejects invalid code format")]
    public void DepartmentUpdateValidator_WithInvalidCode_Fails()
    {
        var model = new DepartmentUpdate { Name = "Info Systems", Code = "i-s lowercase" };
        var result = _deptUpdateValidator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DepartmentUpdate.Code));
    }

    [Fact(DisplayName = "LoginRequestValidator passes when credentials are provided")]
    public void LoginRequestValidator_WithValidCredentials_Passes()
    {
        var request = new LoginRequest { UserName = "admin", Password = "Password123!" };
        var result = _loginValidator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "LoginRequestValidator fails when username or password is missing or short")]
    public void LoginRequestValidator_WithEmptyCredentials_Fails()
    {
        var request = new LoginRequest { UserName = "", Password = "123" };
        var result = _loginValidator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginRequest.UserName));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginRequest.Password));
    }

    [Fact(DisplayName = "RegisterRequestValidator passes with valid registration payload")]
    public void RegisterRequestValidator_WithValidData_Passes()
    {
        var request = new RegisterRequest
        {
            FullName = "Jane Developer",
            UserName = "jane.dev",
            Password = "SecurePassword123!",
            Role = "Admin"
        };
        var result = _registerValidator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "RegisterRequestValidator fails when username contains invalid characters or role is unknown")]
    public void RegisterRequestValidator_WithInvalidCharacters_Fails()
    {
        var request = new RegisterRequest
        {
            FullName = "Jane",
            UserName = "jane@#invalid",
            Password = "123",
            Role = "SuperHacker"
        };
        var result = _registerValidator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.UserName));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Role));
    }
}
