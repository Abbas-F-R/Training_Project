using Microsoft.AspNetCore.Mvc;

namespace OC_System_Training.Shared.Base;

/// <summary>
/// Foundation controller providing user identity context extraction, request building,
/// and standardized response envelope formatting with automatic error translation.
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    private ICurrentUser? _currentUser;
    protected ICurrentUser CurrentUser => _currentUser ??= HttpContext.RequestServices.GetRequiredService<ICurrentUser>();

    protected long Id => CurrentUser.UserId;
    protected string UserName => CurrentUser.UserName;
    protected string Role => CurrentUser.Role;
    protected string Lang => CurrentUser.Lang;

    /// <summary>
    /// Constructs a standardized ServiceRequest combining input DTO with user security context.
    /// </summary>
    protected ServiceRequest<T> CreateServiceRequest<T>(T dto) =>
        new(dto, Id, UserName, Role, Lang);

    /// <summary>
    /// Processes a single-item ServiceResult into 200 OK or 400 BadRequest with localized error message.
    /// </summary>
    protected ObjectResult Ok<T>(ServiceResult<T> result)
    {
        if (result.Error != null)
            return BadRequest(new { Message = result.Error.GetMessage(Lang) });

        return base.Ok(result.Data);
    }

    /// <summary>
    /// Processes a paginated ServiceResult into a standardized Response envelope including metadata.
    /// </summary>
    protected ObjectResult Ok<T>(ServiceResult<List<T>> result, int pageNumber = 1, int pageSize = 10)
    {
        if (result.Error != null)
            return BadRequest(new { Message = result.Error.GetMessage(Lang) });

        return base.Ok(new Response<T>(
            result.Data,
            pageNumber,
            result.TotalCount,
            pageSize
        ));
    }
}
