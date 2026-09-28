namespace OC_System_Training.Infrastructure.Persistence.Repositories.BaseRepository;

/// <summary>
/// Generic repository interface defining standard CRUD data-access contracts via Stored Procedures.
/// </summary>
public interface IBaseRepository<TView, TForm, TUpdate, TFilter>
{
    /// <summary>Retrieves the first entity matching a specified column value.</summary>
    Task<TView?> GetFirstAsync(string columnName, object value, string? viewName = null);

    /// <summary>Retrieves a single entity by its primary key.</summary>
    Task<TView?> Get(long id, string? procedureName = null);

    /// <summary>Retrieves a paginated list of entities along with the total count matching the filter.</summary>
    Task<(List<TView>? data, int totalCount)> GetAll(TFilter filter, string? procedureName = null);

    /// <summary>Inserts a new record and returns the resulting entity from the view.</summary>
    Task<TView?> Add(TForm form, long userId, string? procedureName = null);

    /// <summary>Updates an existing record and returns the updated entity from the view.</summary>
    Task<TView?> Update(long id, TUpdate update, long userId, string? procedureName = null);

    /// <summary>Performs a soft delete on a record by ID.</summary>
    Task<bool> Delete(long id, long userId, string? procedureName = null);

    /// <summary>Checks whether a column value already exists in the table, optionally excluding a record by ID.</summary>
    Task<bool> IsDuplicateAsync(string columnName, object value, long? excludeId = null);
}
