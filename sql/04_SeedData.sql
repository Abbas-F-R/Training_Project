-- ============================================================================
-- 04_SeedData.sql
-- Student Management System Seed Data
-- Initial Data for Departments, Students, and Users
-- ============================================================================

USE [Training_Project_DB];
GO

-- 1. Ø¥Ø¶Ø§ÙØ© Ø§Ù„Ø£Ù‚Ø³Ø§Ù… Ø§Ù„Ø¯Ø±Ø§Ø³ÙŠØ© Ø§Ù„ØªØ¬Ø±ÙŠØ¨ÙŠØ© (Departments)
IF NOT EXISTS (SELECT 1 FROM Departments WHERE Code = 'CS')
BEGIN
    INSERT INTO Departments (Name, Code, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'Ø¹Ù„ÙˆÙ… Ø§Ù„Ø­Ø§Ø³ÙˆØ¨ (Computer Science)', 'CS', 0, 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Departments WHERE Code = 'SE')
BEGIN
    INSERT INTO Departments (Name, Code, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'Ù‡Ù†Ø¯Ø³Ø© Ø§Ù„Ø¨Ø±Ù…Ø¬ÙŠØ§Øª (Software Engineering)', 'SE', 0, 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Departments WHERE Code = 'IS')
BEGIN
    INSERT INTO Departments (Name, Code, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'Ù†Ø¸Ù… Ø§Ù„Ù…Ø¹Ù„ÙˆÙ…Ø§Øª (Information Systems)', 'IS', 0, 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Departments WHERE Code = 'AI')
BEGIN
    INSERT INTO Departments (Name, Code, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'Ø§Ù„Ø°ÙƒØ§Ø¡ Ø§Ù„Ø§ØµØ·Ù†Ø§Ø¹ÙŠ (Artificial Intelligence)', 'AI', 0, 1, GETDATE());
END
GO

-- 2. Ø¥Ø¶Ø§ÙØ© Ø·Ù„Ø§Ø¨ ØªØ¬Ø±ÙŠØ¨ÙŠÙŠÙ† (Students)
DECLARE @CsId BIGINT = (SELECT TOP 1 Id FROM Departments WHERE Code = 'CS');
DECLARE @SeId BIGINT = (SELECT TOP 1 Id FROM Departments WHERE Code = 'SE');
DECLARE @IsId BIGINT = (SELECT TOP 1 Id FROM Departments WHERE Code = 'IS');

IF NOT EXISTS (SELECT 1 FROM Students WHERE StudentCode = 'STU-2026-001')
BEGIN
    INSERT INTO Students (FullName, StudentCode, Email, PhoneNumber, DepartmentId, Stage, BirthDate, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'Ø¹Ù„ÙŠ Ø£Ø­Ù…Ø¯ Ø­Ø³Ù†', 'STU-2026-001', 'ali.ahmed@univ.edu', '07701234567', @CsId, 3, '2003-05-14', 0, 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Students WHERE StudentCode = 'STU-2026-002')
BEGIN
    INSERT INTO Students (FullName, StudentCode, Email, PhoneNumber, DepartmentId, Stage, BirthDate, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'ÙØ§Ø·Ù…Ø© Ø­ÙŠØ¯Ø± ÙƒØ§Ø¸Ù…', 'STU-2026-002', 'fatima.haidar@univ.edu', '07802345678', @SeId, 4, '2002-11-20', 0, 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Students WHERE StudentCode = 'STU-2026-003')
BEGIN
    INSERT INTO Students (FullName, StudentCode, Email, PhoneNumber, DepartmentId, Stage, BirthDate, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'Ø­Ø³ÙŠÙ† Ù…Ø­Ù…Ø¯ Ø¬ÙˆØ§Ø¯', 'STU-2026-003', 'hussein.m@univ.edu', '07903456789', @IsId, 2, '2004-02-10', 0, 1, GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Students WHERE StudentCode = 'STU-2026-004')
BEGIN
    INSERT INTO Students (FullName, StudentCode, Email, PhoneNumber, DepartmentId, Stage, BirthDate, IsDeleted, CreatedBy, CreatedAt)
    VALUES (N'Ø²ÙŠÙ†Ø¨ Ø¹Ø¨Ø§Ø³ ÙƒØ±ÙŠÙ…', 'STU-2026-004', 'zainab.a@univ.edu', '07704567890', @CsId, 1, '2005-08-25', 0, 1, GETDATE());
END
GO
