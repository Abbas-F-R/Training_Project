namespace Training_Project.Features.Auth.Dtos;

/// <summary>
/// Data transfer object for user login credentials.
/// </summary>
public class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
