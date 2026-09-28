-- ============================================================================
-- sql/MasterMigration.sql
-- Master Database Migration Script
-- Consolidates all feature-based database scripts in execution order:
-- 1. Infrastructure: Core shared procedures and AuditLogs table
-- 2. Auth: Users table, indexes, constraints, and authentication procedures
-- 3. Departments: Departments table, view, constraints, and CRUD procedures with atomic auditing
-- 4. Students: Students table, view, constraints, and CRUD procedures with atomic auditing
-- 5. Seed Data: Default departments, users, and students
-- ============================================================================

-- Ø¥Ù†Ø´Ø§Ø¡ Ù‚Ø§Ø¹Ø¯Ø© Ø§Ù„Ø¨ÙŠØ§Ù†Ø§Øª Ø¥Ø°Ø§ Ù„Ù… ØªÙƒÙ† Ù…ÙˆØ¬ÙˆØ¯Ø©
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'Training_Project_DB')
BEGIN
    CREATE DATABASE [Training_Project_DB];
END
GO

USE [Training_Project_DB];
GO

-- ============================================================================
-- Ø§Ù„Ø®Ø·ÙˆØ© 1: Ø¬Ø¯ÙˆÙ„ Ø³Ø¬Ù„ Ø§Ù„ØªØ¯Ù‚ÙŠÙ‚ ÙˆØ§Ù„ØªØªØ¨Ø¹ (AuditLogs) ÙˆØ§Ù„Ø¥Ø¬Ø±Ø§Ø¡Ø§Øª Ø§Ù„Ù…Ø³Ø§Ø¹Ø¯Ø© Ø§Ù„Ù…Ø´ØªØ±ÙƒØ©
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE AuditLogs
    (
        Id          BIGINT IDENTITY(1,1) NOT NULL,
        UserId      BIGINT               NULL,
        Action      NVARCHAR(50)         NOT NULL,
        EntityName  NVARCHAR(100)        NOT NULL,
        EntityId    NVARCHAR(100)        NULL,
        Changes     NVARCHAR(MAX)        NULL,
        IpAddress   NVARCHAR(50)         NULL,
        UserAgent   NVARCHAR(255)        NULL,
        IsSuccess   BIT                  DEFAULT 1 NOT NULL,
        CreatedAt   DATETIME             DEFAULT GETDATE() NOT NULL,

        CONSTRAINT PK_AuditLogs PRIMARY KEY CLUSTERED (Id)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_Entity' AND object_id = OBJECT_ID('AuditLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AuditLogs_Entity
    ON AuditLogs (EntityName, EntityId)
    INCLUDE (Action, UserId, CreatedAt);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_UserId' AND object_id = OBJECT_ID('AuditLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AuditLogs_UserId
    ON AuditLogs (UserId)
    INCLUDE (Action, EntityName, CreatedAt)
    WHERE UserId IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_CreatedAt' AND object_id = OBJECT_ID('AuditLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AuditLogs_CreatedAt
    ON AuditLogs (CreatedAt DESC)
    INCLUDE (Action, EntityName, UserId, IsSuccess);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_Action' AND object_id = OBJECT_ID('AuditLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AuditLogs_Action
    ON AuditLogs (Action, IsSuccess)
    INCLUDE (EntityName, EntityId, CreatedAt);
END
GO

-- Ø¥Ø¬Ø±Ø§Ø¡ Ø¥Ø¶Ø§ÙØ© Ø³Ø¬Ù„ ØªØ¯Ù‚ÙŠÙ‚
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

    SELECT Id, UserId, Action, EntityName, EntityId, Changes, IpAddress, UserAgent, IsSuccess, CreatedAt
    FROM AuditLogs WHERE Id = @NewId;
END
GO

-- Ø¥Ø¬Ø±Ø§Ø¡ Ø§Ø³ØªØ¹Ø±Ø§Ø¶ Ø³Ø¬Ù„Ø§Øª Ø§Ù„ØªØ¯Ù‚ÙŠÙ‚
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

    SELECT COUNT(*) AS TotalCount
    FROM AuditLogs
    WHERE (@EntityName IS NULL OR EntityName = @EntityName)
      AND (@Action IS NULL OR Action = @Action)
      AND (@UserId IS NULL OR UserId = @UserId)
      AND (@FromDate IS NULL OR CreatedAt >= @FromDate)
      AND (@ToDate IS NULL OR CreatedAt <= @ToDate);

    SELECT Id, UserId, Action, EntityName, EntityId, Changes, IpAddress, UserAgent, IsSuccess, CreatedAt
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

-- ÙØ­Øµ ØªÙƒØ±Ø§Ø± Ù‚ÙŠÙ…Ø© ÙÙŠ Ø¹Ù…ÙˆØ¯ Ù…Ø¹ÙŠÙ† (ØªØ³ØªØ®Ø¯Ù…Ù‡ IsDuplicateAsync ÙÙŠ BaseRepository)
CREATE OR ALTER PROCEDURE Base_CheckDuplicate
    @TableName  NVARCHAR(100),
    @ColumnName NVARCHAR(100),
    @Value      NVARCHAR(255),
    @ExcludeId  BIGINT = NULL
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

-- Ø¬Ù„Ø¨ Ø£ÙˆÙ„ Ø³Ø¬Ù„ ÙŠØ·Ø§Ø¨Ù‚ Ù‚ÙŠÙ…Ø© Ø¹Ù…ÙˆØ¯ (ØªØ³ØªØ®Ø¯Ù…Ù‡ GetFirstAsync ÙÙŠ BaseRepository)
CREATE OR ALTER PROCEDURE Base_GetFirst
    @TableName  NVARCHAR(100),
    @ColumnName NVARCHAR(100),
    @Value      NVARCHAR(255)
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
-- Ø§Ù„Ø®Ø·ÙˆØ© 2: Ù…ÙŠØ²Ø© Ø§Ù„Ø£Ù…Ø§Ù† ÙˆØ§Ù„Ù…Ø³ØªØ®Ø¯Ù…ÙŠÙ† (Features/Auth/Sql)
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users
    (
        Id           BIGINT IDENTITY(1,1) NOT NULL,
        FullName     NVARCHAR(150) NOT NULL,
        UserName     NVARCHAR(100) NOT NULL,
        PasswordHash NVARCHAR(255) NOT NULL,
        Role         NVARCHAR(50)  DEFAULT 'Admin' NOT NULL,
        IsActive     BIT           DEFAULT 1 NOT NULL,
        IsDeleted    BIT           DEFAULT 0 NOT NULL,
        CreatedBy    BIGINT        NULL,
        CreatedAt    DATETIME      DEFAULT GETDATE() NOT NULL,
        UpdatedBy    BIGINT        NULL,
        UpdatedAt    DATETIME      NULL,

        CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT CK_Users_Role CHECK (Role IN ('Admin', 'User', 'Manager')),
        CONSTRAINT CK_Users_UserName_Length CHECK (LEN(UserName) >= 3)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Users_UserName_Active' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Users_UserName_Active
    ON Users (UserName)
    WHERE IsDeleted = 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Login' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_Login
    ON Users (UserName, IsActive)
    INCLUDE (PasswordHash, FullName, Role)
    WHERE IsDeleted = 0;
END
GO

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

    INSERT INTO Users (FullName, UserName, PasswordHash, Role, IsActive, IsDeleted, CreatedBy, CreatedAt)
    VALUES (@FullName, @UserName, @PasswordHash, @Role, 1, 0, @CreatedBy, GETDATE());

    SET @NewId = SCOPE_IDENTITY();

    -- ØªØ³Ø¬ÙŠÙ„ Ø§Ù„ØªØ¯Ù‚ÙŠÙ‚ Ø¨Ø¯ÙˆÙ† Ø­ÙØ¸ ÙƒÙ„Ù…Ø© Ø§Ù„Ù…Ø±ÙˆØ±
    INSERT INTO AuditLogs (UserId, Action, EntityName, EntityId, Changes, IsSuccess, CreatedAt)
    VALUES (
        @CreatedBy,
        'INSERT',
        'Users',
        CAST(@NewId AS NVARCHAR(100)),
        CONCAT(N'{"FullName":"', REPLACE(@FullName, '"', '\"'), N'","UserName":"', REPLACE(@UserName, '"', '\"'), N'","Role":"', REPLACE(@Role, '"', '\"'), N'"}'),
        1,
        GETDATE()
    );

    SELECT Id, FullName, UserName, PasswordHash, Role, IsActive, IsDeleted, CreatedAt
    FROM Users WHERE Id = @NewId;
END
GO

-- ============================================================================
-- Ø§Ù„Ø®Ø·ÙˆØ© 3: Ù…ÙŠØ²Ø© Ø§Ù„Ø£Ù‚Ø³Ø§Ù… Ø§Ù„Ø¯Ø±Ø§Ø³ÙŠØ© (Features/Departments/Sql)
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Departments')
BEGIN
    CREATE TABLE Departments
    (
        Id        BIGINT IDENTITY(1,1) NOT NULL,
        Name      NVARCHAR(100) NOT NULL,
        Code      NVARCHAR(20)  NOT NULL,
        IsDeleted BIT           DEFAULT 0 NOT NULL,
        CreatedBy BIGINT        NULL,
        CreatedAt DATETIME      DEFAULT GETDATE() NOT NULL,
        UpdatedBy BIGINT        NULL,
        UpdatedAt DATETIME      NULL,

        CONSTRAINT PK_Departments PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT CK_Departments_Code CHECK (LEN(Code) >= 2 AND Code NOT LIKE '% %'),
        CONSTRAINT CK_Departments_Name CHECK (LEN(Name) >= 3)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Departments_Code_Active' AND object_id = OBJECT_ID('Departments'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Departments_Code_Active
    ON Departments (Code)
    WHERE IsDeleted = 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Departments_Name' AND object_id = OBJECT_ID('Departments'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Departments_Name
    ON Departments (Name)
    WHERE IsDeleted = 0;
END
GO

CREATE OR ALTER VIEW vw_Departments
AS
SELECT
    d.Id,
    d.Name,
    d.Code,
    d.IsDeleted,
    d.CreatedBy,
    d.CreatedAt,
    d.UpdatedBy,
    d.UpdatedAt
FROM Departments d
WHERE d.IsDeleted = 0;
GO

CREATE OR ALTER PROCEDURE DepartmentsGetById
    @Id BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_Departments WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE DepartmentsGetAll
    @PageNumber INT = 1,
    @PageSize   INT = 10,
    @Name       NVARCHAR(100) = NULL,
    @Code       NVARCHAR(20)  = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT COUNT(*) AS TotalCount
    FROM vw_Departments
    WHERE (@Name IS NULL OR Name LIKE N'%' + @Name + N'%')
      AND (@Code IS NULL OR Code LIKE N'%' + @Code + N'%');

    SELECT *
    FROM vw_Departments
    WHERE (@Name IS NULL OR Name LIKE N'%' + @Name + N'%')
      AND (@Code IS NULL OR Code LIKE N'%' + @Code + N'%')
    ORDER BY Id DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE DepartmentsLookup
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name, Code
    FROM vw_Departments
    ORDER BY Name ASC;
END
GO

CREATE OR ALTER PROCEDURE DepartmentsInsert
    @Name      NVARCHAR(100),
    @Code      NVARCHAR(20),
    @CreatedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NewId BIGINT;

    INSERT INTO Departments (Name, Code, IsDeleted, CreatedBy, CreatedAt)
    VALUES (@Name, @Code, 0, @CreatedBy, GETDATE());

    SET @NewId = SCOPE_IDENTITY();

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

CREATE OR ALTER PROCEDURE DepartmentsUpdate
    @Id        BIGINT,
    @Name      NVARCHAR(100),
    @Code      NVARCHAR(20),
    @UpdatedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Departments
    SET Name      = @Name,
        Code      = @Code,
        UpdatedBy = @UpdatedBy,
        UpdatedAt = GETDATE()
    WHERE Id = @Id AND IsDeleted = 0;

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

CREATE OR ALTER PROCEDURE DepartmentsDelete
    @Id     BIGINT,
    @UserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Departments
    SET IsDeleted = 1,
        UpdatedBy = @UserId,
        UpdatedAt = GETDATE()
    WHERE Id = @Id AND IsDeleted = 0;

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
-- Ø§Ù„Ø®Ø·ÙˆØ© 4: Ù…ÙŠØ²Ø© Ø§Ù„Ø·Ù„Ø§Ø¨ (Features/Students/Sql)
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
BEGIN
    CREATE TABLE Students
    (
        Id           BIGINT IDENTITY(1,1) NOT NULL,
        FullName     NVARCHAR(150) NOT NULL,
        StudentCode  NVARCHAR(50)  NOT NULL,
        Email        NVARCHAR(100) NULL,
        PhoneNumber  NVARCHAR(30)  NULL,
        DepartmentId BIGINT        NOT NULL,
        Stage        INT           DEFAULT 1 NOT NULL,
        BirthDate    DATE          NULL,
        IsDeleted    BIT           DEFAULT 0 NOT NULL,
        CreatedBy    BIGINT        NULL,
        CreatedAt    DATETIME      DEFAULT GETDATE() NOT NULL,
        UpdatedBy    BIGINT        NULL,
        UpdatedAt    DATETIME      NULL,

        CONSTRAINT PK_Students PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Students_Departments FOREIGN KEY (DepartmentId) 
            REFERENCES Departments(Id) ON DELETE NO ACTION,
        CONSTRAINT CK_Students_Stage CHECK (Stage BETWEEN 1 AND 6),
        CONSTRAINT CK_Students_Email CHECK (Email IS NULL OR Email LIKE '%_@__%.__%'),
        CONSTRAINT CK_Students_BirthDate CHECK (BirthDate IS NULL OR BirthDate <= GETDATE())
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Students_StudentCode_Active' AND object_id = OBJECT_ID('Students'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Students_StudentCode_Active
    ON Students (StudentCode)
    WHERE IsDeleted = 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_DepartmentId' AND object_id = OBJECT_ID('Students'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Students_DepartmentId
    ON Students (DepartmentId)
    INCLUDE (FullName, StudentCode, Stage)
    WHERE IsDeleted = 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_FullName' AND object_id = OBJECT_ID('Students'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Students_FullName
    ON Students (FullName)
    INCLUDE (StudentCode, DepartmentId, Stage)
    WHERE IsDeleted = 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_Stage' AND object_id = OBJECT_ID('Students'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Students_Stage
    ON Students (Stage)
    WHERE IsDeleted = 0;
END
GO

CREATE OR ALTER VIEW vw_Students
AS
SELECT
    s.Id,
    s.FullName,
    s.StudentCode,
    s.Email,
    s.PhoneNumber,
    s.DepartmentId,
    d.Name AS DepartmentName,
    d.Code AS DepartmentCode,
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
GO

CREATE OR ALTER PROCEDURE StudentsGetById
    @Id BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_Students WHERE Id = @Id;
END
GO

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

    SELECT COUNT(*) AS TotalCount
    FROM vw_Students
    WHERE (@FullName IS NULL OR FullName LIKE N'%' + @FullName + N'%')
      AND (@StudentCode IS NULL OR StudentCode LIKE N'%' + @StudentCode + N'%')
      AND (@DepartmentId IS NULL OR DepartmentId = @DepartmentId)
      AND (@Stage IS NULL OR Stage = @Stage);

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

    INSERT INTO Students (FullName, StudentCode, Email, PhoneNumber, DepartmentId, Stage, BirthDate, IsDeleted, CreatedBy, CreatedAt)
    VALUES (@FullName, @StudentCode, @Email, @PhoneNumber, @DepartmentId, @Stage, @BirthDate, 0, @CreatedBy, GETDATE());

    SET @NewId = SCOPE_IDENTITY();

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

CREATE OR ALTER PROCEDURE StudentsDelete
    @Id     BIGINT,
    @UserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Students
    SET IsDeleted = 1,
        UpdatedBy = @UserId,
        UpdatedAt = GETDATE()
    WHERE Id = @Id AND IsDeleted = 0;

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
-- Ø§Ù„Ø®Ø·ÙˆØ© 5: Ø§Ù„Ø¨ÙŠØ§Ù†Ø§Øª Ø§Ù„Ø£ÙˆÙ„ÙŠØ© Ø§Ù„ØªØ¬Ø±ÙŠØ¨ÙŠØ© (Infrastructure/Persistence/Sql)
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM Departments WHERE Code = 'CS')
    INSERT INTO Departments (Name, Code, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'Ø¹Ù„ÙˆÙ… Ø§Ù„Ø­Ø§Ø³ÙˆØ¨ (Computer Science)', 'CS', 0, 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Departments WHERE Code = 'SE')
    INSERT INTO Departments (Name, Code, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'Ù‡Ù†Ø¯Ø³Ø© Ø§Ù„Ø¨Ø±Ù…Ø¬ÙŠØ§Øª (Software Engineering)', 'SE', 0, 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Departments WHERE Code = 'IS')
    INSERT INTO Departments (Name, Code, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'Ù†Ø¸Ù… Ø§Ù„Ù…Ø¹Ù„ÙˆÙ…Ø§Øª (Information Systems)', 'IS', 0, 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Departments WHERE Code = 'AI')
    INSERT INTO Departments (Name, Code, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'Ø§Ù„Ø°ÙƒØ§Ø¡ Ø§Ù„Ø§ØµØ·Ù†Ø§Ø¹ÙŠ (Artificial Intelligence)', 'AI', 0, 1, GETDATE());
GO

DECLARE @CsId BIGINT = (SELECT TOP 1 Id FROM Departments WHERE Code = 'CS');
DECLARE @SeId BIGINT = (SELECT TOP 1 Id FROM Departments WHERE Code = 'SE');
DECLARE @IsId BIGINT = (SELECT TOP 1 Id FROM Departments WHERE Code = 'IS');

IF NOT EXISTS (SELECT 1 FROM Students WHERE StudentCode = 'STU-2026-001')
    INSERT INTO Students (FullName, StudentCode, Email, PhoneNumber, DepartmentId, Stage, BirthDate, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'Ø¹Ù„ÙŠ Ø£Ø­Ù…Ø¯ Ø­Ø³Ù†', 'STU-2026-001', 'ali.ahmed@univ.edu', '07701234567', @CsId, 3, '2003-05-14', 0, 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Students WHERE StudentCode = 'STU-2026-002')
    INSERT INTO Students (FullName, StudentCode, Email, PhoneNumber, DepartmentId, Stage, BirthDate, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'ÙØ§Ø·Ù…Ø© Ø­ÙŠØ¯Ø± ÙƒØ§Ø¸Ù…', 'STU-2026-002', 'fatima.haidar@univ.edu', '07802345678', @SeId, 4, '2002-11-20', 0, 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Students WHERE StudentCode = 'STU-2026-003')
    INSERT INTO Students (FullName, StudentCode, Email, PhoneNumber, DepartmentId, Stage, BirthDate, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'Ø­Ø³ÙŠÙ† Ù…Ø­Ù…Ø¯ Ø¬ÙˆØ§Ø¯', 'STU-2026-003', 'hussein.m@univ.edu', '07903456789', @IsId, 2, '2004-02-10', 0, 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Students WHERE StudentCode = 'STU-2026-004')
    INSERT INTO Students (FullName, StudentCode, Email, PhoneNumber, DepartmentId, Stage, BirthDate, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'Ø²ÙŠÙ†Ø¨ Ø¹Ø¨Ø§Ø³ ÙƒØ±ÙŠÙ…', 'STU-2026-004', 'zainab.a@univ.edu', '07704567890', @CsId, 1, '2005-08-25', 0, 1, GETDATE());
GO
