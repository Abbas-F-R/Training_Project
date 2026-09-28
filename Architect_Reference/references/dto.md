# DTO Patterns for OC_System

Every feature defines a consistent set of DTOs within its `Dtos/` folder.

## 1. Form (POST Request)
Used for creating new entities. Contains only the properties required for insertion.
```csharp
public class StudentForm
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string StudentCode { get; set; } = string.Empty;

    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    [Sqid]
    public long DepartmentId { get; set; }

    public int Stage { get; set; }
    public DateTime? BirthDate { get; set; }
}
```

## 2. Update (PUT Request)
Used for modifying existing records. Excludes immutable fields (e.g., `CreatedBy`, `CreatedAt`).
```csharp
public class StudentUpdate
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string StudentCode { get; set; } = string.Empty;

    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    [Sqid]
    public long DepartmentId { get; set; }

    public int Stage { get; set; }
    public DateTime? BirthDate { get; set; }
}
```

## 3. Filter (GET Query Parameters)
Used for search and pagination. **Must inherit from `BaseFilter`**.
```csharp
public class StudentFilter : BaseFilter
{
    public string? Search { get; set; }
    public long? DepartmentId { get; set; }
    public int? Stage { get; set; }
}
```
`BaseFilter` provides `PageNumber` (default 1) and `PageSize` (default 10).

## 4. Response (Public Output DTO)
Returned to clients. Uses `[Sqid]` on entity IDs so raw database IDs are not exposed.
```csharp
public class StudentResponse
{
    [Sqid]
    public long Id { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    [Sqid]
    public long DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;

    public int Stage { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

## 5. Key Attributes

### `[Sqid]`
Enables URL/JSON ID obfuscation:
- Encodes `long` IDs into clean alphanumeric strings when serializing JSON responses.
- Decodes incoming strings back to `long` for model binding and request bodies.

### `[IgnoreParameter]`
Tells `BaseRepository.GetDynamicParameters()` to skip this property when creating parameters for Dapper stored procedure calls (useful for navigation or computed properties).
