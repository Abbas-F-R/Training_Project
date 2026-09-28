-- ============================================================================
-- Features/Students/Sql/02_Students_Procedures.sql
-- إجراءات الـ CRUD للطلاب مع تسجيل التدقيق المدمج (In-Transaction Audit)
-- تم استبعاد إجراء GetAllNotPaged لعدم وجود حاجة وظيفية له في الطلاب
-- ============================================================================

-- 1. جلب طالب بالمعرف
CREATE OR ALTER PROCEDURE StudentsGetById
    @Id BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_Students WHERE Id = @Id;
END
GO

-- 2. جلب الطلاب بنظام الصفحات (Pagination) - يرجع TotalCount ثم البيانات
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

-- 3. إضافة طالب جديد (مع تسجيل التدقيق ضمن نفس المعاملة)
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

-- 4. تعديل بيانات طالب (مع تسجيل التدقيق ضمن نفس المعاملة)
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

-- 5. حذف طالب (حذف منطقي Soft Delete مع تسجيل التدقيق)
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
