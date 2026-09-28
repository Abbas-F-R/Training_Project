-- ============================================================================
-- Features/Auth/Sql/02_Users_Procedures.sql
-- إجراءات إدارة المستخدمين والمصادقة
-- ============================================================================

-- 1. جلب مستخدم باسم المستخدم
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

-- 2. إنشاء مستخدم جديد (مع تسجيل التدقيق وتجنب حفظ كلمة المرور في التدقيق)
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
