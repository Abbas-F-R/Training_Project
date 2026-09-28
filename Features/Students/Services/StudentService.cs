using OC_System_Training.Features.Students.Dtos;
using OC_System_Training.Infrastructure.Persistence.Repositories.RepositoryWrapper;
using OC_System_Training.Shared.Attributes;
using OC_System_Training.Shared.Base.dto;
using OC_System_Training.Shared.Constants;

namespace OC_System_Training.Features.Students.Services;

// تعليق تدريبي: تطبيق خدمة الطلاب (StudentService)
// يستعرض استخدام IRepositoryWrapper للوصول إلى أكثر من Repository (الطلاب والأقسام معاً)
// للتحقق من وجود القسم الدراسي وفحص عدم تكرار الرقم الجامعي للطالب
[Scoped]
public class StudentService(IRepositoryWrapper wrapper) : IStudentService
{
    public async Task<ServiceResult<StudentResponse>> Get(ServiceRequest<long> request)
    {
        var item = await wrapper.Student.Get(request.Dto);
        return item != null
            ? ServiceResult<StudentResponse>.Ok(item)
            : ServiceResult<StudentResponse>.Failure(Messages.RecordNotFound);
    }

    public async Task<ServiceResult<List<StudentResponse>>> GetAll(ServiceRequest<StudentFilter> request)
    {
        var (data, totalCount) = await wrapper.Student.GetAll(request.Dto);
        return ServiceResult<List<StudentResponse>>.PagedOk(data, totalCount);
    }

    public async Task<ServiceResult<StudentResponse>> Add(ServiceRequest<StudentForm> request)
    {
        // 1. التحقق من وجود القسم الدراسي المحدد
        var department = await wrapper.Department.Get(request.Dto.DepartmentId);
        if (department == null)
            return ServiceResult<StudentResponse>.Failure(Messages.DepartmentNotFound);

        // 2. التحقق من عدم تكرار الرقم الجامعي
        if (await wrapper.Student.IsDuplicateAsync("StudentCode", request.Dto.StudentCode))
            return ServiceResult<StudentResponse>.Failure(Messages.DuplicateStudentCode);

        // 3. تنفيذ الإضافة وتمرير UserId للتدقيق
        var created = await wrapper.Student.Add(request.Dto, request.UserId);
        return created != null
            ? ServiceResult<StudentResponse>.Ok(created)
            : ServiceResult<StudentResponse>.Failure(Messages.InsertFailed);
    }

    public async Task<ServiceResult<StudentResponse>> Update(long id, ServiceRequest<StudentUpdate> request)
    {
        // 1. التحقق من وجود القسم الدراسي
        var department = await wrapper.Department.Get(request.Dto.DepartmentId);
        if (department == null)
            return ServiceResult<StudentResponse>.Failure(Messages.DepartmentNotFound);

        // 2. التحقق من عدم تكرار الرقم الجامعي مع استثناء السجل الحالي
        if (await wrapper.Student.IsDuplicateAsync("StudentCode", request.Dto.StudentCode, excludeId: id))
            return ServiceResult<StudentResponse>.Failure(Messages.DuplicateStudentCode);

        // 3. تنفيذ التعديل
        var updated = await wrapper.Student.Update(id, request.Dto, request.UserId);
        return updated != null
            ? ServiceResult<StudentResponse>.Ok(updated)
            : ServiceResult<StudentResponse>.Failure(Messages.UpdateFailed);
    }

    public async Task<ServiceResult<bool>> Delete(ServiceRequest<long> request)
    {
        var success = await wrapper.Student.Delete(request.Dto, request.UserId);
        return success
            ? ServiceResult<bool>.Ok(true)
            : ServiceResult<bool>.Failure(Messages.DeleteFailed);
    }
}
