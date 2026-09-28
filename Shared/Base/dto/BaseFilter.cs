namespace OC_System_Training.Shared.Base.dto;

/// <summary>
/// Base class for query filters establishing standardized pagination parameters.
/// </summary>
public class BaseFilter
{
    /// <summary>Number of items per page (default: 10, max: 100).</summary>
    public int PageSize { get; set; } = 10;

    /// <summary>Target page number (1-indexed).</summary>
    public int PageNumber { get; set; } = 1;
}
