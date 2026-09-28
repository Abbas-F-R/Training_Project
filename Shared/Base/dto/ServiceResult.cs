namespace OC_System_Training.Shared.Base.dto;

/// <summary>
/// Result envelope unifying operational outcomes, payload, pagination metadata, and error codes.
/// </summary>
public class ServiceResult<T>
{
    public T? Data { get; set; }
    public int TotalCount { get; set; }
    public string? Error { get; set; }
    public bool Success => Error == null;
    public bool IsSuccess => Success;

    public ServiceResult()
    {
    }

    public ServiceResult(T? data)
    {
        Data = data;
    }

    public ServiceResult(T? data, int totalCount)
    {
        Data = data;
        TotalCount = totalCount;
    }

    public ServiceResult(string? error)
    {
        Error = error;
    }

    public static ServiceResult<T> Ok(T? data) => new(data);
    public static ServiceResult<T> PagedOk(T? data, int totalCount) => new(data, totalCount);
    public static ServiceResult<T> Failure(string error) => new(error);
}
