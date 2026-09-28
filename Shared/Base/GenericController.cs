using Microsoft.AspNetCore.Mvc;

namespace OC_System_Training.Shared.Base;

// تعليق تدريبي: المتحكم العام (GenericController)
// يرث من BaseController ويقبل الخدمة المقابلة عبر IBaseService
// يوفر الدوال الأساسية (BaseGet, BaseGetAll, BaseAdd, BaseUpdate, BaseDelete)
// بحيث لا يحتاج المطور لكتابة استدعاءات الـ Service المتكررة داخل كل Controller
public abstract class GenericController<TView, TForm, TUpdate, TFilter>(
    IBaseService<TView, TForm, TUpdate, TFilter> service) : BaseController
{
    protected readonly IBaseService<TView, TForm, TUpdate, TFilter> Service = service;

    /// <summary>جلب سجل مفرد بالمعرف</summary>
    protected async Task<ActionResult<TView>> BaseGet(long id) => 
        Ok(await Service.Get(CreateServiceRequest(id)));

    /// <summary>جلب قائمة مرقمة بناءً على كائن الفلترة</summary>
    protected async Task<ActionResult<Response<TView>>> BaseGetAll([FromQuery] TFilter filter) =>
        Ok(await Service.GetAll(CreateServiceRequest(filter)), (filter as BaseFilter)?.PageNumber ?? 1, (filter as BaseFilter)?.PageSize ?? 10);

    /// <summary>إضافة سجل جديد من نموذج الإدخال</summary>
    protected async Task<ActionResult<TView>> BaseAdd([FromBody] TForm form) =>
        Ok(await Service.Add(CreateServiceRequest(form)));

    /// <summary>تعديل سجل موجود</summary>
    protected async Task<ActionResult<TView>> BaseUpdate(long id, [FromBody] TUpdate update) =>
        Ok(await Service.Update(id, CreateServiceRequest(update)));

    /// <summary>حذف سجل بالمعرف</summary>
    protected async Task<ActionResult<bool>> BaseDelete(long id) => 
        Ok(await Service.Delete(CreateServiceRequest(id)));
}
