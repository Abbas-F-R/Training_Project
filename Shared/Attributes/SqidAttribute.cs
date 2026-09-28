namespace OC_System_Training.Shared.Attributes;

// تعليق تدريبي: يوضع هذا الـ Attribute على معرّفات الـ Id (من نوع long)
// لتشفيرها عند الإرسال للـ Client وفك تشفيرها تلقائياً عند استقبال الطلب

/// <summary>
/// وسم الخاصية ليتم تشفيرها وفك تشفيرها تلقائياً باستخدام خوارزمية Sqids
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class SqidAttribute : Attribute
{
}
