-- ============================================================================
-- Infrastructure/Persistence/Sql/00_Base_Procedures.sql
-- الإجراءات المساعدة المشتركة للنظام الأساسي (Base Repository Procedures)
-- ============================================================================

-- 1. فحص تكرار قيمة في عمود معين (تستخدمه IsDuplicateAsync في BaseRepository)
CREATE OR ALTER PROCEDURE Base_CheckDuplicate
    @TableName  NVARCHAR(100),
    @ColumnName NVARCHAR(100),
    @Value      NVARCHAR(255),
    @ExcludeId  BIGINT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @Count INT;

    SET @SQL = N'SELECT @CountOut = COUNT(*) FROM [' + @TableName + N'] 
                 WHERE [' + @ColumnName + N'] = @Val 
                   AND IsDeleted = 0 
                   AND (@ExId IS NULL OR Id <> @ExId)';

    EXEC sp_executesql @SQL, 
        N'@Val NVARCHAR(255), @ExId BIGINT, @CountOut INT OUTPUT', 
        @Val = @Value, @ExId = @ExcludeId, @CountOut = @Count OUTPUT;

    SELECT ISNULL(@Count, 0);
END
GO

-- 2. جلب أول سجل يطابق قيمة عمود (تستخدمه GetFirstAsync في BaseRepository)
CREATE OR ALTER PROCEDURE Base_GetFirst
    @TableName  NVARCHAR(100),
    @ColumnName NVARCHAR(100),
    @Value      NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @SQL NVARCHAR(MAX);

    SET @SQL = N'SELECT TOP 1 * FROM [' + @TableName + N'] 
                 WHERE [' + @ColumnName + N'] = @Val';

    EXEC sp_executesql @SQL, N'@Val NVARCHAR(255)', @Val = @Value;
END
GO
