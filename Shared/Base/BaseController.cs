using Microsoft.AspNetCore.Mvc;

namespace OC_System_Training.Shared.Base;

// تعليق تدريبي: المتحكم الأساسي (BaseController)
// يرث من ControllerBase ويوفر دوال مساعدة لجميع المتحكمات في النظام:
// 1. استخراج هوية المستخدم الحالي من ICurrentUser
// 2. إنشاء ServiceRequest موحد يدمج مدخلات العميل مع هوية المستخدم المنفذ
// 3. توحيد الاستجابات (Ok / BadRequest) وترجمة رسائل الخطأ تلقائياً للغة المطلوبة
[ApiController]
public abstract class BaseController : ControllerBase
{
    private ICurrentUser? _currentUser;
    protected ICurrentUser CurrentUser => _currentUser ??= HttpContext.RequestServices.GetRequiredService<ICurrentUser>();

    protected long Id => CurrentUser.UserId;
    protected string UserName => CurrentUser.UserName;
    protected string Role => CurrentUser.Role;
    protected string Lang => CurrentUser.Lang;

    /// <summary>
    /// دمج الـ DTO مع هوية المستخدم الحالي لإنشاء ServiceRequest
    /// </summary>
    protected ServiceRequest<T> CreateServiceRequest<T>(T dto) =>
        new(dto, Id, UserName, Role, Lang);

    /// <summary>
    /// معالجة استجابة كائن مفرد: يُرجع 200 OK مع البيانات أو 400 BadRequest مع رسالة الخطأ المترجمة
    /// </summary>
    protected ObjectResult Ok<T>(ServiceResult<T> result)
    {
        if (result.Error != null)
            return BadRequest(new { Message = result.Error.GetMessage(Lang) });

        return base.Ok(result.Data);
    }

    /// <summary>
    /// معالجة استجابة القوائم: يُغلف القائمة داخل كائن Response<T> الموحد الذي يشمل Pagination Metadata
    /// </summary>
    protected ObjectResult Ok<T>(ServiceResult<List<T>> result, int pageNumber = 1, int pageSize = 10)
    {
        if (result.Error != null)
            return BadRequest(new { Message = result.Error.GetMessage(Lang) });

        return base.Ok(new Response<T>(
            result.Data,
            pageNumber,
            result.TotalCount,
            pageSize
        ));
    }
}
