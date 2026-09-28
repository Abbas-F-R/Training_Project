-- ============================================================================
-- Features/Departments/Sql/02_Departments_Procedures.sql
-- إجراءات الـ CRUD للأقسام الدراسية مع تسجيل التدقيق الآلي المدمج (In-Transaction Audit)
-- والـ Lookup للقوائم المنسدلة
-- ============================================================================

-- 1. جلب قسم بالمعرف
CREATE OR ALTER PROCEDURE DepartmentsGetById
    @Id BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_Departments WHERE Id = @Id;
END
GO

-- 2. جلب الأقسام بنظام الصفحات (Pagination) - يرجع TotalCount ثم البيانات
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

-- 3. استرجاع الأقسام كـ Lookup للقوائم المنسدلة (بديل NotPaged)
CREATE OR ALTER PROCEDURE DepartmentsLookup
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name, Code
    FROM vw_Departments
    ORDER BY Name ASC;
END
GO

-- 4. إضافة قسم جديد (مع تسجيل Audit Log في نفس المعاملة)
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

-- 5. تعديل بيانات قسم (مع تسجيل Audit Log في نفس المعاملة)
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

-- 6. حذف قسم (حذف منطقي Soft Delete مع تسجيل التدقيق)
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
