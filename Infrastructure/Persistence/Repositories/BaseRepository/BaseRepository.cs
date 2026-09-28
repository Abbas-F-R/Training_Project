using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using Dapper;
using OC_System_Training.Shared.Attributes;

namespace OC_System_Training.Infrastructure.Persistence.Repositories.BaseRepository;

// تعليق تدريبي: كاش لحفظ خصائص الـ DTOs لتفادي استدعاء الـ Reflection في كل طلب
internal static class RepositoryCache
{
    public static readonly ConcurrentDictionary<Type, List<PropertyInfo>> ParameterPropertiesCache = new();
}

// تعليق تدريبي: الـ Repository الأساسي (BaseRepository)
// يوفر التطبيق العملي الكامل لكافة عمليات الـ CRUD عبر استدعاء الـ Stored Procedures في Dapper
// ويطبق المعايير الصارمة:
// 1. عدم كتابة أي SQL خام داخل الـ C#
// 2. استخدام Transactions عند الإضافة والتعديل والحذف
// 3. قراءة البيانات دوماً من الـ View المقابل للجدول (vw_{TableName})
public abstract class BaseRepository<TView, TForm, TUpdate, TFilter>(DapperContext context, string tableName)
    : IBaseRepository<TView, TForm, TUpdate, TFilter>
{
    protected readonly DapperContext Context = context;
    protected readonly string TableName = tableName;

    /// <summary>
    /// التراجع الآمن عن المعاملة (Transaction) في حال حدوث خطأ دون التسبب بخطأ إضافي
    /// إذا كان الإجراء المخزن قد قام بعمل Rollback بالفعل
    /// </summary>
    private static void RollbackQuietly(IDbTransaction transaction)
    {
        try
        {
            transaction.Rollback();
        }
        catch (InvalidOperationException)
        {
            // تم التراجع عنها داخل الإجراء المخزن مسبقاً
        }
    }

    /// <summary>
    /// استخراج خصائص الكائن وتحويلها إلى معلمات Dapper مع استثناء الخصائص الموسومة بـ [IgnoreParameter]
    /// </summary>
    protected DynamicParameters GetDynamicParameters(object? obj)
    {
        if (obj == null) return new DynamicParameters();

        var type = obj.GetType();
        var properties = RepositoryCache.ParameterPropertiesCache.GetOrAdd(type, t =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
             .Where(p => p.GetCustomAttribute<IgnoreParameterAttribute>() == null)
             .ToList());

        var parameters = new DynamicParameters();
        foreach (var prop in properties)
        {
            parameters.Add(prop.Name, prop.GetValue(obj));
        }

        return parameters;
    }

    public virtual async Task<TView?> GetFirstAsync(string columnName, object value, string? viewName = null)
    {
        var targetView = viewName ?? $"vw_{TableName}";
        using var connection = Context.CreateConnection();
        return (await connection.QueryAsync<TView>(
            "Base_GetFirst",
            new { TableName = targetView, ColumnName = columnName, Value = value.ToString() },
            commandType: CommandType.StoredProcedure
        )).FirstOrDefault();
    }

    public virtual async Task<TView?> Get(long id, string? procedureName = null)
    {
        var proc = procedureName ?? $"{TableName}GetById";
        using var connection = Context.CreateConnection();
        return (await connection.QueryAsync<TView>(
            proc,
            new { Id = id },
            commandType: CommandType.StoredProcedure
        )).FirstOrDefault();
    }

    public virtual async Task<(List<TView>? data, int totalCount)> GetAll(TFilter filter, string? procedureName = null)
    {
        var proc = procedureName ?? $"{TableName}GetAll";
        using var connection = Context.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(
            proc,
            GetDynamicParameters(filter),
            commandType: CommandType.StoredProcedure
        );

        var totalCount = await multi.ReadFirstAsync<int>();
        var data = (await multi.ReadAsync<TView>()).ToList();

        return (data, totalCount);
    }

    public virtual async Task<TView?> Add(TForm form, long userId, string? procedureName = null)
    {
        var proc = procedureName ?? $"{TableName}Insert";
        using var connection = Context.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var parameters = GetDynamicParameters(form);
            parameters.Add("CreatedBy", userId);

            var result = (await connection.QueryAsync<TView>(
                proc,
                parameters,
                commandType: CommandType.StoredProcedure,
                transaction: transaction
            )).FirstOrDefault();

            transaction.Commit();
            return result;
        }
        catch
        {
            RollbackQuietly(transaction);
            throw;
        }
    }

    public virtual async Task<TView?> Update(long id, TUpdate update, long userId, string? procedureName = null)
    {
        var proc = procedureName ?? $"{TableName}Update";
        using var connection = Context.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var parameters = GetDynamicParameters(update);
            parameters.Add("Id", id);
            parameters.Add("UpdatedBy", userId);

            var result = (await connection.QueryAsync<TView>(
                proc,
                parameters,
                commandType: CommandType.StoredProcedure,
                transaction: transaction
            )).FirstOrDefault();

            transaction.Commit();
            return result;
        }
        catch
        {
            RollbackQuietly(transaction);
            throw;
        }
    }

    public virtual async Task<bool> Delete(long id, long userId, string? procedureName = null)
    {
        var proc = procedureName ?? $"{TableName}Delete";
        using var connection = Context.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var result = (await connection.QueryAsync<bool>(
                proc,
                new { Id = id, UserId = userId },
                commandType: CommandType.StoredProcedure,
                transaction: transaction
            )).FirstOrDefault();

            transaction.Commit();
            return result;
        }
        catch
        {
            RollbackQuietly(transaction);
            throw;
        }
    }

    public virtual async Task<bool> IsDuplicateAsync(string columnName, object value, long? excludeId = null)
    {
        using var connection = Context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            "Base_CheckDuplicate",
            new
            {
                TableName = TableName,
                ColumnName = columnName,
                Value = value.ToString(),
                ExcludeId = excludeId
            },
            commandType: CommandType.StoredProcedure
        );
        return count > 0;
    }
}
