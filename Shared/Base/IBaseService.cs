namespace OC_System_Training.Shared.Base;

// تعليق تدريبي: واجهة الخدمة الأساسية (IBaseService)
// تحدد العقود المشتركة لعمليات الـ CRUD القياسية. تم استخدام Default Interface Methods
// لرمي NotSupportedException افتراضياً، بحيث لا تجبر الخدمة المشتقة إلا على تطبيق ما تدعمه فعلياً
public interface IBaseService<TView, TForm, TUpdate, TFilter>
{
    /// <summary>جلب سجل مفرد بالمعرف</summary>
    Task<ServiceResult<TView>> Get(ServiceRequest<long> request) =>
        throw new NotSupportedException("Get is not supported for this service");

    /// <summary>جلب قائمة مرقمة مع الفلاتر</summary>
    Task<ServiceResult<List<TView>>> GetAll(ServiceRequest<TFilter> request) =>
        throw new NotSupportedException("GetAll is not supported for this service");

    /// <summary>إضافة سجل جديد</summary>
    Task<ServiceResult<TView>> Add(ServiceRequest<TForm> request) =>
        throw new NotSupportedException("Add is not supported for this service");

    /// <summary>تعديل سجل موجود</summary>
    Task<ServiceResult<TView>> Update(long id, ServiceRequest<TUpdate> request) =>
        throw new NotSupportedException("Update is not supported for this service");

    /// <summary>حذف سجل بالمعرف (حذف منطقي Soft Delete)</summary>
    Task<ServiceResult<bool>> Delete(ServiceRequest<long> request) =>
        throw new NotSupportedException("Delete(id) is not supported for this service");
}
