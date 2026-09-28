namespace OC_System_Training.Shared.Base.dto;

// تعليق تدريبي: كائن تغليف الطلب (ServiceRequest)
// يمرر بيانات الـ DTO مصحوبة ببيانات المستخدم المنفذ (UserId, UserName, Role, Lang)
// لكي لا تعتمد طبقة الـ Service على HttpContext مباشرة
public class ServiceRequest<T>
{
    public T Dto { get; set; } = default!;
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Lang { get; set; } = "ar";

    public ServiceRequest() { }

    public ServiceRequest(T dto, long userId, string userName, string role, string lang)
    {
        Dto = dto;
        UserId = userId;
        UserName = userName;
        Role = role;
        Lang = string.IsNullOrWhiteSpace(lang) ? "ar" : lang;
    }
}
