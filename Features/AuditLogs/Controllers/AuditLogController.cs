using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OC_System_Training.Features.AuditLogs.Dtos;
using OC_System_Training.Features.AuditLogs.Services;
using OC_System_Training.Shared.Base;
using OC_System_Training.Shared.Base.dto;

namespace OC_System_Training.Features.AuditLogs.Controllers;

// تعليق تدريبي: متحكم سجل التدقيق والتتبع (AuditLogController)
// يخضع لصلاحية محددة: مقتصر حصراً على مدراء النظام [Authorize(Roles = "Admin")]
// يرجع 403 Forbidden للمستخدمين العاديين، و 401 Unauthorized لغير المسجلين
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AuditLogController(IAuditLogService service) : BaseController
{
    /// <summary>
    /// استعراض سجلات التدقيق والتتبع بالنظام (خاص بالمسؤول Admin فقط)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Response<AuditLogResponse>>> GetAll([FromQuery] AuditLogFilter filter) =>
        Ok(await service.GetAll(CreateServiceRequest(filter)), filter.PageNumber, filter.PageSize);
}
