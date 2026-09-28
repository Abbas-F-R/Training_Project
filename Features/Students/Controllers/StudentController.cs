using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training_Project.Features.Students.Dtos;
using Training_Project.Features.Students.Services;
using Training_Project.Shared.Base;
using Training_Project.Shared.Base.dto;

namespace Training_Project.Features.Students.Controllers;

/// <summary>
/// Controller for student management operations.
/// Read and write operations are permitted for authenticated users; deletion requires Admin privileges.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StudentController(IStudentService service)
    : GenericController<StudentResponse, StudentForm, StudentUpdate, StudentFilter>(service)
{
    /// <summary>Retrieves a single student record by primary key.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<StudentResponse>> Get(long id) => 
        await BaseGet(id);

    /// <summary>Retrieves a paginated and filtered list of students.</summary>
    [HttpGet]
    public async Task<ActionResult<Response<StudentResponse>>> GetAll([FromQuery] StudentFilter filter) => 
        await BaseGetAll(filter);

    /// <summary>Creates a new student record.</summary>
    [HttpPost]
    public async Task<ActionResult<StudentResponse>> Add([FromBody] StudentForm form) => 
        await BaseAdd(form);

    /// <summary>Updates an existing student record.</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<StudentResponse>> Update(long id, [FromBody] StudentUpdate update) => 
        await BaseUpdate(id, update);

    /// <summary>Soft deletes a student record (Admin role required).</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<bool>> Delete(long id) => 
        await BaseDelete(id);
}
