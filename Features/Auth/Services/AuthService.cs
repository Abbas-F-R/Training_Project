using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OC_System_Training.Features.AuditLogs.Repositories;
using OC_System_Training.Features.Auth.Dtos;
using OC_System_Training.Features.Auth.Repositories;
using OC_System_Training.Shared.Attributes;
using OC_System_Training.Shared.Base.dto;
using OC_System_Training.Shared.Constants;
using OC_System_Training.Shared.Utils;

namespace OC_System_Training.Features.Auth.Services;

/// <summary>
/// Authentication service providing credential verification, BCrypt password hashing,
/// atomic audit logging for authentication events, and signed JWT issuance.
/// </summary>
[Scoped]
public class AuthService(
    IUserRepository repository,
    IAuditLogRepository auditLogRepository,
    IConfiguration configuration) : IAuthService
{
    public async Task<ServiceResult<LoginResponse>> Login(LoginRequest request)
    {
        var userName = request.UserName.Trim();

        // 1. Locate user record by username
        var user = await repository.GetByUserName(userName);
        if (user == null)
        {
            // Record failed authentication attempt without exposing sensitive details
            await auditLogRepository.LogAsync(
                userId: null,
                action: "LOGIN_FAILED",
                entityName: "Auth",
                entityId: userName,
                changes: "{\"Reason\":\"InvalidCredentials\"}",
                isSuccess: false
            );
            return ServiceResult<LoginResponse>.Failure(Messages.InvalidCredentials);
        }

        // 2. Validate account active status
        if (!user.IsActive)
        {
            await auditLogRepository.LogAsync(
                userId: user.Id,
                action: "LOGIN_FAILED",
                entityName: "Auth",
                entityId: userName,
                changes: "{\"Reason\":\"UserInactive\"}",
                isSuccess: false
            );
            return ServiceResult<LoginResponse>.Failure(Messages.UserInactive);
        }

        // 3. Verify BCrypt hashed password
        if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            await auditLogRepository.LogAsync(
                userId: user.Id,
                action: "LOGIN_FAILED",
                entityName: "Auth",
                entityId: userName,
                changes: "{\"Reason\":\"InvalidCredentials\"}",
                isSuccess: false
            );
            return ServiceResult<LoginResponse>.Failure(Messages.InvalidCredentials);
        }

        // 4. Log successful authentication event to audit log
        await auditLogRepository.LogAsync(
            userId: user.Id,
            action: "LOGIN_SUCCESS",
            entityName: "Auth",
            entityId: userName,
            changes: $"{{\"Role\":\"{user.Role}\"}}",
            isSuccess: true
        );

        // 5. Generate signed JWT token and return response
        var response = GenerateJwtToken(user);
        return ServiceResult<LoginResponse>.Ok(response);
    }

    public async Task<ServiceResult<LoginResponse>> Register(RegisterRequest request, long? creatorId = null)
    {
        // 1. Validate username uniqueness
        if (await repository.IsUserNameTaken(request.UserName.Trim()))
            return ServiceResult<LoginResponse>.Failure(Messages.DuplicateRecord);

        // 2. Hash password with BCrypt and initialize user entity
        var newUser = new UserDto
        {
            FullName = request.FullName.Trim(),
            UserName = request.UserName.Trim(),
            PasswordHash = PasswordHasher.Hash(request.Password),
            Role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role.Trim(),
            IsActive = true
        };

        var created = await repository.Add(newUser, creatorId);
        if (created == null)
            return ServiceResult<LoginResponse>.Failure(Messages.InsertFailed);

        var response = GenerateJwtToken(created);
        return ServiceResult<LoginResponse>.Ok(response);
    }

    private LoginResponse GenerateJwtToken(UserDto user)
    {
        var secretKey = configuration["Jwt:SecretKey"]
                        ?? "SuperSecretKeyForStudentManagementSystem2026SecureMin32Bytes!";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddDays(7);

        var claims = new List<Claim>
        {
            new("UserId", user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("UserName", user.UserName),
            new(ClaimTypes.Name, user.UserName),
            new("FullName", user.FullName),
            new(ClaimTypes.Role, user.Role),
            new("Role", user.Role),
            new("Lang", "en")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            SigningCredentials = credentials,
            Issuer = "StudentManagementSystem",
            Audience = "StudentManagementSystemClients"
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return new LoginResponse
        {
            UserId = user.Id,
            UserName = user.UserName,
            FullName = user.FullName,
            Role = user.Role,
            Token = tokenString,
            ExpiresAt = expires
        };
    }
}
