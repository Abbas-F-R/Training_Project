-- ============================================================================
-- 03_Procedures.sql
-- Student Management System Stored Procedures
-- High-Performance CRUD Operations with Atomic In-Transaction Auditing
-- ============================================================================

USE [Training_Project_DB];
GO

-- ============================================================================
-- 1. Ø§Ù„Ø¥Ø¬Ø±Ø§Ø¡Ø§Øª Ø§Ù„Ù…Ø³Ø§Ø¹Ø¯Ø© Ù„Ù„Ù†Ø¸Ø§Ù… Ø§Ù„Ø£Ø³Ø§Ø³ÙŠ (Base Helper Procedures)
-- ============================================================================

-- ÙØ­Øµ ØªÙƒØ±Ø§Ø± Ù‚ÙŠÙ…Ø© ÙÙŠ Ø¹Ù…ÙˆØ¯ Ù…Ø¹ÙŠÙ† (ØªØ³ØªØ®Ø¯Ù…Ù‡ IsDuplicateAsync)
CREATE OR ALTER PROCEDURE Base_CheckDuplicate
    @TableName NVARCHAR(100),
    @ColumnName NVARCHAR(100),
    @Value NVARCHAR(255),
    @ExcludeId BIGINT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @Count INT;

    SET @SQL = N'SELECT @CountOut = COUNT(*) FROM [' + @TableName + N'] 
                 WHERE [' + @ColumnName + N'] = @Val 
                   AND IsDeleted = 0 
                   AND (@ExId IS NULL OR Id <> @ExId)';

    EXEC sp_executesql @SQL, 
        N'@Val NVARCHAR(255), @ExId BIGINT, @CountOut INT OUTPUT', 
        @Val = @Value, @ExId = @ExcludeId, @CountOut = @Count OUTPUT;

    SELECT ISNULL(@Count, 0);
END
GO

-- Ø¬Ù„Ø¨ Ø£ÙˆÙ„ Ø³Ø¬Ù„ ÙŠØ·Ø§Ø¨Ù‚ Ù‚ÙŠÙ…Ø© Ø¹Ù…ÙˆØ¯ (ØªØ³ØªØ®Ø¯Ù…Ù‡ GetFirstAsync)
CREATE OR ALTER PROCEDURE Base_GetFirst
    @TableName NVARCHAR(100),
    @ColumnName NVARCHAR(100),
    @Value NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @SQL NVARCHAR(MAX);

    SET @SQL = N'SELECT TOP 1 * FROM [' + @TableName + N'] 
                 WHERE [' + @ColumnName + N'] = @Val';

    EXEC sp_executesql @SQL, N'@Val NVARCHAR(255)', @Val = @Value;
END
GO

-- ============================================================================
-- 2. Ø¥Ø¬Ø±Ø§Ø¡Ø§Øª Ø³Ø¬Ù„ Ø§Ù„ØªØ¯Ù‚ÙŠÙ‚ ÙˆØ§Ù„ØªØªØ¨Ø¹ (AuditLogs Procedures)
-- ============================================================================

-- Ø¥Ø¶Ø§ÙØ© Ø³Ø¬Ù„ ØªØ¯Ù‚ÙŠÙ‚ Ø¬Ø¯ÙŠØ¯ (ØªØ³ØªØ®Ø¯Ù…Ù‡Ø§ Ø§Ù„Ø®Ø¯Ù…Ø§Øª ÙˆØªØ¯Ù‚ÙŠÙ‚ Ø§Ù„Ø¯Ø®ÙˆÙ„)
CREATE OR ALTER PROCEDURE AuditLogsInsert
    @UserId     BIGINT        = NULL,
    @Action     NVARCHAR(50),
    @EntityName NVARCHAR(100),
    @EntityId   NVARCHAR(100) = NULL,
    @Changes    NVARCHAR(MAX) = NULL,
    @IpAddress  NVARCHAR(50)  = NULL,
    @UserAgent  NVARCHAR(255) = NULL,
    @IsSuccess  BIT           = 1
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NewId BIGINT;

    INSERT INTO AuditLogs (UserId, Action, EntityName, EntityId, Changes, IpAddress, UserAgent, IsSuccess, CreatedAt)
    VALUES (@UserId, @Action, @EntityName, @EntityId, @Changes, @IpAddress, @UserAgent, @IsSuccess, GETDATE());

    SET @NewId = SCOPE_IDENTITY();

    SELECT 
        Id,
        UserId,
        Action,
        EntityName,
        EntityId,
        Changes,
        IpAddress,
        UserAgent,
        IsSuccess,
        CreatedAt
    FROM AuditLogs 
    WHERE Id = @NewId;
END
GO

-- Ø§Ø³ØªØ¹Ø±Ø§Ø¶ Ø³Ø¬Ù„Ø§Øª Ø§Ù„ØªØ¯Ù‚ÙŠÙ‚ Ø¨Ù†Ø¸Ø§Ù… Ø§Ù„ØµÙØ­Ø§Øª Ù…Ø¹ Ø§Ù„ÙÙ„ØªØ±Ø© (Ø®Ø§ØµØ© Ø¨Ø§Ù„Ù…Ø³Ø¤ÙˆÙ„ Admin)
CREATE OR ALTER PROCEDURE AuditLogsGetAll
    @PageNumber INT           = 1,
    @PageSize   INT           = 20,
    @EntityName NVARCHAR(100) = NULL,
    @Action     NVARCHAR(50)  = NULL,
    @UserId     BIGINT        = NULL,
    @FromDate   DATETIME      = NULL,
    @ToDate     DATETIME      = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Ø§Ù„Ù†ØªÙŠØ¬Ø© 1: Ø¥Ø¬Ù…Ø§Ù„ÙŠ Ø¹Ø¯Ø¯ Ø§Ù„Ø³Ø¬Ù„Ø§Øª Ø§Ù„Ù…Ø·Ø§Ø¨Ù‚Ø©
    SELECT COUNT(*) AS TotalCount
    FROM AuditLogs
    WHERE (@EntityName IS NULL OR EntityName = @EntityName)
      AND (@Action IS NULL OR Action = @Action)
      AND (@UserId IS NULL OR UserId = @UserId)
      AND (@FromDate IS NULL OR CreatedAt >= @FromDate)
      AND (@ToDate IS NULL OR CreatedAt <= @ToDate);

    -- Ø§Ù„Ù†ØªÙŠØ¬Ø© 2: Ø¨ÙŠØ§Ù†Ø§Øª Ø§Ù„ØµÙØ­Ø© Ø§Ù„Ù…Ø·Ù„ÙˆØ¨Ø©
    SELECT 
        Id,
        UserId,
        Action,
        EntityName,
        EntityId,
        Changes,
        IpAddress,
        UserAgent,
        IsSuccess,
        CreatedAt
    FROM AuditLogs
    WHERE (@EntityName IS NULL OR EntityName = @EntityName)
      AND (@Action IS NULL OR Action = @Action)
      AND (@UserId IS NULL OR UserId = @UserId)
      AND (@FromDate IS NULL OR CreatedAt >= @FromDate)
      AND (@ToDate IS NULL OR CreatedAt <= @ToDate)
    ORDER BY Id DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ============================================================================
-- 3. Ø¥Ø¬Ø±Ø§Ø¡Ø§Øª Ø§Ù„Ø£Ù‚Ø³Ø§Ù… Ø§Ù„Ø¯Ø±Ø§Ø³ÙŠØ© (Departments Stored Procedures)
-- ============================================================================

-- Ø¬Ù„Ø¨ Ù‚Ø³Ù… Ø¨Ø§Ù„Ù…Ø¹Ø±Ù
CREATE OR ALTER PROCEDURE DepartmentsGetById
    @Id BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_Departments WHERE Id = @Id;
END
GO

-- Ø¬Ù„Ø¨ Ø§Ù„Ø£Ù‚Ø³Ø§Ù… Ø¨Ù†Ø¸Ø§Ù… Ø§Ù„ØµÙØ­Ø§Øª (Pagination) - ÙŠØ±Ø¬Ø¹ TotalCount Ø«Ù… Ø§Ù„Ø¨ÙŠØ§Ù†Ø§Øª
CREATE OR ALTER PROCEDURE DepartmentsGetAll
    @PageNumber INT = 1,
    @PageSize   INT = 10,
    @Name       NVARCHAR(100) = NULL,
    @Code       NVARCHAR(20)  = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Ø§Ù„Ù†ØªÙŠØ¬Ø© 1: Ø¥Ø¬Ù…Ø§Ù„ÙŠ Ø§Ù„Ø³Ø¬Ù„Ø§Øª Ø§Ù„Ù…Ø·Ø§Ø¨Ù‚Ø©
    SELECT COUNT(*) AS TotalCount
    FROM vw_Departments
    WHERE (@Name IS NULL OR Name LIKE N'%' + @Name + N'%')
      AND (@Code IS NULL OR Code LIKE N'%' + @Code + N'%');

    -- Ø§Ù„Ù†ØªÙŠØ¬Ø© 2: Ø¨ÙŠØ§Ù†Ø§Øª Ø§Ù„ØµÙØ­Ø© Ø§Ù„Ù…Ø­Ø¯Ø¯Ø©
    SELECT *
    FROM vw_Departments
    WHERE (@Name IS NULL OR Name LIKE N'%' + @Name + N'%')
      AND (@Code IS NULL OR Code LIKE N'%' + @Code + N'%')
    ORDER BY Id DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Ø¬Ù„Ø¨ Ø§Ù„Ø£Ù‚Ø³Ø§Ù… ÙƒÙ€ Lookup Ù„Ù„Ù‚ÙˆØ§Ø¦Ù… Ø§Ù„Ù…Ù†Ø³Ø¯Ù„Ø© (Ø¨Ø¯ÙŠÙ„ NotPaged)
CREATE OR ALTER PROCEDURE DepartmentsLookup
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name, Code
    FROM vw_Departments
    ORDER BY Name ASC;
END
GO

-- Ø¥Ø¶Ø§ÙØ© Ù‚Ø³Ù… Ø¬Ø¯ÙŠØ¯ (Ù…Ø¹ ØªØ³Ø¬ÙŠÙ„ Audit Log ÙÙŠ Ù†ÙØ³ Ø§Ù„Ù…Ø¹Ø§Ù…Ù„Ø© Ø§Ù„Ø°Ø±ÙŠØ©)
CREATE OR ALTER PROCEDURE DepartmentsInsert
    @Name      NVARCHAR(100),
    @Code      NVARCHAR(20),
    @CreatedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NewId BIGINT;

    -- 1. Ø¥Ø¯Ø®Ø§Ù„ Ø§Ù„Ù‚Ø³Ù…
    INSERT INTO Departments (Name, Code, IsDeleted, CreatedBy, CreatedAt)
    VALUES (@Name, @Code, 0, @CreatedBy, GETDATE());

    SET @NewId = SCOPE_IDENTITY();

    -- 2. ØªØ³Ø¬ÙŠÙ„ Ø§Ù„ØªØ¯Ù‚ÙŠÙ‚ (Ø¶Ù…Ù† Ù†ÙØ³ Ø§Ù„Ù…Ø¹Ø§Ù…Ù„Ø© Ø§Ù„Ø°Ø±ÙŠØ©)
    INSERT INTO AuditLogs (UserId, Action, EntityName, EntityId, Changes, IsSuccess, CreatedAt)
    VALUES (
        @CreatedBy,
        'INSERT',
        'Departments',
        CAST(@NewId AS NVARCHAR(100)),
        CONCAT(N'{"Name":"', REPLACE(@Name, '"', '\"'), N'","Code":"', REPLACE(@Code, '"', '\"'), N'"}'),
        1,
        GETDATE()
    );

    SELECT * FROM vw_Departments WHERE Id = @NewId;
END
GO

-- ØªØ¹Ø¯ÙŠÙ„ Ø¨ÙŠØ§Ù†Ø§Øª Ù‚Ø³Ù… (Ù…Ø¹ ØªØ³Ø¬ÙŠÙ„ Audit Log ÙÙŠ Ù†ÙØ³ Ø§Ù„Ù…Ø¹Ø§Ù…Ù„Ø© Ø§Ù„Ø°Ø±ÙŠØ©)
CREATE OR ALTER PROCEDURE DepartmentsUpdate
    @Id        BIGINT,
    @Name      NVARCHAR(100),
    @Code      NVARCHAR(20),
    @UpdatedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. ØªØ­Ø¯ÙŠØ« Ø¨ÙŠØ§Ù†Ø§Øª Ø§Ù„Ù‚Ø³Ù…
    UPDATE Departments
    SET Name      = @Name,
        Code      = @Code,
        UpdatedBy = @UpdatedBy,
        UpdatedAt = GETDATE()
    WHERE Id = @Id AND IsDeleted = 0;

    -- 2. ØªØ³Ø¬ÙŠÙ„ Ø§Ù„ØªØ¯Ù‚ÙŠÙ‚
    INSERT INTO AuditLogs (UserId, Action, EntityName, EntityId, Changes, IsSuccess, CreatedAt)
    VALUES (
        @UpdatedBy,
        'UPDATE',
        'Departments',
        CAST(@Id AS NVARCHAR(100)),
        CONCAT(N'{"Name":"', REPLACE(@Name, '"', '\"'), N'","Code":"', REPLACE(@Code, '"', '\"'), N'"}'),
        1,
        GETDATE()
    );

    SELECT * FROM vw_Departments WHERE Id = @Id;
END
GO

-- Ø­Ø°Ù Ù‚Ø³Ù… (Ø­Ø°Ù Ù…Ù†Ø·Ù‚ÙŠ Soft Delete Ù…Ø¹ ØªØ³Ø¬ÙŠÙ„ Ø§Ù„ØªØ¯Ù‚ÙŠÙ‚)
CREATE OR ALTER PROCEDURE DepartmentsDelete
    @Id     BIGINT,
    @UserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Ø§Ù„Ø­Ø°Ù Ø§Ù„Ù…Ù†Ø·Ù‚ÙŠ
    UPDATE Departments
    SET IsDeleted = 1,
        UpdatedBy = @UserId,
        UpdatedAt = GETDATE()
    WHERE Id = @Id AND IsDeleted = 0;

    -- 2. ØªØ³Ø¬ÙŠÙ„ Ø§Ù„ØªØ¯Ù‚ÙŠÙ‚
    INSERT INTO AuditLogs (UserId, Action, EntityName, EntityId, Changes, IsSuccess, CreatedAt)
    VALUES (
        @UserId,
        'DELETE',
        'Departments',
        CAST(@Id AS NVARCHAR(100)),
        N'{"IsDeleted":1}',
        1,
        GETDATE()
    );

    SELECT CAST(1 AS BIT) AS Success;
END
GO

-- ============================================================================
-- 4. Ø¥Ø¬Ø±Ø§Ø¡Ø§Øª Ø§Ù„Ø·Ù„Ø§Ø¨ (Students Stored Procedures)
-- ============================================================================

-- Ø¬Ù„Ø¨ Ø·Ø§Ù„Ø¨ Ø¨Ø§Ù„Ù…Ø¹Ø±Ù
CREATE OR ALTER PROCEDURE StudentsGetById
    @Id BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_Students WHERE Id = @Id;
END
GO

-- Ø¬Ù„Ø¨ Ø§Ù„Ø·Ù„Ø§Ø¨ Ø¨Ù†Ø¸Ø§Ù… Ø§Ù„ØµÙØ­Ø§Øª (Pagination) - ÙŠØ±Ø¬Ø¹ TotalCount Ø«Ù… Ø§Ù„Ø¨ÙŠØ§Ù†Ø§Øª
CREATE OR ALTER PROCEDURE StudentsGetAll
    @PageNumber   INT = 1,
    @PageSize     INT = 10,
    @FullName     NVARCHAR(150) = NULL,
    @StudentCode  NVARCHAR(50)  = NULL,
    @DepartmentId BIGINT        = NULL,
    @Stage        INT           = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Ø§Ù„Ù†ØªÙŠØ¬Ø© 1: Ø¥Ø¬Ù…Ø§Ù„ÙŠ Ø§Ù„Ø³Ø¬Ù„Ø§Øª Ø§Ù„Ù…Ø·Ø§Ø¨Ù‚Ø© Ù„Ù„ØªØµÙÙŠØ©
    SELECT COUNT(*) AS TotalCount
    FROM vw_Students
    WHERE (@FullName IS NULL OR FullName LIKE N'%' + @FullName + N'%')
      AND (@StudentCode IS NULL OR StudentCode LIKE N'%' + @StudentCode + N'%')
      AND (@DepartmentId IS NULL OR DepartmentId = @DepartmentId)
      AND (@Stage IS NULL OR Stage = @Stage);

    -- Ø§Ù„Ù†ØªÙŠØ¬Ø© 2: Ø¨ÙŠØ§Ù†Ø§Øª Ø§Ù„ØµÙØ­Ø© Ø§Ù„Ø­Ø§Ù„ÙŠØ©
    SELECT *
    FROM vw_Students
    WHERE (@FullName IS NULL OR FullName LIKE N'%' + @FullName + N'%')
      AND (@StudentCode IS NULL OR StudentCode LIKE N'%' + @StudentCode + N'%')
      AND (@DepartmentId IS NULL OR DepartmentId = @DepartmentId)
      AND (@Stage IS NULL OR Stage = @Stage)
    ORDER BY Id DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Ø¥Ø¶Ø§ÙØ© Ø·Ø§Ù„Ø¨ Ø¬Ø¯ÙŠØ¯ (Ù…Ø¹ ØªØ³Ø¬ÙŠÙ„ Ø§Ù„ØªØ¯Ù‚ÙŠÙ‚ Ø¶Ù…Ù† Ù†ÙØ³ Ø§Ù„Ù…Ø¹Ø§Ù…Ù„Ø© Ø§Ù„Ø°Ø±ÙŠØ©)
CREATE OR ALTER PROCEDURE StudentsInsert
    @FullName     NVARCHAR(150),
    @StudentCode  NVARCHAR(50),
    @Email        NVARCHAR(100) = NULL,
    @PhoneNumber  NVARCHAR(30)  = NULL,
    @DepartmentId BIGINT,
    @Stage        INT = 1,
    @BirthDate    DATE = NULL,
    @CreatedBy    BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NewId BIGINT;

    -- 1. Ø¥Ø¯Ø®Ø§Ù„ Ø§Ù„Ø·Ø§Ù„Ø¨
    INSERT INTO Students (FullName, StudentCode, Email, PhoneNumber, DepartmentId, Stage, BirthDate, IsDeleted, CreatedBy, CreatedAt)
    VALUES (@FullName, @StudentCode, @Email, @PhoneNumber, @DepartmentId, @Stage, @BirthDate, 0, @CreatedBy, GETDATE());

    SET @NewId = SCOPE_IDENTITY();

    -- 2. ØªØ³Ø¬ÙŠÙ„ Ø§Ù„ØªØ¯Ù‚ÙŠÙ‚ (Ø¶Ù…Ù† Ù†ÙØ³ Ø§Ù„Ù…Ø¹Ø§Ù…Ù„Ø© Ø§Ù„Ø°Ø±ÙŠØ©)
    INSERT INTO AuditLogs (UserId, Action, EntityName, EntityId, Changes, IsSuccess, CreatedAt)
    VALUES (
        @CreatedBy,
        'INSERT',
        'Students',
        CAST(@NewId AS NVARCHAR(100)),
        CONCAT(
            N'{"FullName":"', REPLACE(@FullName, '"', '\"'),
            N'","StudentCode":"', REPLACE(@StudentCode, '"', '\"'),
            N'","DepartmentId":', @DepartmentId,
            N',"Stage":', @Stage,
            N'}'
        ),
        1,
        GETDATE()
    );

    SELECT * FROM vw_Students WHERE Id = @NewId;
END
GO

-- ØªØ¹Ø¯ÙŠÙ„ Ø¨ÙŠØ§Ù†Ø§Øª Ø·Ø§Ù„Ø¨ (Ù…Ø¹ ØªØ³Ø¬ÙŠÙ„ Ø§Ù„ØªØ¯Ù‚ÙŠÙ‚ Ø¶Ù…Ù† Ù†ÙØ³ Ø§Ù„Ù…Ø¹Ø§Ù…Ù„Ø© Ø§Ù„Ø°Ø±ÙŠØ©)
CREATE OR ALTER PROCEDURE StudentsUpdate
    @Id           BIGINT,
    @FullName     NVARCHAR(150),
    @StudentCode  NVARCHAR(50),
    @Email        NVARCHAR(100) = NULL,
    @PhoneNumber  NVARCHAR(30)  = NULL,
    @DepartmentId BIGINT,
    @Stage        INT = 1,
    @BirthDate    DATE = NULL,
    @UpdatedBy    BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. ØªØ­Ø¯ÙŠØ« Ø§Ù„Ø·Ø§Ù„Ø¨
    UPDATE Students
    SET FullName     = @FullName,
        StudentCode  = @StudentCode,
        Email        = @Email,
        PhoneNumber  = @PhoneNumber,
        DepartmentId = @DepartmentId,
        Stage        = @Stage,
        BirthDate    = @BirthDate,
        UpdatedBy    = @UpdatedBy,
        UpdatedAt    = GETDATE()
    WHERE Id = @Id AND IsDeleted = 0;

    -- 2. ØªØ³Ø¬ÙŠÙ„ Ø§Ù„ØªØ¯Ù‚ÙŠÙ‚
    INSERT INTO AuditLogs (UserId, Action, EntityName, EntityId, Changes, IsSuccess, CreatedAt)
    VALUES (
        @UpdatedBy,
        'UPDATE',
        'Students',
        CAST(@Id AS NVARCHAR(100)),
        CONCAT(
            N'{"FullName":"', REPLACE(@FullName, '"', '\"'),
            N'","StudentCode":"', REPLACE(@StudentCode, '"', '\"'),
            N'","DepartmentId":', @DepartmentId,
            N',"Stage":', @Stage,
            N'}'
        ),
        1,
        GETDATE()
    );

    SELECT * FROM vw_Students WHERE Id = @Id;
END
GO

-- Ø­Ø°Ù Ø·Ø§Ù„Ø¨ (Ø­Ø°Ù Ù…Ù†Ø·Ù‚ÙŠ Soft Delete Ù…Ø¹ ØªØ³Ø¬ÙŠÙ„ Ø§Ù„ØªØ¯Ù‚ÙŠÙ‚)
CREATE OR ALTER PROCEDURE StudentsDelete
    @Id     BIGINT,
    @UserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Ø§Ù„Ø­Ø°Ù Ø§Ù„Ù…Ù†Ø·Ù‚ÙŠ
    UPDATE Students
    SET IsDeleted = 1,
        UpdatedBy = @UserId,
        UpdatedAt = GETDATE()
    WHERE Id = @Id AND IsDeleted = 0;

    -- 2. ØªØ³Ø¬ÙŠÙ„ Ø§Ù„ØªØ¯Ù‚ÙŠÙ‚
    INSERT INTO AuditLogs (UserId, Action, EntityName, EntityId, Changes, IsSuccess, CreatedAt)
    VALUES (
        @UserId,
        'DELETE',
        'Students',
        CAST(@Id AS NVARCHAR(100)),
        N'{"IsDeleted":1}',
        1,
        GETDATE()
    );

    SELECT CAST(1 AS BIT) AS Success;
END
GO

-- ============================================================================
-- 5. Ø¥Ø¬Ø±Ø§Ø¡Ø§Øª Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù…ÙŠÙ† ÙˆØ§Ù„Ù…ØµØ§Ø¯Ù‚Ø© (Users & Auth Stored Procedures)
-- ============================================================================

-- Ø¬Ù„Ø¨ Ù…Ø³ØªØ®Ø¯Ù… Ø¨Ø§Ø³Ù… Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù…
-- ØªÙ… Ø¥Ø²Ø§Ù„Ø© Ø´Ø±Ø· IsActive = 1 Ù„ØªÙ…ÙƒÙŠÙ† Ø·Ø¨Ù‚Ø© Ø§Ù„Ø®Ø¯Ù…Ø© Ù…Ù† ÙØ­Øµ Ø­Ø§Ù„Ø© Ø§Ù„Ø­Ø³Ø§Ø¨ ÙˆØ¥Ø±Ø¬Ø§Ø¹ Ø±Ø³Ø§Ù„Ø© "Ø§Ù„Ø­Ø³Ø§Ø¨ Ù…Ø¹Ø·Ù„"
CREATE OR ALTER PROCEDURE UsersGetByUserName
    @UserName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, FullName, UserName, PasswordHash, Role, IsActive, IsDeleted, CreatedAt
    FROM Users
    WHERE UserName = @UserName AND IsDeleted = 0;
END
GO

-- Ø¥Ù†Ø´Ø§Ø¡ Ù…Ø³ØªØ®Ø¯Ù… Ø¬Ø¯ÙŠØ¯ (Ù…Ø¹ ØªØ³Ø¬ÙŠÙ„ Ø§Ù„ØªØ¯Ù‚ÙŠÙ‚ ÙˆØªØ¬Ù†Ø¨ Ø­ÙØ¸ ÙƒÙ„Ù…Ø© Ø§Ù„Ù…Ø±ÙˆØ± ÙÙŠ Ø§Ù„ØªØ¯Ù‚ÙŠÙ‚)
CREATE OR ALTER PROCEDURE UsersInsert
    @FullName     NVARCHAR(150),
    @UserName     NVARCHAR(100),
    @PasswordHash NVARCHAR(255),
    @Role         NVARCHAR(50) = 'Admin',
    @CreatedBy    BIGINT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NewId BIGINT;

    -- 1. Ø¥Ø¶Ø§ÙØ© Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù…
    INSERT INTO Users (FullName, UserName, PasswordHash, Role, IsActive, IsDeleted, CreatedBy, CreatedAt)
    VALUES (@FullName, @UserName, @PasswordHash, @Role, 1, 0, @CreatedBy, GETDATE());

    SET @NewId = SCOPE_IDENTITY();

    -- 2. ØªØ³Ø¬ÙŠÙ„ Ø§Ù„ØªØ¯Ù‚ÙŠÙ‚ (Ù…Ù„Ø§Ø­Ø¸Ø©: Ù„Ø§ Ù†Ù‚ÙˆÙ… Ø¨ØªØ³Ø¬ÙŠÙ„ PasswordHash Ù†Ù‡Ø§Ø¦ÙŠØ§Ù‹ Ù„Ø­Ù…Ø§ÙŠØ© Ø§Ù„Ø£Ù…Ø§Ù†)
    INSERT INTO AuditLogs (UserId, Action, EntityName, EntityId, Changes, IsSuccess, CreatedAt)
    VALUES (
        @CreatedBy,
        'INSERT',
        'Users',
        CAST(@NewId AS NVARCHAR(100)),
        CONCAT(
            N'{"FullName":"', REPLACE(@FullName, '"', '\"'),
            N'","UserName":"', REPLACE(@UserName, '"', '\"'),
            N'","Role":"', REPLACE(@Role, '"', '\"'),
            N'"}'
        ),
        1,
        GETDATE()
    );

    SELECT Id, FullName, UserName, PasswordHash, Role, IsActive, IsDeleted, CreatedAt
    FROM Users WHERE Id = @NewId;
END
GO
