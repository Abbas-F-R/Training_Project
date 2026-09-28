using OC_System_Training.Shared.Attributes;

namespace OC_System_Training.Features.Auth.Dtos;

/// <summary>
/// Data transfer object for successful login response containing JWT bearer token and user metadata.
/// </summary>
public class LoginResponse
{
    [Sqid]
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
