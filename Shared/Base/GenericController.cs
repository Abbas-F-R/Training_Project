using Microsoft.AspNetCore.Mvc;

namespace Training_Project.Shared.Base;

/// <summary>
/// Generic base controller encapsulating boilerplate HTTP CRUD endpoint delegation to IBaseService.
/// </summary>
public abstract class GenericController<TView, TForm, TUpdate, TFilter>(
    IBaseService<TView, TForm, TUpdate, TFilter> service) : BaseController
{
    protected readonly IBaseService<TView, TForm, TUpdate, TFilter> Service = service;

    /// <summary>Retrieves a single entity by primary key.</summary>
    protected async Task<ActionResult<TView>> BaseGet(long id) => 
        Ok(await Service.Get(CreateServiceRequest(id)));

    /// <summary>Retrieves a paginated list of entities matching the filter criteria.</summary>
    protected async Task<ActionResult<Response<TView>>> BaseGetAll([FromQuery] TFilter filter) =>
        Ok(await Service.GetAll(CreateServiceRequest(filter)), (filter as BaseFilter)?.PageNumber ?? 1, (filter as BaseFilter)?.PageSize ?? 10);

    /// <summary>Creates a new entity from input form DTO.</summary>
    protected async Task<ActionResult<TView>> BaseAdd([FromBody] TForm form) =>
        Ok(await Service.Add(CreateServiceRequest(form)));

    /// <summary>Updates an existing entity by primary key.</summary>
    protected async Task<ActionResult<TView>> BaseUpdate(long id, [FromBody] TUpdate update) =>
        Ok(await Service.Update(id, CreateServiceRequest(update)));

    /// <summary>Deletes an entity by primary key.</summary>
    protected async Task<ActionResult<bool>> BaseDelete(long id) => 
        Ok(await Service.Delete(CreateServiceRequest(id)));
}
