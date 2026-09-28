-- ============================================================================
-- Features/AuditLogs/Sql/01_AuditLogs_Tables_Indexes.sql
-- جدول سجل التدقيق والتتبع (AuditLogs) مع الفهارس المخصصة لتسريع الاستعلام
-- تصميم Append-Only غير قابل للتعديل أو الحذف
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE AuditLogs
    (
        Id          BIGINT IDENTITY(1,1) NOT NULL,
        UserId      BIGINT               NULL,         -- معرف المستخدم المنفذ للعملية (NULL في حال فشل تسجيل الدخول أو عمليات النظام)
        Action      NVARCHAR(50)         NOT NULL,     -- نوع العملية: INSERT, UPDATE, DELETE, LOGIN_SUCCESS, LOGIN_FAILED
        EntityName  NVARCHAR(100)        NOT NULL,     -- اسم الكيان المتأثر: Departments, Students, Auth, Users
        EntityId    NVARCHAR(100)        NULL,         -- المعرف الخاص بالسجل المتأثر (أو اسم المستخدم في محاولات الدخول)
        Changes     NVARCHAR(MAX)        NULL,         -- تفاصيل التغييرات بصيغة JSON خالية تماماً من البيانات الحساسة
        IpAddress   NVARCHAR(50)         NULL,         -- عنوان IP لمصدر الطلب (إن وجد)
        UserAgent   NVARCHAR(255)        NULL,         -- متصفح أو برنامج العميل
        IsSuccess   BIT                  DEFAULT 1 NOT NULL, -- هل نجحت العملية (1) أم فشلت (0)
        CreatedAt   DATETIME             DEFAULT GETDATE() NOT NULL,

        -- القيد الأساسي
        CONSTRAINT PK_AuditLogs PRIMARY KEY CLUSTERED (Id)
    );
END
GO

-- 1. فهرس لتسريع البحث وتتبع سجلات كيان معين (EntityName + EntityId)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_Entity' AND object_id = OBJECT_ID('AuditLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AuditLogs_Entity
    ON AuditLogs (EntityName, EntityId)
    INCLUDE (Action, UserId, CreatedAt);
END
GO

-- 2. فهرس لتسريع تتبع كافة العمليات التي قام بها مستخدم محدد
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_UserId' AND object_id = OBJECT_ID('AuditLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AuditLogs_UserId
    ON AuditLogs (UserId)
    INCLUDE (Action, EntityName, CreatedAt)
    WHERE UserId IS NOT NULL;
END
GO

-- 3. فهرس لتسريع تصفية واستعراض السجلات بحسب التاريخ التنازلي (أحدث العمليات أولاً)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_CreatedAt' AND object_id = OBJECT_ID('AuditLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AuditLogs_CreatedAt
    ON AuditLogs (CreatedAt DESC)
    INCLUDE (Action, EntityName, UserId, IsSuccess);
END
GO

-- 4. فهرس لتسريع التصفية حسب نوع العملية (مثل مراقبة محاولات الدخول الفاشلة LOGIN_FAILED)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_Action' AND object_id = OBJECT_ID('AuditLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AuditLogs_Action
    ON AuditLogs (Action, IsSuccess)
    INCLUDE (EntityName, EntityId, CreatedAt);
END
GO
