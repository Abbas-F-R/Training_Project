using System.ComponentModel.DataAnnotations;

namespace OC_System_Training.Shared.Base.dto;

/// <summary>
/// Base class for query filters establishing standardized pagination parameters.
/// </summary>
public class BaseFilter
{
    /// <summary>Number of items per page (default: 10, max: 100).</summary>
    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 10;

    /// <summary>Target page number (1-indexed).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be at least 1.")]
    public int PageNumber { get; set; } = 1;
}
