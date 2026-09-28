using OC_System_Training.Features.Auth.Dtos;

namespace OC_System_Training.Features.Auth.Repositories;

// تعليق تدريبي: واجهة مستودع المستخدمين للتحقق من الحسابات والمصادقة
public interface IUserRepository
{
    Task<UserDto?> GetByUserName(string userName);
    Task<UserDto?> GetById(long id);
    Task<UserDto?> Add(UserDto user, long? createdBy = null);
    Task<bool> IsUserNameTaken(string userName);
}
