using OC_System_Training.Features.Auth.Dtos;

namespace OC_System_Training.Features.Auth.Services;

// تعليق تدريبي: واجهة خدمة تسجيل الدخول وإدارة الهوية
public interface IAuthService
{
    Task<ServiceResult<LoginResponse>> Login(LoginRequest request);
    Task<ServiceResult<LoginResponse>> Register(RegisterRequest request, long? creatorId = null);
}
