-- ============================================================================
-- 02_Views.sql
-- Student Management System Database Views
-- Relational Projection Views with JOINs and Soft-Delete Filters
-- ============================================================================

USE [OC_System_Training_DB];
GO

-- 1. View الخاص بالأقسام: vw_Departments
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

-- 2. View الخاص بالطلاب: vw_Students
-- يربط الطالب بقسمه لإرجاع DepartmentName بدلاً من مجرد المعرف
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
