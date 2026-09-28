using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training_Project.Features.Auth.Dtos;
using Training_Project.Features.Auth.Services;

namespace Training_Project.Features.Auth.Controllers;

/// <summary>
/// Authentication controller handling login, token generation, and user account provisioning.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : BaseController
{
    /// <summary>
    /// Authenticates user credentials and issues a signed JWT Bearer token.
    /// </summary>
    [HttpPost("Login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request) =>
        Ok(await authService.Login(request));

    /// <summary>
    /// Registers a new user account (restricted to Admin role).
    /// </summary>
    [HttpPost("Register")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request) =>
        Ok(await authService.Register(request, Id));
}
