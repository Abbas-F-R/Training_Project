# Controller Patterns for OC_System

OC_System standardizes API handling through a two-tier hierarchy:
1. `BaseController`: Provides actor claims/context, `CurrentUser` injection, language handling, and `Ok()` result unwrappers.
2. `GenericController<TView, TForm, TUpdate, TFilter>`: Auto-wires standard REST CRUD endpoints.

## 1. Response Wrapper (`Response<T>`)
Every paginated response adheres to this JSON structure:
```json
{
  "data": [...],
  "pagesCount": 5,
  "currentPage": 1,
  "totalCount": 50,
  "isLast": false
}
```

## 2. BaseController
BaseController derives from `ControllerBase` and provides:
- User context properties: `Id` (User ID), `UserName`, `Role`, `Lang`.
- `CreateServiceRequest<T>(T dto)`: Automatically bundles user identity into the request.
- `Ok(ServiceResult<T>)`: Returns `200 OK` with data or `400 BadRequest` with translated error message.
- `Ok(ServiceResult<List<T>>, pageNumber, pageSize)`: Wraps list into `Response<T>`.

## 3. GenericController (Rapid CRUD)
```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StudentController(IStudentService service)
    : GenericController<StudentResponse, StudentForm, StudentUpdate, StudentFilter>(service)
{
    [HttpGet("{id}")]
    public async Task<ActionResult<StudentResponse>> Get(long id) => await BaseGet(id);

    [HttpGet]
    public async Task<ActionResult<Response<StudentResponse>>> GetAll([FromQuery] StudentFilter filter)
        => await BaseGetAll(filter);

    [HttpGet("NotPaged")]
    public async Task<ActionResult<List<StudentResponse>>> GetAllNotPaged([FromQuery] StudentFilter filter)
        => await BaseGetAllNotPaged(filter);

    [HttpPost]
    public async Task<ActionResult<StudentResponse>> Add([FromBody] StudentForm form)
        => await BaseAdd(form);

    [HttpPut("{id}")]
    public async Task<ActionResult<StudentResponse>> Update(long id, [FromBody] StudentUpdate update)
        => await BaseUpdate(id, update);

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> Delete(long id) => await BaseDelete(id);
}
```

## 4. Key Rules
- Apply `[Authorize]` at the controller level unless anonymous access is explicitly required (e.g., login).
- Leverage `GenericController` endpoints to avoid rewriting repetitive CRUD boilerplate.
- Expose feature-specific endpoints as standard actions calling custom service methods.
