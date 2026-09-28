using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OC_System_Training.Features.Students.Dtos;
using OC_System_Training.Features.Students.Services;
using OC_System_Training.Shared.Base;
using OC_System_Training.Shared.Base.dto;

namespace OC_System_Training.Features.Students.Controllers;

// تعليق تدريبي: متحكم إدارة الطلاب (StudentController)
// يرث من GenericController ويحدد سياسة الصلاحيات:
// - استعراض الطلاب، وإضافتهم، وتعديل بياناتهم متاح لجميع المستخدمين المسجلين (Admin أو User)
// - عملية الحذف محصورة حصراً بمسؤولي النظام [Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StudentController(IStudentService service)
    : GenericController<StudentResponse, StudentForm, StudentUpdate, StudentFilter>(service)
{
    /// <summary>جلب بيانات طالب محدد بواسطة المعرف</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<StudentResponse>> Get(long id) => 
        await BaseGet(id);

    /// <summary>جلب قائمة الطلاب بنظام الصفحات والبحث والفلترة</summary>
    [HttpGet]
    public async Task<ActionResult<Response<StudentResponse>>> GetAll([FromQuery] StudentFilter filter) => 
        await BaseGetAll(filter);

    /// <summary>إضافة طالب جديد</summary>
    [HttpPost]
    public async Task<ActionResult<StudentResponse>> Add([FromBody] StudentForm form) => 
        await BaseAdd(form);

    /// <summary>تعديل بيانات طالب موجود</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<StudentResponse>> Update(long id, [FromBody] StudentUpdate update) => 
        await BaseUpdate(id, update);

    /// <summary>حذف طالب (خاص بالمسؤول Admin فقط - حذف منطقي Soft Delete)</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<bool>> Delete(long id) => 
        await BaseDelete(id);
}
