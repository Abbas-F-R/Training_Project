using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training_Project.Features.AuditLogs.Dtos;
using Training_Project.Features.AuditLogs.Services;
using Training_Project.Shared.Base;
using Training_Project.Shared.Base.dto;

namespace Training_Project.Features.AuditLogs.Controllers;

/// <summary>
/// Controller for viewing immutable audit trail entries (restricted exclusively to Admin role).
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AuditLogController(IAuditLogService service) : BaseController
{
    /// <summary>
    /// Retrieves paginated audit log entries with optional filtering by entity, action, or date.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Response<AuditLogResponse>>> GetAll([FromQuery] AuditLogFilter filter) =>
        Ok(await service.GetAll(CreateServiceRequest(filter)), filter.PageNumber, filter.PageSize);
}
