-- ============================================================================
-- 03_Procedures.sql
-- Student Management System Stored Procedures
-- High-Performance CRUD Operations with Atomic In-Transaction Auditing
-- ============================================================================

USE [OC_System_Training_DB];
GO

-- ============================================================================
-- 1. الإجراءات المساعدة للنظام الأساسي (Base Helper Procedures)
-- ============================================================================

-- فحص تكرار قيمة في عمود معين (تستخدمه IsDuplicateAsync)
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

-- جلب أول سجل يطابق قيمة عمود (تستخدمه GetFirstAsync)
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
-- 2. إجراءات سجل التدقيق والتتبع (AuditLogs Procedures)
-- ============================================================================

-- إضافة سجل تدقيق جديد (تستخدمها الخدمات وتدقيق الدخول)
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

-- استعراض سجلات التدقيق بنظام الصفحات مع الفلترة (خاصة بالمسؤول Admin)
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

    -- النتيجة 1: إجمالي عدد السجلات المطابقة
    SELECT COUNT(*) AS TotalCount
    FROM AuditLogs
    WHERE (@EntityName IS NULL OR EntityName = @EntityName)
      AND (@Action IS NULL OR Action = @Action)
      AND (@UserId IS NULL OR UserId = @UserId)
      AND (@FromDate IS NULL OR CreatedAt >= @FromDate)
      AND (@ToDate IS NULL OR CreatedAt <= @ToDate);

    -- النتيجة 2: بيانات الصفحة المطلوبة
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
-- 3. إجراءات الأقسام الدراسية (Departments Stored Procedures)
-- ============================================================================

-- جلب قسم بالمعرف
CREATE OR ALTER PROCEDURE DepartmentsGetById
    @Id BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_Departments WHERE Id = @Id;
END
GO

-- جلب الأقسام بنظام الصفحات (Pagination) - يرجع TotalCount ثم البيانات
CREATE OR ALTER PROCEDURE DepartmentsGetAll
    @PageNumber INT = 1,
    @PageSize   INT = 10,
    @Name       NVARCHAR(100) = NULL,
    @Code       NVARCHAR(20)  = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- النتيجة 1: إجمالي السجلات المطابقة
    SELECT COUNT(*) AS TotalCount
    FROM vw_Departments
    WHERE (@Name IS NULL OR Name LIKE N'%' + @Name + N'%')
      AND (@Code IS NULL OR Code LIKE N'%' + @Code + N'%');

    -- النتيجة 2: بيانات الصفحة المحددة
    SELECT *
    FROM vw_Departments
    WHERE (@Name IS NULL OR Name LIKE N'%' + @Name + N'%')
      AND (@Code IS NULL OR Code LIKE N'%' + @Code + N'%')
    ORDER BY Id DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- جلب الأقسام كـ Lookup للقوائم المنسدلة (بديل NotPaged)
CREATE OR ALTER PROCEDURE DepartmentsLookup
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name, Code
    FROM vw_Departments
    ORDER BY Name ASC;
END
GO

-- إضافة قسم جديد (مع تسجيل Audit Log في نفس المعاملة الذرية)
CREATE OR ALTER PROCEDURE DepartmentsInsert
    @Name      NVARCHAR(100),
    @Code      NVARCHAR(20),
    @CreatedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NewId BIGINT;

    -- 1. إدخال القسم
    INSERT INTO Departments (Name, Code, IsDeleted, CreatedBy, CreatedAt)
    VALUES (@Name, @Code, 0, @CreatedBy, GETDATE());

    SET @NewId = SCOPE_IDENTITY();

    -- 2. تسجيل التدقيق (ضمن نفس المعاملة الذرية)
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

-- تعديل بيانات قسم (مع تسجيل Audit Log في نفس المعاملة الذرية)
CREATE OR ALTER PROCEDURE DepartmentsUpdate
    @Id        BIGINT,
    @Name      NVARCHAR(100),
    @Code      NVARCHAR(20),
    @UpdatedBy BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. تحديث بيانات القسم
    UPDATE Departments
    SET Name      = @Name,
        Code      = @Code,
        UpdatedBy = @UpdatedBy,
        UpdatedAt = GETDATE()
    WHERE Id = @Id AND IsDeleted = 0;

    -- 2. تسجيل التدقيق
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

-- حذف قسم (حذف منطقي Soft Delete مع تسجيل التدقيق)
CREATE OR ALTER PROCEDURE DepartmentsDelete
    @Id     BIGINT,
    @UserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. الحذف المنطقي
    UPDATE Departments
    SET IsDeleted = 1,
        UpdatedBy = @UserId,
        UpdatedAt = GETDATE()
    WHERE Id = @Id AND IsDeleted = 0;

    -- 2. تسجيل التدقيق
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
-- 4. إجراءات الطلاب (Students Stored Procedures)
-- ============================================================================

-- جلب طالب بالمعرف
CREATE OR ALTER PROCEDURE StudentsGetById
    @Id BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_Students WHERE Id = @Id;
END
GO

-- جلب الطلاب بنظام الصفحات (Pagination) - يرجع TotalCount ثم البيانات
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

    -- النتيجة 1: إجمالي السجلات المطابقة للتصفية
    SELECT COUNT(*) AS TotalCount
    FROM vw_Students
    WHERE (@FullName IS NULL OR FullName LIKE N'%' + @FullName + N'%')
      AND (@StudentCode IS NULL OR StudentCode LIKE N'%' + @StudentCode + N'%')
      AND (@DepartmentId IS NULL OR DepartmentId = @DepartmentId)
      AND (@Stage IS NULL OR Stage = @Stage);

    -- النتيجة 2: بيانات الصفحة الحالية
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

-- إضافة طالب جديد (مع تسجيل التدقيق ضمن نفس المعاملة الذرية)
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

    -- 1. إدخال الطالب
    INSERT INTO Students (FullName, StudentCode, Email, PhoneNumber, DepartmentId, Stage, BirthDate, IsDeleted, CreatedBy, CreatedAt)
    VALUES (@FullName, @StudentCode, @Email, @PhoneNumber, @DepartmentId, @Stage, @BirthDate, 0, @CreatedBy, GETDATE());

    SET @NewId = SCOPE_IDENTITY();

    -- 2. تسجيل التدقيق (ضمن نفس المعاملة الذرية)
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

-- تعديل بيانات طالب (مع تسجيل التدقيق ضمن نفس المعاملة الذرية)
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

    -- 1. تحديث الطالب
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

    -- 2. تسجيل التدقيق
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

-- حذف طالب (حذف منطقي Soft Delete مع تسجيل التدقيق)
CREATE OR ALTER PROCEDURE StudentsDelete
    @Id     BIGINT,
    @UserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. الحذف المنطقي
    UPDATE Students
    SET IsDeleted = 1,
        UpdatedBy = @UserId,
        UpdatedAt = GETDATE()
    WHERE Id = @Id AND IsDeleted = 0;

    -- 2. تسجيل التدقيق
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
-- 5. إجراءات المستخدمين والمصادقة (Users & Auth Stored Procedures)
-- ============================================================================

-- جلب مستخدم باسم المستخدم
-- تم إزالة شرط IsActive = 1 لتمكين طبقة الخدمة من فحص حالة الحساب وإرجاع رسالة "الحساب معطل"
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

-- إنشاء مستخدم جديد (مع تسجيل التدقيق وتجنب حفظ كلمة المرور في التدقيق)
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

    -- 1. إضافة المستخدم
    INSERT INTO Users (FullName, UserName, PasswordHash, Role, IsActive, IsDeleted, CreatedBy, CreatedAt)
    VALUES (@FullName, @UserName, @PasswordHash, @Role, 1, 0, @CreatedBy, GETDATE());

    SET @NewId = SCOPE_IDENTITY();

    -- 2. تسجيل التدقيق (ملاحظة: لا نقوم بتسجيل PasswordHash نهائياً لحماية الأمان)
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
