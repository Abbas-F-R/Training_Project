namespace OC_System_Training.Shared.Base.dto;

// تعليق تدريبي: غلاف الاستجابة الموحد للقوائم المرقمة (Paged Lists)
// يوفر للـ Frontend بيانات الصفحة وإجمالي السجلات وعدد الصفحات
public class Response<T>
{
    public List<T> Data { get; set; }
    public int PagesCount { get; set; }
    public int CurrentPage { get; set; }
    public int TotalCount { get; set; }
    public bool IsLast { get; set; }

    public Response(List<T>? data, int currentPage, int totalCount, int pageSize)
    {
        Data = data ?? new List<T>();
        CurrentPage = currentPage;
        TotalCount = totalCount;

        if (pageSize <= 0)
        {
            PagesCount = 0;
            IsLast = true;
            return;
        }

        PagesCount = (totalCount + pageSize - 1) / pageSize;
        IsLast = currentPage >= PagesCount;
    }
}
