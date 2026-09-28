using System.ComponentModel.DataAnnotations;

namespace OC_System_Training.Features.Auth.Dtos;

/// <summary>
/// Data transfer object for user login credentials.
/// </summary>
public class LoginRequest
{
    [Required(ErrorMessage = "Username is required.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}
