-- ============================================================================
-- Infrastructure/Persistence/Sql/01_SeedData.sql
-- البيانات الأولية التجريبية (الأقسام، الطلاب، والمستخدمين)
-- ============================================================================

-- 1. إضافة الأقسام الدراسية التجريبية (Departments)
IF NOT EXISTS (SELECT 1 FROM Departments WHERE Code = 'CS')
BEGIN
    INSERT INTO Departments (Name, Code, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'علوم الحاسوب (Computer Science)', 'CS', 0, 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Departments WHERE Code = 'SE')
BEGIN
    INSERT INTO Departments (Name, Code, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'هندسة البرمجيات (Software Engineering)', 'SE', 0, 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Departments WHERE Code = 'IS')
BEGIN
    INSERT INTO Departments (Name, Code, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'نظم المعلومات (Information Systems)', 'IS', 0, 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Departments WHERE Code = 'AI')
BEGIN
    INSERT INTO Departments (Name, Code, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'الذكاء الاصطناعي (Artificial Intelligence)', 'AI', 0, 1, GETDATE());
END
GO

-- 2. إضافة طلاب تجريبيين (Students)
DECLARE @CsId BIGINT = (SELECT TOP 1 Id FROM Departments WHERE Code = 'CS');
DECLARE @SeId BIGINT = (SELECT TOP 1 Id FROM Departments WHERE Code = 'SE');
DECLARE @IsId BIGINT = (SELECT TOP 1 Id FROM Departments WHERE Code = 'IS');

IF NOT EXISTS (SELECT 1 FROM Students WHERE StudentCode = 'STU-2026-001')
BEGIN
    INSERT INTO Students (FullName, StudentCode, Email, PhoneNumber, DepartmentId, Stage, BirthDate, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'علي أحمد حسن', 'STU-2026-001', 'ali.ahmed@univ.edu', '07701234567', @CsId, 3, '2003-05-14', 0, 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Students WHERE StudentCode = 'STU-2026-002')
BEGIN
    INSERT INTO Students (FullName, StudentCode, Email, PhoneNumber, DepartmentId, Stage, BirthDate, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'فاطمة حيدر كاظم', 'STU-2026-002', 'fatima.haidar@univ.edu', '07802345678', @SeId, 4, '2002-11-20', 0, 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Students WHERE StudentCode = 'STU-2026-003')
BEGIN
    INSERT INTO Students (FullName, StudentCode, Email, PhoneNumber, DepartmentId, Stage, BirthDate, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'حسين محمد جواد', 'STU-2026-003', 'hussein.m@univ.edu', '07903456789', @IsId, 2, '2004-02-10', 0, 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Students WHERE StudentCode = 'STU-2026-004')
BEGIN
    INSERT INTO Students (FullName, StudentCode, Email, PhoneNumber, DepartmentId, Stage, BirthDate, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'زينب عباس كريم', 'STU-2026-004', 'zainab.a@univ.edu', '07704567890', @CsId, 1, '2005-08-25', 0, 1, GETDATE());
END
GO
