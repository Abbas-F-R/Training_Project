using Training_Project.Features.Auth.Dtos;

namespace Training_Project.Features.Auth.Services;

/// <summary>
/// Service contract for user authentication, session initiation, and account registration.
/// </summary>
public interface IAuthService
{
    Task<ServiceResult<LoginResponse>> Login(LoginRequest request);
    Task<ServiceResult<LoginResponse>> Register(RegisterRequest request, long? creatorId = null);
}
