using System.ComponentModel.DataAnnotations;

namespace OC_System_Training.Shared.Base.dto;

// تعليق تدريبي: كلاس الفلترة والترقيم الأساسي (Pagination)
// ترث منه جميع كلاسات الفلترة في الـ Features لتوحيد باراميترات رقم الصفحة وحجمها
public class BaseFilter
{
    /// <summary>
    /// عدد العناصر في الصفحة الواحدة (افتراضياً 10، وبحد أقصى 100)
    /// </summary>
    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// رقم الصفحة الحالية (يبدأ من 1)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be at least 1.")]
    public int PageNumber { get; set; } = 1;
}
