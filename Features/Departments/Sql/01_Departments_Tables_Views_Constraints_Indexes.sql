-- ============================================================================
-- Features/Departments/Sql/01_Departments_Tables_Views_Constraints_Indexes.sql
-- تعريف جدول الأقسام، الـ View، القيود والفهارس المصفاة
-- ============================================================================

-- 1. جدول الأقسام الدراسية
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

        -- القيد الأساسي
        CONSTRAINT PK_Departments PRIMARY KEY CLUSTERED (Id),

        -- قيد التحقق من رمز القسم
        CONSTRAINT CK_Departments_Code CHECK (LEN(Code) >= 2),

        -- قيد التحقق من اسم القسم
        CONSTRAINT CK_Departments_Name CHECK (LEN(TRIM(Name)) > 0)
    );
END
GO

-- 2. الفهارس (Indexes)
-- فهرس فريد مصفى لرمز القسم (يسمح بإعادة استخدام الرمز في حال حذف القسم القديم)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Departments_Code_Active' AND object_id = OBJECT_ID('Departments'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Departments_Code_Active
    ON Departments (Code)
    WHERE IsDeleted = 0;
END
GO

-- فهرس لتسريع البحث بالاسم
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Departments_Name' AND object_id = OBJECT_ID('Departments'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Departments_Name
    ON Departments (Name)
    INCLUDE (Code)
    WHERE IsDeleted = 0;
END
GO

-- 3. الـ View المقابل: vw_Departments
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
