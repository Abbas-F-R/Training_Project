-- ============================================================================
-- Features/Students/Sql/01_Students_Tables_Views_Constraints_Indexes.sql
-- تعريف جدول الطلاب، الـ View، القيود الكاملة والفهارس المصفاة
-- ============================================================================

-- 1. جدول الطلاب
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

        -- القيد الأساسي
        CONSTRAINT PK_Students PRIMARY KEY CLUSTERED (Id),

        -- قيد المفتاح الأجنبي مع جدول الأقسام
        CONSTRAINT FK_Students_Departments FOREIGN KEY (DepartmentId) 
            REFERENCES Departments(Id) ON DELETE NO ACTION,

        -- قيد التحقق من المرحلة الدراسية (بين 1 و 6)
        CONSTRAINT CK_Students_Stage CHECK (Stage BETWEEN 1 AND 6),

        -- قيد التحقق من صيغة البريد الإلكتروني
        CONSTRAINT CK_Students_Email CHECK (Email IS NULL OR Email LIKE '%_@__%.__%'),

        -- قيد التحقق من تاريخ الميلاد (لا يمكن أن يكون في المستقبل)
        CONSTRAINT CK_Students_BirthDate CHECK (BirthDate IS NULL OR BirthDate <= GETDATE())
    );
END
GO

-- 2. الفهارس (Indexes)
-- فهرس فريد مصفى للرقم الجامعي (Unique Filtered Index)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Students_StudentCode_Active' AND object_id = OBJECT_ID('Students'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Students_StudentCode_Active
    ON Students (StudentCode)
    WHERE IsDeleted = 0;
END
GO

-- فهرس المفتاح الأجنبي (Foreign Key Index) لتسريع الـ Joins وعمليات البحث حسب القسم
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_DepartmentId' AND object_id = OBJECT_ID('Students'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Students_DepartmentId
    ON Students (DepartmentId)
    INCLUDE (FullName, StudentCode, Stage)
    WHERE IsDeleted = 0;
END
GO

-- فهرس لتسريع البحث باسم الطالب
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_FullName' AND object_id = OBJECT_ID('Students'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Students_FullName
    ON Students (FullName)
    INCLUDE (StudentCode, DepartmentId, Stage)
    WHERE IsDeleted = 0;
END
GO

-- فهرس لتسريع الفلترة حسب المرحلة الدراسية
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_Stage' AND object_id = OBJECT_ID('Students'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Students_Stage
    ON Students (Stage)
    WHERE IsDeleted = 0;
END
GO

-- 3. الـ View المقابل: vw_Students
-- يربط الطالب بالقسم ويسترجع السجلات غير المحذوفة فقط
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
