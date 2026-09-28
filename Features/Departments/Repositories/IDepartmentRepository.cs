using OC_System_Training.Features.Departments.Dtos;
using OC_System_Training.Infrastructure.Persistence.Repositories.BaseRepository;

namespace OC_System_Training.Features.Departments.Repositories;

// تعليق تدريبي: واجهة الـ Repository الخاصة بالأقسام
// توفر عمليات CRUD القياسية إضافة إلى دالة Lookup للقوائم المنسدلة
public interface IDepartmentRepository : IBaseRepository<DepartmentResponse, DepartmentForm, DepartmentUpdate, DepartmentFilter>
{
    Task<List<DepartmentResponse>> Lookup();
}
