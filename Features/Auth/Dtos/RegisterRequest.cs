namespace Training_Project.Features.Auth.Dtos;

/// <summary>
/// Data transfer object for administrative user account registration.
/// </summary>
public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
}
