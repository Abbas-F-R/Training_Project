-- ============================================================================
-- Features/AuditLogs/Sql/02_AuditLogs_Procedures.sql
-- إجراءات سجل التدقيق: الإضافة والاستعلام المرقم
-- تنبيه: لا توجد إجراءات تعديل أو حذف لحماية سلامة وتاريخية سجلات التدقيق
-- ============================================================================

-- 1. إضافة سجل تدقيق جديد (تستخدمها الخدمات وأحداث المصادقة)
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

-- 2. استعراض سجلات التدقيق بنظام الصفحات مع الفلترة (خاصة بالمسؤول Admin)
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
