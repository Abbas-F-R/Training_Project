using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OC_System_Training.Features.Departments.Dtos;
using OC_System_Training.Features.Departments.Services;
using OC_System_Training.Shared.Base;
using OC_System_Training.Shared.Base.dto;

namespace OC_System_Training.Features.Departments.Controllers;

// تعليق تدريبي: متحكم الأقسام الدراسية (DepartmentController)
// يرث من GenericController ويحدد قواعد الصلاحيات بدقة:
// - القراءة واستخدام الـ Lookup متاح لجميع المستخدمين المصادق عليهم
// - عمليات الإضافة والتعديل والحذف محصورة حصراً بمسؤولي النظام [Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DepartmentController(IDepartmentService service)
    : GenericController<DepartmentResponse, DepartmentForm, DepartmentUpdate, DepartmentFilter>(service)
{
    /// <summary>جلب قسم دراسي بواسطة المعرف</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DepartmentResponse>> Get(long id) => 
        await BaseGet(id);

    /// <summary>جلب قائمة الأقسام بنظام الترقيم والفلترة</summary>
    [HttpGet]
    public async Task<ActionResult<Response<DepartmentResponse>>> GetAll([FromQuery] DepartmentFilter filter) => 
        await BaseGetAll(filter);

    /// <summary>جلب قائمة الأقسام كـ Lookup للقوائم المنسدلة (بديل NotPaged)</summary>
    [HttpGet("lookup")]
    public async Task<ActionResult<List<DepartmentResponse>>> Lookup() => 
        Ok(await service.Lookup());

    /// <summary>إضافة قسم دراسي جديد (خاص بالمسؤول Admin فقط)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DepartmentResponse>> Add([FromBody] DepartmentForm form) => 
        await BaseAdd(form);

    /// <summary>تعديل بيانات قسم دراسي (خاص بالمسؤول Admin فقط)</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DepartmentResponse>> Update(long id, [FromBody] DepartmentUpdate update) => 
        await BaseUpdate(id, update);

    /// <summary>حذف قسم دراسي (خاص بالمسؤول Admin فقط - حذف منطقي Soft Delete)</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<bool>> Delete(long id) => 
        await BaseDelete(id);
}
