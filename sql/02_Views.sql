-- ============================================================================
-- 02_Views.sql
-- Student Management System Database Views
-- Relational Projection Views with JOINs and Soft-Delete Filters
-- ============================================================================

USE [Training_Project_DB];
GO

-- 1. View Ø§Ù„Ø®Ø§Øµ Ø¨Ø§Ù„Ø£Ù‚Ø³Ø§Ù…: vw_Departments
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

-- 2. View Ø§Ù„Ø®Ø§Øµ Ø¨Ø§Ù„Ø·Ù„Ø§Ø¨: vw_Students
-- ÙŠØ±Ø¨Ø· Ø§Ù„Ø·Ø§Ù„Ø¨ Ø¨Ù‚Ø³Ù…Ù‡ Ù„Ø¥Ø±Ø¬Ø§Ø¹ DepartmentName Ø¨Ø¯Ù„Ø§Ù‹ Ù…Ù† Ù…Ø¬Ø±Ø¯ Ø§Ù„Ù…Ø¹Ø±Ù
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
