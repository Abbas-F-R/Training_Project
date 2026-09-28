namespace OC_System_Training.Shared.Attributes;

// تعليق تدريبي: يوضع هذا الـ Attribute على أي خاصية (Property) داخل الـ DTO
// لا نريد تمريرها كـ Parameter إلى الإجراء المخزن (Stored Procedure) في Dapper

/// <summary>
/// استثناء الخاصية من معلمات الاستعلام في Dapper
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class IgnoreParameterAttribute : Attribute
{
}
