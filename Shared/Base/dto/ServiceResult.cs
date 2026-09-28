namespace OC_System_Training.Shared.Base.dto;

// تعليق تدريبي: كائن نتيجة الخدمة (ServiceResult)
// يوحد مخرجات طبقة الـ Service سواء كانت نجاحاً أو فشلاً مع مفتاح الخطأ
public class ServiceResult<T>
{
    public T? Data { get; set; }
    public int TotalCount { get; set; }
    public string? Error { get; set; }
    public bool Success => Error == null;
    public bool IsSuccess => Success;

    public ServiceResult()
    {
    }

    // منشئ النجاح لكائن واحد
    public ServiceResult(T? data)
    {
        Data = data;
    }

    // منشئ النجاح لقائمة مرقمة مع إجمالي السجلات
    public ServiceResult(T? data, int totalCount)
    {
        Data = data;
        TotalCount = totalCount;
    }

    // منشئ الفشل مع مفتاح الخطأ
    public ServiceResult(string? error)
    {
        Error = error;
    }

    public static ServiceResult<T> Ok(T? data) => new(data);
    public static ServiceResult<T> PagedOk(T? data, int totalCount) => new(data, totalCount);
    public static ServiceResult<T> Failure(string error) => new(error);
}
