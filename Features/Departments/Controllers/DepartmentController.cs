using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OC_System_Training.Features.Departments.Dtos;
using OC_System_Training.Features.Departments.Services;
using OC_System_Training.Shared.Base;
using OC_System_Training.Shared.Base.dto;

namespace OC_System_Training.Features.Departments.Controllers;

/// <summary>
/// Controller for academic department management.
/// Lookup and retrieval endpoints are open to all authenticated users; modifications require Admin privileges.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DepartmentController(IDepartmentService service)
    : GenericController<DepartmentResponse, DepartmentForm, DepartmentUpdate, DepartmentFilter>(service)
{
    /// <summary>Retrieves a single department record by primary key.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DepartmentResponse>> Get(long id) => 
        await BaseGet(id);

    /// <summary>Retrieves a paginated and filtered list of departments.</summary>
    [HttpGet]
    public async Task<ActionResult<Response<DepartmentResponse>>> GetAll([FromQuery] DepartmentFilter filter) => 
        await BaseGetAll(filter);

    /// <summary>Retrieves a lookup list of all active departments for dropdown selection.</summary>
    [HttpGet("lookup")]
    public async Task<ActionResult<List<DepartmentResponse>>> Lookup() => 
        Ok(await service.Lookup());

    /// <summary>Creates a new department (Admin role required).</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DepartmentResponse>> Add([FromBody] DepartmentForm form) => 
        await BaseAdd(form);

    /// <summary>Updates an existing department (Admin role required).</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DepartmentResponse>> Update(long id, [FromBody] DepartmentUpdate update) => 
        await BaseUpdate(id, update);

    /// <summary>Soft deletes a department record (Admin role required).</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<bool>> Delete(long id) => 
        await BaseDelete(id);
}
