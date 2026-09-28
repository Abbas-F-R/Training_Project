# SQL Patterns for OC_System

All database interactions execute via SQL Server Stored Procedures. No raw SQL is written in C# repositories.

## 1. Table Schema
Standard auditing and soft-deletion columns must exist on every entity table:
```sql
CREATE TABLE [Entity]
(
    Id        BIGINT IDENTITY(1,1) PRIMARY KEY,
    -- Entity specific columns --
    IsDeleted BIT      DEFAULT 0 NOT NULL,
    CreatedBy BIGINT,
    CreatedAt DATETIME DEFAULT GETDATE() NOT NULL,
    UpdatedBy BIGINT,
    UpdatedAt DATETIME,

    CONSTRAINT FK_[Entity]_[OtherEntity] FOREIGN KEY (OtherEntityId) REFERENCES [OtherEntity](Id)
);
```

## 2. Mandatory View Pattern (`vw_{TableName}`)
Every table must have a corresponding view named `vw_{TableName}`.
- Joins foreign tables to provide readable names (e.g., `DepartmentName`).
- Filters out soft-deleted records (`WHERE IsDeleted = 0`).
```sql
CREATE VIEW vw_Students AS
SELECT
    s.Id,
    s.FullName,
    s.StudentCode,
    s.Email,
    s.PhoneNumber,
    s.DepartmentId,
    d.Name AS DepartmentName,
    s.Stage,
    s.BirthDate,
    s.IsDeleted,
    s.CreatedBy,
    s.CreatedAt,
    s.UpdatedBy,
    s.UpdatedAt
FROM Students s
LEFT JOIN Departments d ON d.Id = s.DepartmentId
WHERE s.IsDeleted = 0;
```

## 3. Standard Stored Procedures (The 5 CRUD Procs)

### 1. `[Entity]GetById`
```sql
CREATE PROCEDURE [Entity]GetById
    @Id BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_[Entity] WHERE Id = @Id;
END
```

### 2. `[Entity]GetAll` (Paged)
**Must return exactly two result sets:**
1. Total matching count (`SELECT COUNT(*) AS TotalCount`)
2. Paged data rows from `vw_[Entity]`
```sql
CREATE PROCEDURE [Entity]GetAll
    @PageNumber INT = 1,
    @PageSize   INT = 10,
    @Search     NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Result Set 1: Total Count
    SELECT COUNT(*) AS TotalCount
    FROM vw_[Entity]
    WHERE (@Search IS NULL OR FullName LIKE N'%' + @Search + N'%');

    -- Result Set 2: Paged Records
    SELECT * FROM vw_[Entity]
    WHERE (@Search IS NULL OR FullName LIKE N'%' + @Search + N'%')
    ORDER BY Id DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
```

### 3. `[Entity]GetAllNotPaged`
```sql
CREATE PROCEDURE [Entity]GetAllNotPaged
    @Search NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_[Entity]
    WHERE (@Search IS NULL OR FullName LIKE N'%' + @Search + N'%')
    ORDER BY FullName ASC;
END
```

### 4. `[Entity]Insert`
Inserts record, captures generated ID, and returns the new row via the view:
```sql
CREATE PROCEDURE [Entity]Insert
    @FullName      NVARCHAR(150),
    @StudentCode   NVARCHAR(50),
    @DepartmentId  BIGINT,
    @CreatedBy     BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NewId BIGINT;

    INSERT INTO [Entity] (FullName, StudentCode, DepartmentId, IsDeleted, CreatedBy, CreatedAt)
    VALUES (@FullName, @StudentCode, @DepartmentId, 0, @CreatedBy, GETDATE());

    SET @NewId = SCOPE_IDENTITY();

    SELECT * FROM vw_[Entity] WHERE Id = @NewId;
END
```

### 5. `[Entity]Update`
Updates record, updates audit timestamps, and returns updated row via view:
```sql
CREATE PROCEDURE [Entity]Update
    @Id            BIGINT,
    @FullName      NVARCHAR(150),
    @StudentCode   NVARCHAR(50),
    @DepartmentId  BIGINT,
    @UpdatedBy     BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [Entity]
    SET FullName     = @FullName,
        StudentCode  = @StudentCode,
        DepartmentId = @DepartmentId,
        UpdatedBy    = @UpdatedBy,
        UpdatedAt    = GETDATE()
    WHERE Id = @Id AND IsDeleted = 0;

    SELECT * FROM vw_[Entity] WHERE Id = @Id;
END
```

### 6. `[Entity]Delete` (Soft Delete)
```sql
CREATE PROCEDURE [Entity]Delete
    @Id     BIGINT,
    @UserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [Entity]
    SET IsDeleted = 1,
        UpdatedBy = @UserId,
        UpdatedAt = GETDATE()
    WHERE Id = @Id;

    SELECT CAST(1 AS BIT) AS Success;
END
```

## 4. Base Helper Stored Procedures
- `Base_CheckDuplicate`: Checks if a column value exists in a table, optionally excluding a record by ID.
- `Base_GetFirst`: Retrieves the first row matching a column name and value.
- Note: Foreign key validation (`Base_Exists`) was retired in favor of native SQL foreign keys which throw SQL 547.
