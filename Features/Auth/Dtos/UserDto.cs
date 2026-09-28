namespace OC_System_Training.Features.Auth.Dtos;

/// <summary>
/// Internal DTO representing user account entity retrieved from database queries.
/// </summary>
public class UserDto
{
    public long Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}
