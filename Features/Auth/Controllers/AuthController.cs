using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OC_System_Training.Features.Auth.Dtos;
using OC_System_Training.Features.Auth.Services;

namespace OC_System_Training.Features.Auth.Controllers;

// تعليق تدريبي: متحكم المصادقة وتسجيل الدخول (AuthController)
// يوفر نقطة تسجيل الدخول ومعرفة بيانات المستخدم الحالي
[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : BaseController
{
    /// <summary>
    /// تسجيل الدخول والحصول على JWT Token
    /// </summary>
    [HttpPost("Login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request) =>
        Ok(await authService.Login(request));

    /// <summary>
    /// إنشاء حساب مستخدم جديد (خاص بالمسؤول Admin فقط)
    /// </summary>
    [HttpPost("Register")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request) =>
        Ok(await authService.Register(request, Id));

    /// <summary>
    /// جلب معلومات المستخدم المتصل حالياً من الـ Token
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
