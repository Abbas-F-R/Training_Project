# مشروع التدريب البرمجي: نظام إدارة الطلاب (Student Management System)
### مبني وفق معمارية وأنماط وقواعد نظام المراسلات الرسمية (OC_System)

---

## 1. فكرة المشروع والهدف التدريبي

هذا المشروع هو تطبيق عملي صُمم لتدريب مطوري الـ Backend الجدد على معمارية وأنماط البرمجة المعتمدة في **OC_System**.
يمثل المشروع تطبيقاً حقيقياً لنظام إدارة الطلاب في كلية أو جامعة، حيث يدير:
- **المستخدمين والمصادقة (Auth & Users):** تسجيل الدخول وتوليد رموز JWT المشفرة، حماية الـ Endpoints، والتعامل مع الحسابات المعطلة.
- **إدارة الصلاحيات (Authorization):** الفصل الدقيق بين رتبتي `Admin` و `User` مع التمييز العملي بين `401 Unauthorized` و `403 Forbidden`.
- **الأقسام الدراسية (Departments):** إدارة الأقسام ورموزها مع توفير نقطة اتصال الـ `Lookup` للقوائم المنسدلة.
- **الطلاب (Students):** إدارة بيانات الطلاب وربطهم بالأقسام والمراحل مع التحقق الصارم من صحة المدخلات.
- **سجل التدقيق والتتبع (Audit Log):** تسجيل تلقائي ذري (In-Transaction) لجميع عمليات الإضافة والتعديل والحذف وأحداث تسجيل الدخول.

---

## 2. التقنيات وحزم العمل المستخدمة

- **.NET 10 Web API** (C# 13 / 14)
- **Dapper 2.1.72:** للتعامل فائق السرعة مع الإجراءات المخزنة (Stored Procedures).
- **Microsoft.Data.SqlClient 7.0.0:** الاتصال بقاعدة بيانات SQL Server.
- **Microsoft.AspNetCore.Authentication.JwtBearer 10.0.5:** المصادقة بالتوكن.
- **FluentValidation.AspNetCore 11.3.0:** التحقق المتقدم والصارم من صحة المدخلات لكل ميزة.
- **BCrypt.Net-Next 4.1.0:** التشفير الآمن لكلمات المرور.
- **Scrutor 7.0.0:** التسجيل التلقائي للتبعيات عبر الـ Attributes (`[Scoped]`, `[Transient]`, `[Singleton]`).
- **Sqids 3.2.1:** تشفير معرفات الـ IDs الرقمية إلى رموز آمنة في الـ URLs والـ JSON.
- **Scalar.AspNetCore 2.13.15:** واجهة توثيق واختبار الـ API التفاعلية الحديثة.
- **Swashbuckle.AspNetCore 10.1.7:** واجهة Swagger UI التقليدية.
- **xUnit & FluentAssertions:** مشروع الاختبارات الآلية الشاملة للتحقق من السلوك الفعلي.

---

## 3. طريقة إعداد قاعدة البيانات (Database Architecture & Setup)

تم تنظيم ملفات قاعدة البيانات وفق **معمارية الميزات (Feature-Based SQL)**:

```text
├── Infrastructure/Persistence/Sql/
│   ├── 00_Base_Procedures.sql       <-- إجراءات النظام الأساسية المشتركة (Base_CheckDuplicate, Base_GetFirst)
│   └── 01_SeedData.sql              <-- البيانات الأولية التجريبية
├── Features/AuditLogs/Sql/
│   ├── 01_AuditLogs_Tables_Indexes.sql  <-- جدول سجل التدقيق والفهارس المخصصة
│   └── 02_AuditLogs_Procedures.sql      <-- إجراءات الإضافة والاستعلام المرقم للمسؤول
├── Features/Auth/Sql/
│   ├── 01_Users_Tables_Constraints_Indexes.sql  <-- جدول المستخدمين والقيود وفهرس الدخول المصفى
│   └── 02_Users_Procedures.sql                  <-- إجراءات المصادقة والمستخدمين مع تدقيق الإضافة
├── Features/Departments/Sql/
│   ├── 01_Departments_Tables_Views_Constraints_Indexes.sql  <-- جدول الأقسام والـ View والفهارس
│   └── 02_Departments_Procedures.sql                        <-- إجراءات الـ CRUD مع التدقيق المدمج والـ Lookup
├── Features/Students/Sql/
│   ├── 01_Students_Tables_Views_Constraints_Indexes.sql     <-- جدول الطلاب، الـ View، FK، والقيود
│   └── 02_Students_Procedures.sql                           <-- إجراءات الـ CRUD مع التدقيق المدمج
└── sql/
    └── MasterMigration.sql          <-- سكريبت التهيئة الشامل المنظم بالترتيب الصحيح
```

### خطوات التنفيذ:
يمكنك تشغيل سكريبت `sql/MasterMigration.sql` مباشرة في SSMS أو Azure Data Studio لتهيئة قاعدة البيانات كاملة دفعة واحدة، أو تنفيذ السكريبتات الموزعة حسب الميزات.
كما يتضمن التطبيق `DatabaseSeeder.cs` لإنشاء حساب المدير `admin` تلقائياً عند أول تشغيل.

---

## 4. إعدادات المشروع والتشغيل (Configuration & Run)

يعتمد المشروع على إعدادات ASP.NET Core القياسية عبر `appsettings.json` و `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OC_System_Training_DB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "SecretKey": "SuperSecretKeyForOCSystemTrainingProject2026SecureMin32Bytes!"
  },
  "Swagger": {
    "Enabled": true
  },
  "Scalar": {
    "Enabled": true
  }
}
```

> [!NOTE]
> تم ضبط الاتصال الافتراضي بقاعدة بيانات محلية تدعم مصادقة ويندوز (`Trusted_Connection=True`). يمكنك تعديل اسم السيرفر المحلي بحسب إعدادات SQL Server لديك (مثل `localhost` أو `.` أو `localhost\SQLEXPRESS`).

### تشغيل المشروع عبر Terminal:
```bash
dotnet restore
dotnet build
dotnet run
```

### تشغيل حزمة الاختبارات الآلية (Automated Tests):
```bash
dotnet test tests/OC_System_Training.Tests/OC_System_Training.Tests.csproj
```

---

## 5. روابط التوثيق التفاعلية (Swagger & Scalar)

عند تشغيل المشروع على المنفذ الافتراضي (`http://localhost:5207`):
- **واجهة Scalar الحديثة (مستحسنة):**  
  [http://localhost:5207/scalar/v1](http://localhost:5207/scalar/v1)
- **واجهة Swagger UI التقليدية:**  
  [http://localhost:5207/swagger](http://localhost:5207/swagger)

---

## 6. بيانات تسجيل الدخول وتجربة الـ Endpoints

تم تزويد النظام بحساب مسؤول افتراضي:
- **اسم المستخدم (UserName):** `admin`
- **كلمة المرور (Password):** `Admin@12345`
- **الرتبة (Role):** `Admin`

### خريطة الصلاحيات للاختبار:
- **عمليات الـ Lookup العامة:** `GET /api/department/lookup` (متاح للجميع بعد المصادقة).
- **عمليات الطلاب:** `GET`, `POST`, `PUT` متاحة للمستخدم العادي والمسؤول؛ بينما `DELETE /api/student/{id}` محصورة بالمسؤول **Admin** (ترجع `403 Forbidden` للمستخدم العادي).
- **سجلات التدقيق:** `GET /api/auditlog` محصورة بالمسؤول **Admin**.

---

## 7. فهرس وثائق التدريب المرفقة في مجلد `Documentation/`

| الوثيقة | الوصف ومحتوى الدليل |
|---------|---------------------|
| [`Audit-Log-Guide.md`](Documentation/Audit-Log-Guide.md) | شرح تصميم جدول التدقيق، والتدقيق الذري المدمج بالـ SPs، ومصفوفة تدقيق الإجراءات. |
| [`Authentication-Guide.md`](Documentation/Authentication-Guide.md) | شرح الـ JWT، وتشفير BCrypt، وسياق المستخدم CurrentUser، وتدقيق الدخول. |
| [`Authorization-Guide.md`](Documentation/Authorization-Guide.md) | الفرق بين 401 و 403، وإدارة رتبتي Admin و User، ومصفوفة حماية الـ Endpoints. |
| [`Validation-Guide.md`](Documentation/Validation-Guide.md) | هرم التحقق الثلاثي: FluentValidation، وقواعد عمل الخدمة، وقيود قاعدة البيانات. |
| [`Security-Flow-Guide.md`](Documentation/Security-Flow-Guide.md) | دورة حياة الطلب المتكاملة خطوة بخطوة من الوصول وحتى الـ DB وسجل التدقيق. |
| [`Architecture-Guide.md`](Documentation/Architecture-Guide.md) | شرح المعمارية الرأسية والطبقات وترابط المكونات. |
| [`Training-Roadmap.md`](Documentation/Training-Roadmap.md) | خريطة طريق خطوة بخطوة لبناء أي ميزة جديدة من الصفر للمتدربين. |
| [`Database-Guide.md`](Documentation/Database-Guide.md) | الدليل الشامل لتصميم الجداول والقيود والفهارس المصفاة والـ Views والـ SPs. |
| [`Base-Files-Guide.md`](Documentation/Base-Files-Guide.md) | شرح عميق للملفات الثابتة في Base وExtensions وShared وإجابة الأسئلة الـ 9. |
| [`API-Testing-Guide.md`](Documentation/API-Testing-Guide.md) | دليل عملي لاختبار الـ API بنماذج الطلبات والاستجابات وحالات التحقق والخطأ. |
