-- ============================================================================
-- 01_Tables.sql
-- مشروع OC_System التدريبي (نظام إدارة الطلاب - Student Management System)
-- إنشاء جداول النظام الأساسية، الفهارس المصفاة، وقيود سلامة البيانات (Constraints)
-- ============================================================================

USE [OC_System_Training_DB];
GO

-- ============================================================================
-- 1. جدول سجل التدقيق والتتبع (AuditLogs) - غير قابل للتعديل (Append-Only)
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

-- فهارس تحسين أداء استعلامات التدقيق
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

-- ============================================================================
-- 2. جدول المستخدمين (Users) لتسجيل الدخول والمصادقة (Authentication)
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

-- فهرس فريد مصفى لاسم المستخدم (يسمح بإعادة استخدام الاسم إذا حُذف الحساب منطقياً)
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

-- ============================================================================
-- 3. جدول الأقسام الدراسية (Departments)
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
        CONSTRAINT CK_Departments_Name CHECK (LEN(Name) >= 3),
        CONSTRAINT CK_Departments_Code CHECK (LEN(Code) >= 2 AND Code NOT LIKE '% %')
    );
END
GO

-- فهرس فريد مصفى لكود القسم (يسمح بإعادة استخدام الكود عند الحذف المنطقي)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Departments_Code_Active' AND object_id = OBJECT_ID('Departments'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Departments_Code_Active
    ON Departments (Code)
    WHERE IsDeleted = 0;
END
GO

-- ============================================================================
-- 4. جدول الطلاب (Students)
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
        CONSTRAINT FK_Students_Departments FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
        CONSTRAINT CK_Students_Stage CHECK (Stage BETWEEN 1 AND 6),
        CONSTRAINT CK_Students_BirthDate CHECK (BirthDate IS NULL OR BirthDate < GETDATE()),
        CONSTRAINT CK_Students_Email CHECK (Email IS NULL OR Email LIKE '%_@__%.__%')
    );
END
GO

-- فهرس فريد مصفى لكود الطالب
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Students_StudentCode_Active' AND object_id = OBJECT_ID('Students'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Students_StudentCode_Active
    ON Students (StudentCode)
    WHERE IsDeleted = 0;
END
GO

-- فهرس لتسريع الربط بين الطلاب والأقسام
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_DepartmentId' AND object_id = OBJECT_ID('Students'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Students_DepartmentId
    ON Students (DepartmentId)
    INCLUDE (FullName, StudentCode, Stage)
    WHERE IsDeleted = 0;
END
GO
