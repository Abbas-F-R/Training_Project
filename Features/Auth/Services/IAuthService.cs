using OC_System_Training.Features.Auth.Dtos;

namespace OC_System_Training.Features.Auth.Services;

/// <summary>
/// Service contract for user authentication, session initiation, and account registration.
/// </summary>
public interface IAuthService
{
    Task<ServiceResult<LoginResponse>> Login(LoginRequest request);
    Task<ServiceResult<LoginResponse>> Register(RegisterRequest request, long? creatorId = null);
}
