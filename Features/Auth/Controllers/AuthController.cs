using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OC_System_Training.Features.Auth.Dtos;
using OC_System_Training.Features.Auth.Services;

namespace OC_System_Training.Features.Auth.Controllers;

/// <summary>
/// Authentication controller handling login, token generation, user registration, and identity resolution.
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

    /// <summary>
    /// Returns the identity claims of the currently authenticated user from the JWT token.
    /// </summary>
    [HttpGet("Me")]
    [Authorize]
    public ActionResult<object> GetMe() =>
        base.Ok(new
        {
            UserId = CurrentUser.UserId,
            UserName = CurrentUser.UserName,
            FullName = CurrentUser.FullName,
            Role = CurrentUser.Role,
            Lang = CurrentUser.Lang,
            IsAuthenticated = CurrentUser.IsAuthenticated
        });
}
