namespace OC_System_Training.Shared.Attributes;

// تعليق تدريبي: هذه الـ Attributes تستخدم مع مكتبة Scrutor
// للحقن التلقائي للتبعيات (Auto-DI) دون الحاجة لتسجيل كل كلاس يدوياً في Program.cs

/// <summary>
/// تسجيل الكلاس كـ Scoped (يُنشأ كائن واحد لكل HTTP Request)
/// يُستخدم عادة للخدمات (Services) والـ Repositories
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ScopedAttribute : Attribute
{
}

/// <summary>
/// تسجيل الكلاس كـ Transient (يُنشأ كائن جديد في كل مرة يُطلب فيها)
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TransientAttribute : Attribute
{
}

/// <summary>
/// تسجيل الكلاس كـ Singleton (يُنشأ كائن واحد فقط على مستوى التطبيق بالكامل)
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SingletonAttribute : Attribute
{
}
