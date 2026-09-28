namespace OC_System_Training.Infrastructure.Persistence.Repositories.BaseRepository;

// تعليق تدريبي: واجهة الـ Repository الأساسية (IBaseRepository)
// تحدد جميع عمليات الوصول للبيانات والـ CRUD القياسية عبر الـ Stored Procedures
public interface IBaseRepository<TView, TForm, TUpdate, TFilter>
{
    /// <summary>جلب أول سجل يطابق قيمة عمود محدد</summary>
    Task<TView?> GetFirstAsync(string columnName, object value, string? viewName = null);

    /// <summary>جلب سجل مفرد بالمعرف</summary>
    Task<TView?> Get(long id, string? procedureName = null);

    /// <summary>جلب قائمة مرقمة مع إجمالي عدد السجلات المطابقة</summary>
    Task<(List<TView>? data, int totalCount)> GetAll(TFilter filter, string? procedureName = null);

    /// <summary>إضافة سجل جديد وإرجاع السطر المضاف من الـ View</summary>
    Task<TView?> Add(TForm form, long userId, string? procedureName = null);

    /// <summary>تعديل سجل موجود وإرجاع السطر المعدل من الـ View</summary>
    Task<TView?> Update(long id, TUpdate update, long userId, string? procedureName = null);

    /// <summary>حذف سجل بالمعرف (حذف منطقي Soft Delete)</summary>
    Task<bool> Delete(long id, long userId, string? procedureName = null);

    /// <summary>التحقق من عدم تكرار قيمة في عمود معين مع إمكانية استثناء سجل محدد</summary>
    Task<bool> IsDuplicateAsync(string columnName, object value, long? excludeId = null);
}
