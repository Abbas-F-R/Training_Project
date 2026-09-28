-- ============================================================================
-- Features/Auth/Sql/01_Users_Tables_Constraints_Indexes.sql
-- تعريف جدول المستخدمين مع القيود (Constraints) والفهارس المصفاة (Filtered Indexes)
-- ============================================================================

-- 1. جدول المستخدمين
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

        -- القيد الأساسي
        CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),

        -- قيد التحقق من صحة الدور
        CONSTRAINT CK_Users_Role CHECK (Role IN ('Admin', 'Teacher', 'Student', 'Staff')),

        -- قيد التحقق من طول اسم المستخدم
        CONSTRAINT CK_Users_UserName_Length CHECK (LEN(UserName) >= 3)
    );
END
GO

-- 2. الفهارس (Indexes)
-- فهرس فريد مصفى لاسم المستخدم (يسمح بإعادة استخدام الاسم إذا حُذف الحساب منطقياً)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Users_UserName_Active' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Users_UserName_Active
    ON Users (UserName)
    WHERE IsDeleted = 0;
END
GO

-- فهرس لتسريع تسجيل الدخول والبحث عن المستخدمين النشطين
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Login' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_Login
    ON Users (UserName, IsActive)
    INCLUDE (PasswordHash, FullName, Role)
    WHERE IsDeleted = 0;
END
GO
