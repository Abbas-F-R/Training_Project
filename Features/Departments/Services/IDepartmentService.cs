using OC_System_Training.Features.Departments.Dtos;
using OC_System_Training.Shared.Base;
using OC_System_Training.Shared.Base.dto;

namespace OC_System_Training.Features.Departments.Services;

// تعليق تدريبي: واجهة خدمة الأقسام الدراسية (IDepartmentService)
public interface IDepartmentService : IBaseService<DepartmentResponse, DepartmentForm, DepartmentUpdate, DepartmentFilter>
{
    Task<ServiceResult<List<DepartmentResponse>>> Lookup();
}
