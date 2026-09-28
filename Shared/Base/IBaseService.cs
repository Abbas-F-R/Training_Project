namespace OC_System_Training.Shared.Base;

/// <summary>
/// Generic service contract defining CRUD operations.
/// Default interface methods allow selective feature implementation by raising NotSupportedException for unsupported actions.
/// </summary>
public interface IBaseService<TView, TForm, TUpdate, TFilter>
{
    /// <summary>Retrieves a single entity by primary key.</summary>
    Task<ServiceResult<TView>> Get(ServiceRequest<long> request) =>
        throw new NotSupportedException("Get is not supported for this service");

    /// <summary>Retrieves a paginated list of entities matching the filter.</summary>
    Task<ServiceResult<List<TView>>> GetAll(ServiceRequest<TFilter> request) =>
        throw new NotSupportedException("GetAll is not supported for this service");

    /// <summary>Creates a new entity from input form DTO.</summary>
    Task<ServiceResult<TView>> Add(ServiceRequest<TForm> request) =>
        throw new NotSupportedException("Add is not supported for this service");

    /// <summary>Updates an existing entity by primary key.</summary>
    Task<ServiceResult<TView>> Update(long id, ServiceRequest<TUpdate> request) =>
        throw new NotSupportedException("Update is not supported for this service");

    /// <summary>Soft deletes an entity by primary key.</summary>
    Task<ServiceResult<bool>> Delete(ServiceRequest<long> request) =>
        throw new NotSupportedException("Delete is not supported for this service");
}
