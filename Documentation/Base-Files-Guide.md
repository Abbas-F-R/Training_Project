# دليل الملفات الثابتة والأساسية (Base Files & Extensions Guide)
### الشرح المنهجي الشامل للمكونات الثابتة في Base وExtensions وShared

هذا الدليل يُمثل المرجع الأهم للمطورين المتدربين؛ حيث يشرح بدقة كل ملف ثابت موجود في النظام وإجابة الأسئلة التسعة الجوهرية لكل ملف.

---

## فهرس الملفات المشروحة
1. `BaseController.cs`
2. `GenericController.cs`
3. `IBaseService.cs`
4. `CurrentUser.cs`
5. `BaseFilter.cs`
6. `Response.cs`
7. `ServiceRequest.cs`
8. `ServiceResult.cs`
9. `BaseRepository.cs`
10. `IBaseRepository.cs`
11. `RepositoryWrapper.cs`
12. `DapperContext.cs`
13. `PipelineExtension.cs`
14. `ApplicationSecurityExtension.cs`
15. `ApplicationServicesExtension.cs`
16. `ControllersExtension.cs`
17. `DiAttributes.cs` (`[Scoped]`, `[Transient]`, `[Singleton]`)
18. `SqidAttribute.cs` & `SqidCodec.cs`
19. `IgnoreParameterAttribute.cs`
20. `ErrorMessagesUtils.cs`
21. `PasswordHasher.cs`

---

## 1. `BaseController.cs`
* **المسار:** `Shared/Base/BaseController.cs`
1. **ما وظيفة هذا الملف؟** المتحكم الأساسي لجميع الـ Controllers في النظام؛ يوفر استخراج سياق المستخدم الحالي (`Id`, `UserName`, `Role`, `Lang`)، ودوال `Ok()` الذكية التي تفك تغليف `ServiceResult<T>` وترجم الأخطاء تلقائياً.
2. **لماذا نحتاجه؟** لتفادي كتابة كود استخراج الـ Claims والتحقق من الأخطاء وترجمتها في كل Controller.
3. **ما المشكلة التي يحلها؟** القضاء على تكرار فحص `if (result.Error != null)` وتحويل الأخطاء يدوياً إلى نصوص عربية أو إنجليزية.
4. **متى يتم استدعاؤه؟** في كل مرة يتم فيها استدعاء Endpoint في أي Controller.
5. **من يستدعيه؟** إطار عمل ASP.NET Core عند توجيه الـ HTTP Request.
6. **ما الملفات التي يعتمد عليها؟** `ICurrentUser`, `ServiceResult<T>`, `Response<T>`, `ErrorMessagesUtils`.
7. **هل يحتاج المتدرب إلى تعديله عند إنشاء Feature جديدة؟** **لا إطلاقاً.** يرث منه فقط.
8. **مثال على استخدامه:**
   ```csharp
   [HttpGet("{id}")]
   public async Task<ActionResult<StudentResponse>> Get(long id) =>
       Ok(await _service.Get(CreateServiceRequest(id)));
   ```
9. **الأخطاء الشائعة عند استخدامه:** استدعاء `base.Ok()` الأصلي لـ .NET بدلاً من `Ok(ServiceResult)`، مما يحرم الاستجابة من التغليف الموحد وترجمة الخطأ.

---

## 2. `GenericController.cs`
* **المسار:** `Shared/Base/GenericController.cs`
1. **ما وظيفة هذا الملف؟** توفير التنفيذ الجاهز لعمليات الـ CRUD الأساسية (Get, GetAll, Add, Update, Delete) على مستوى المتحكم.
2. **لماذا نحتاجه؟** لتقليص حجم كود الـ Controller لأي كيان جديد بنسبة 80%.
3. **ما المشكلة التي يحلها؟** كتابة نفس الـ 5 Methods في كل Controller من الصفر.
4. **متى يتم استدعاؤه؟** عند استدعاء أي من دوال الـ CRUD الأساسية.
5. **من يستدعيه؟** الـ Controller المشتق منه في الـ Feature.
6. **ما الملفات التي يعتمد عليها؟** `BaseController`, `IBaseService<TView, TForm, TUpdate, TFilter>`.
7. **هل يحتاج المتدرب إلى تعديله عند إنشاء Feature جديدة؟** **لا إطلاقاً.**
8. **مثال على استخدامه:**
   ```csharp
   public class StudentController(IStudentService service)
       : GenericController<StudentResponse, StudentForm, StudentUpdate, StudentFilter>(service)
   {
       [HttpGet]
       public async Task<ActionResult<Response<StudentResponse>>> GetAll([FromQuery] StudentFilter filter)
           => await BaseGetAll(filter);
   }
   ```
9. **الأخطاء الشائعة:** نسيان تمرير الخدمة `service` إلى الـ Base Constructor.

---

## 3. `IBaseService.cs`
* **المسار:** `Shared/Base/IBaseService.cs`
1. **ما وظيفة هذا الملف؟** واجهة الخدمة الأساسية التي تحدد عقود عمليات الـ CRUD.
2. **لماذا نحتاجه؟** لتوحيد واجهات الخدمات وربطها تلقائياً مع `GenericController`.
3. **ما المشكلة التي يحلها؟** اختلاف أسماء دوال الخدمات بين المطورين (أحدهم يسمي `Find` والآخر `GetById` والآخر `Fetch`).
4. **متى يتم استدعاؤه؟** عندما يستدعي الـ Controller دالة الخدمة.
5. **من يستدعيه؟** `GenericController` أو الـ Controllers المخصصة.
6. **ما الملفات التي يعتمد عليها؟** `ServiceResult<T>`, `ServiceRequest<T>`.
7. **هل يحتاج المتدرب إلى تعديله عند إنشاء Feature جديدة؟** **لا.** ترث واجهة الخدمة الخاصة بالميزة (`IStudentService`) منه فقط.
8. **مثال على استخدامه:**
   ```csharp
   public interface IStudentService : IBaseService<StudentResponse, StudentForm, StudentUpdate, StudentFilter> { }
   ```
9. **الأخطاء الشائعة:** محاولة تطبيق دوال غير مدعومة دون الحاجة إليها (الملف يستخدم Default Interface Methods لرمي `NotSupportedException` للدوال غير المطبقة).

---

## 4. `CurrentUser.cs`
* **المسار:** `Shared/Base/CurrentUser.cs`
1. **ما وظيفة هذا الملف؟** استخراج هوية المستخدم وبياناته (`UserId`, `UserName`, `FullName`, `Role`, `Lang`) من الـ Claims الخاصة بالـ JWT Token.
2. **لماذا نحتاجه؟** لتوفير كائن حقن آمن (`ICurrentUser`) متاح في أي طبقة بدون تمرير `HttpContext`.
3. **ما المشكلة التي يحلها؟** صعوبة وتكرار قراءة الـ Claims يدوياً في كل دالة.
4. **متى يتم استدعاؤه؟** عند كل طلب HTTP يتم التحقق فيه من هوية المستخدم.
5. **من يستدعيه؟** `BaseController` وأي خدمة تطلب حقن `ICurrentUser`.
6. **ما الملفات التي يعتمد عليها؟** `IHttpContextAccessor`, `[Scoped]`.
7. **هل يحتاج المتدرب إلى تعديله عند إنشاء Feature جديدة؟** **لا.**
8. **مثال على استخدامه:**
   ```csharp
   public class AuditService(ICurrentUser currentUser) {
       var actorId = currentUser.UserId;
   }
   ```
9. **الأخطاء الشائعة:** محاولة قراءة `currentUser.UserId` في مسار مسموح به بدون تسجيل دخول (`[AllowAnonymous]`)، حيث يكون المعرف حينها `0`.

---

## 5. `BaseFilter.cs`
* **المسار:** `Shared/Base/dto/BaseFilter.cs`
1. **ما وظيفة هذا الملف؟** DTO أساسي يحمل إعدادات الترقيم: `PageNumber` (افتراضياً 1) و `PageSize` (افتراضياً 10).
2. **لماذا نحتاجه؟** لضمان أن كل طلبات استرجاع القوائم تلتزم بمعايير الترقيم والحد الأقصى (100).
3. **ما المشكلة التي يحلها؟** طلب استرجاع آلاف السجلات دفعة واحدة مما قد يسبب انهيار الذاكرة (Memory Outage).
4. **متى يتم استدعاؤه؟** عند استدعاء استعلامات الـ GET الخاصة بالقوائم.
5. **من يستدعيه؟** الـ ASP.NET Core Query Binder.
6. **ما الملفات التي يعتمد عليها؟** `System.ComponentModel.DataAnnotations`.
7. **هل يحتاج المتدرب إلى تعديله؟** **لا.** يرث منه في كلاس الفلتر الخاص بالميزة (`StudentFilter : BaseFilter`).
8. **مثال:**
   ```csharp
   public class StudentFilter : BaseFilter {
       public string? FullName { get; set; }
   }
   ```
9. **الأخطاء الشائعة:** نسيان وراثة `BaseFilter` في كلاس الفلتر، مما يؤدي لتعطيل الترقيم في `GenericController`.

---

## 6. `Response.cs`
* **المسار:** `Shared/Base/dto/Response.cs`
1. **ما وظيفة هذا الملف؟** الغلاف الموحد لاستجابات القوائم المرقمة الموجهة للعميل (Frontend).
2. **لماذا نحتاجه؟** لتوحيد شكل الـ JSON لجميع القوائم في النظام ليحتوي على: `data`, `pagesCount`, `currentPage`, `totalCount`, `isLast`.
3. **ما المشكلة التي يحلها؟** اختلاف شكل الاستجابات في الواجهات الأمامية بين شاشات النظام المختلفة.
4. **متى يتم استدعاؤه؟** في دالة `BaseController.Ok(ServiceResult<List<T>>)`.
5. **من يستدعيه؟** `BaseController`.
6. **ما الملفات التي يعتمد عليها؟** لا يعتمد على ملفات أخرى.
7. **هل يحتاج المتدرب إلى تعديله؟** **لا.**
8. **مثال على شكله في JSON:**
   ```json
   {
     "data": [...],
     "pagesCount": 2,
     "currentPage": 1,
     "totalCount": 15,
     "isLast": false
   }
   ```
9. **الأخطاء الشائعة:** إنشاء كلاس استجابة مخصص لكل شاشة بدلاً من استخدام `Response<T>`.

---

## 7. `ServiceRequest.cs`
* **المسار:** `Shared/Base/dto/ServiceRequest.cs`
1. **ما وظيفة هذا الملف؟** تغليف كائن البيانات الداخل إلى الخدمة (DTO) مصحوباً ببيانات المستخدم المنفذ (`UserId`, `UserName`, `Role`, `Lang`).
2. **لماذا نحتاجه؟** لعزل طبقة الخدمات (Services) عن طبقة الـ Web (HttpContext).
3. **ما المشكلة التي يحلها؟** جعل طبقة الخدمات قابلة للاختبار (Testable) بدون الحاجة إلى Mocking لـ HttpContext.
4. **متى يتم استدعاؤه؟** عندما يقوم الـ Controller بتجهيز الطلب عبر `CreateServiceRequest(dto)`.
5. **من يستدعيه؟** الـ Controllers.
6. **ما الملفات التي يعتمد عليها؟** لا يعتمد على ملفات أخرى.
7. **هل يحتاج المتدرب إلى تعديله؟** **لا.**
8. **مثال:**
   ```csharp
   var result = await _service.Add(CreateServiceRequest(form));
   ```
9. **الأخطاء الشائعة:** تمرير الـ Form مباشرة للـ Service دون تغليفه بـ `CreateServiceRequest`.

---

## 8. `ServiceResult.cs`
* **المسار:** `Shared/Base/dto/ServiceResult.cs`
1. **ما وظيفة هذا الملف؟** توحيد نتيجة أي عملية في طبقة الخدمات؛ يحتوي على `Data` و `TotalCount` و `Error` و `Success`.
2. **لماذا نحتاجه؟** لتفادي رمي الاستثناءات (Exceptions) لإرجاع أخطاء التحقق العادية مثل "السجل غير موجود".
3. **ما المشكلة التي يحلها؟** تحسين أداء النظام وتوحيد معالجة النجاح والفشل دون Try/Catch مفرط.
4. **متى يتم استدعاؤه؟** في نهاية تنفيذ أي دالة داخل طبقة الـ Service.
5. **من يستدعيه؟** طبقة الخدمات.
6. **ما الملفات التي يعتمد عليها؟** لا يعتمد على ملفات أخرى.
7. **هل يحتاج المتدرب إلى تعديله؟** **لا.**
8. **مثال:**
   ```csharp
   // في حال النجاح المفرد
   return ServiceResult<StudentResponse>.Ok(student);
   // في حال القائمة المرقمة
   return ServiceResult<List<StudentResponse>>.PagedOk(list, totalCount);
   // في حال الفشل
   return ServiceResult<StudentResponse>.Failure(Messages.RecordNotFound);
   ```
9. **الأخطاء الشائعة:** استخدام `Ok(list)` بدلاً من `PagedOk(list, totalCount)` في القوائم المرقمة، مما يسبب فقدان `TotalCount`.

---

## 9. `BaseRepository.cs`
* **المسار:** `Infrastructure/Persistence/Repositories/BaseRepository/BaseRepository.cs`
1. **ما وظيفة هذا الملف؟** مستودع البيانات الأساسي الذي يوفر الربط التلقائي بين دوال الـ C# والإجراءات المخزنة عبر Dapper.
2. **لماذا نحتاجه؟** لأن 90% من عمليات الـ CRUD متطابقة، وهذا الملف ينفذها بـ 0 أسطر كود داخل الـ Repository المشتق!
3. **ما المشكلة التي يحلها؟** كتابة أكواد Dapper وفتح الاتصالات وإدارة الـ Transactions يدوياً في كل مستودع.
4. **متى يتم استدعاؤه؟** عندما تستدعي الـ Service دوال المستودع (`Get`, `GetAll`, `Add`, `Update`, `Delete`).
5. **من يستدعيه؟** الخدمات عبر الـ Repositories.
6. **ما الملفات التي يعتمد عليها؟** `DapperContext`, `IBaseRepository`, `[IgnoreParameter]`, `BaseFilter`.
7. **هل يحتاج المتدرب إلى تعديله؟** **لا إطلاقاً.**
8. **مثال:**
   ```csharp
   [Scoped]
   public class StudentRepository(DapperContext context)
       : BaseRepository<StudentResponse, StudentForm, StudentUpdate, StudentFilter>(context, DbConstants.Tables.Students),
         IStudentRepository { }
   ```
9. **الأخطاء الشائعة:** تسمية الإجراءات المخزنة بأسماء تخالف القاعدة `{TableName}Verb` (مثلاً تسمية `GetStudent` بدلاً من `StudentsGetById`).

---

## 10. `IBaseRepository.cs`
* **المسار:** `Infrastructure/Persistence/Repositories/BaseRepository/IBaseRepository.cs`
1. **ما وظيفة هذا الملف؟** واجهة المستودع الأساسي التي تحدد دوال الـ CRUD على مستوى البيانات.
2. **لماذا نحتاجه؟** لتطبيق مبدأ عزل الاعتماديات والـ Loose Coupling بين الـ Services والـ Repositories.
3. **ما المشكلة التي يحلها؟** توحيد توقيع الدوال في جميع الـ Repositories.
4. **متى يتم استدعاؤه؟** عند كتابة الواجهات المشتقة للكيانات.
5. **من يستدعيه؟** الـ Repositories والـ Services.
6. **ما الملفات التي يعتمد عليها؟** لا يعتمد على ملفات أخرى.
7. **هل يحتاج المتدرب إلى تعديله؟** **لا.**
8. **مثال:**
   ```csharp
   public interface IStudentRepository : IBaseRepository<StudentResponse, StudentForm, StudentUpdate, StudentFilter> { }
   ```

---

## 11. `RepositoryWrapper.cs`
* **المسار:** `Infrastructure/Persistence/Repositories/RepositoryWrapper/RepositoryWrapper.cs`
1. **ما وظيفة هذا الملف؟** نقطة مركزية تجمع كافة الـ Repositories في كائن واحد يسهل حقنه.
2. **لماذا نحتاجه؟** عندما تحتاج الخدمة التعامل مع أكثر من جدول (مثلاً `StudentService` تحتاج فحص `Departments` و `Students`).
3. **ما المشكلة التي يحلها؟** تجنب وجود عشرات الباراميترات في منشئ الخدمة (Constructor Overload).
4. **متى يتم استدعاؤه؟** عند حقن `IRepositoryWrapper` في الـ Services.
5. **من يستدعيه؟** الـ Services.
6. **ما الملفات التي يعتمد عليها؟** واجهات المستودعات (`IStudentRepository`, `IDepartmentRepository`, `IUserRepository`).
7. **هل يحتاج المتدرب إلى تعديله عند إضافة Feature جديدة؟** **نعم،** يضيف خاصية المستودع الجديد في `IRepositoryWrapper` و `RepositoryWrapper`.
8. **مثال:**
   ```csharp
   public class StudentService(IRepositoryWrapper wrapper) {
       var dept = await wrapper.Department.Get(deptId);
       var student = await wrapper.Student.Add(form, userId);
   }
   ```
9. **الأخطاء الشائعة:** نسيان تسجيل المستودع الجديد داخل الـ Wrapper عند إنشائه.

---

## 12. `DapperContext.cs`
* **المسار:** `Infrastructure/Persistence/DapperContext.cs`
1. **ما وظيفة هذا الملف؟** مصنع اتصالات قاعدة البيانات (Connection Factory).
2. **لماذا نحتاجه؟** لإدارة نص الاتصال `CONNECTION_STRING` وإنشاء اتصالات `IDbConnection` جديدة عند الطلب.
3. **ما المشكلة التي يحلها؟** تسريب نص الاتصال أو إدارة الاتصالات المفتوحة بصورة غير صحيحة.
4. **متى يتم استدعاؤه؟** عند تنفيذ أي استعلام في الـ Repositories.
5. **من يستدعيه؟** `BaseRepository` والمستودعات المخصصة.
6. **ما الملفات التي يعتمد عليها؟** `Microsoft.Data.SqlClient`.
7. **هل يحتاج المتدرب إلى تعديله؟** **لا.**
8. **مثال:**
   ```csharp
   using var connection = context.CreateConnection();
   ```
9. **الأخطاء الشائعة:** نسيان تغليف الاتصال بـ `using var` مما يسبب تسريب الاتصالات (Connection Leaks).

---

## 13. `PipelineExtension.cs`
* **المسار:** `Shared/Extensions/PipelineExtension.cs`
1. **ما وظيفة هذا الملف؟** تجميع وضبط ترتيب وسائط المعالجة (Middlewares) في مسار واحد `app.UseApplicationPipeline()`.
2. **لماذا نحتاجه؟** لتنظيم `Program.cs` وضمان أن الترتيب المعماري الحرج للـ Middlewares لا يتعرض للخطأ.
3. **ما المشكلة التي يحلها؟** الترتيب الخاطئ للميدلوير (مثلاً وضع `UseAuthorization` قبل `UseAuthentication` أو `UserContextMiddleware`).
4. **متى يتم استدعاؤه؟** عند بدء تشغيل التطبيق في `Program.cs`.
5. **من يستدعيه؟** `Program.cs`.
6. **ما الملفات التي يعتمد عليها؟** `UserContextMiddleware`, `Scalar.AspNetCore`, `CorsExtension`.
7. **هل يحتاج المتدرب إلى تعديله؟** **لا.**
8. **مثال:**
   ```csharp
   app.UseApplicationPipeline();
   ```

---

## 14. `ApplicationSecurityExtension.cs`
* **المسار:** `Shared/Extensions/ApplicationSecurityExtension.cs`
1. **ما وظيفة هذا الملف؟** ضبط إعدادات المصادقة بـ JWT Bearer وتجهيز Swagger لدعم التوكن.
2. **لماذا نحتاجه؟** لتأمين التطبيق وضمان أن الطلبات تمر بالتحقق الأمني الصحيح.
3. **ما المشكلة التي يحلها؟** عزل كود الأمان المعقد عن `Program.cs`.
4. **متى يتم استدعاؤه؟** عند تشغيل التطبيق في مرحلة تسجيل الخدمات.
5. **من يستدعيه؟** `Program.cs`.
6. **ما الملفات التي يعتمد عليها؟** `Microsoft.AspNetCore.Authentication.JwtBearer`.
7. **هل يحتاج المتدرب إلى تعديله؟** **لا.**
8. **مثال:**
   ```csharp
   builder.Services.AddSecurityExtension(builder.Configuration);
   ```

---

## 15. `ApplicationServicesExtension.cs`
* **المسار:** `Shared/Extensions/ApplicationServicesExtension.cs`
1. **ما وظيفة هذا الملف؟** تسجيل الخدمات التلقائي عبر **Scrutor**.
2. **لماذا نحتاجه؟** لتسجيل أي كلاس يحمل `[Scoped]` أو `[Transient]` أو `[Singleton]` تلقائياً.
3. **ما المشكلة التي يحلها؟** نسيان إضافة `services.AddScoped<IStudentService, StudentService>()` في `Program.cs`.
4. **متى يتم استدعاؤه؟** عند إعداد الخدمات في `Program.cs`.
5. **من يستدعيه؟** `Program.cs`.
6. **ما الملفات التي يعتمد عليها؟** `Scrutor`, `DapperContext`.
7. **هل يحتاج المتدرب إلى تعديله؟** **لا.**
8. **مثال:**
   ```csharp
   builder.Services.AddApplicationServices(builder.Configuration);
   ```

---

## 16. `ControllersExtension.cs`
* **المسار:** `Shared/Extensions/ControllersExtension.cs`
1. **ما وظيفة هذا الملف؟** ضبط إعدادات المتحكمات، مسارات الـ URLs، وربط محولات تشفير الـ Sqids.
2. **لماذا نحتاجه؟** للتعامل التلقائي مع المعرفات المشفرة في الـ URLs ونصوص الـ JSON.
3. **ما المشكلة التي يحلها؟** تحويل الـ Sqid إلى رقم long والعكس دون كتابة كود تحويل يدوي في كل دالة.
4. **متى يتم استدعاؤه؟** في `Program.cs`.
5. **من يستدعيه؟** `Program.cs`.
6. **ما الملفات التي يعتمد عليها؟** `SqidModelBinderProvider`, `SqidJsonConverterFactory`.
7. **هل يحتاج المتدرب إلى تعديله؟** **لا.**

---

## 17. `DiAttributes.cs`
* **المسار:** `Shared/Attributes/DiAttributes.cs`
1. **ما وظيفة هذا الملف؟** يوفر الـ Attributes الثلاثة (`[Scoped]`, `[Transient]`, `[Singleton]`).
2. **لماذا نحتاجه؟** لتوجيه Scrutor لتسجيل الكلاس بدورة الحياة المطلوبة.
3. **ما المشكلة التي يحلها؟** التسجيل اليدوي المرهق للخدمات.
4. **متى يتم استدعاؤه؟** أثناء قراءة المترجم للكود وبدء التطبيق.
5. **من يستدعيه؟** ميزة فحص الـ Assembly في Scrutor.
6. **ما الملفات التي يعتمد عليها؟** لا يعتمد على ملفات أخرى.
7. **هل يحتاج المتدرب إلى تعديله؟** **لا،** ولكن **يجب عليه استخدام `[Scoped]` على كل Service و Repository يكتبه.**
8. **مثال:**
   ```csharp
   [Scoped]
   public class StudentService : IStudentService { }
   ```
9. **الأخطاء الشائعة:** نسيان وضع `[Scoped]` على الكلاس، مما يؤدي لخطأ runtime شهير: `Unable to resolve service for type...`.

---

## 18. `SqidAttribute.cs` & `SqidCodec.cs`
* **المسار:** `Shared/Attributes/SqidAttribute.cs` و `Shared/Helper/SqidCodec.cs`
1. **ما وظيفة هذا الملف؟** تشفير الـ IDs (مثل `1` -> `UkLWZg9D`) لحماية معرفات قاعدة البيانات من التخمين أو التتبع (Insecure Direct Object Reference).
2. **لماذا نحتاجه؟** لحماية النظام أمنياً وعدم كشف التسلسل الرقمي الداخلي للبيانات.
3. **ما المشكلة التي يحلها؟** هجمات التخمين للـ IDs عبر استدعاءات متتالية `id=1, id=2...`.
4. **متى يتم استدعاؤه؟** عند استقبال أو إرسال أي كائن DTO يحتوي على خاصية موسومة بـ `[Sqid]`.
5. **من يستدعيه؟** محولات الـ JSON ومزودات الـ ModelBinding.
6. **ما الملفات التي يعتمد عليها؟** حزمة `Sqids`.
7. **هل يحتاج المتدرب إلى تعديله؟** **لا،** ولكنه يضع `[Sqid]` على حقول الـ ID في الـ DTOs.
8. **مثال:**
   ```csharp
   [Sqid]
   public long DepartmentId { get; set; }
   ```

---

## 19. `IgnoreParameterAttribute.cs`
* **المسار:** `Shared/Attributes/IgnoreParameterAttribute.cs`
1. **ما وظيفة هذا الملف؟** وسم الحقول في الـ DTOs التي لا نريد إرسالها إلى الإجراء المخزن (Stored Procedure) في Dapper.
2. **لماذا نحتاجه؟** عندما يحتوي الـ DTO على حقول حسابية أو للعرض فقط لا يقبلها الـ SP.
3. **ما المشكلة التي يحلها؟** خطأ Dapper الشهير: `Procedure expects parameter @X which was not supplied` أو العكس عند إرسال باراميتر زائد يرفضه الـ SP.
4. **متى يتم استدعاؤه؟** في دالة `BaseRepository.GetDynamicParameters`.
5. **من يستدعيه؟** `BaseRepository`.
6. **ما الملفات التي يعتمد عليها؟** لا يعتمد على ملفات أخرى.
7. **هل يحتاج المتدرب إلى تعديله؟** **لا،** ويستخدمه عند الحاجة في الـ DTO.
8. **مثال:**
   ```csharp
   [IgnoreParameter]
   public string? DisplayTitle { get; set; }
   ```

---

## 20. `ErrorMessagesUtils.cs`
* **المسار:** `Shared/Utils/ErrorMessagesUtils.cs`
1. **ما وظيفة هذا الملف؟** قاموس ترجمة مفاتيح الأخطاء المركزية إلى العربية والإنجليزية.
2. **لماذا نحتاجه؟** لعزل نصوص واجهات المستخدم عن كود الـ Backend وتسهيل تعدد اللغات (i18n).
3. **ما المشكلة التي يحلها؟** كتابة نصوص أخطاء مبعثرة وبلغات مختلفة داخل الـ Services.
4. **متى يتم استدعاؤه؟** في دالة `BaseController.Ok()` عند فك تغليف `result.Error`.
5. **من يستدعيه؟** `BaseController`.
6. **ما الملفات التي يعتمد عليها؟** `Messages.cs`.
7. **هل يحتاج المتدرب إلى تعديله عند إضافة Feature جديدة؟** **نعم،** يضيف مفتاح الخطأ الجديد وترجمته عند الحاجة لرسالة خطأ جديدة خاصة بالميزة.
8. **مثال:**
   ```csharp
   [Messages.DuplicateStudentCode] = new() {
       ["ar"] = "الرقم الجامعي للطالب مسجل مسبقاً لطالب آخر.",
       ["en"] = "Student code already belongs to another student."
   }
   ```

---

## 21. `PasswordHasher.cs`
* **المسار:** `Shared/Utils/PasswordHasher.cs`
1. **ما وظيفة هذا الملف؟** تشفير كلمات المرور ومطابقتها بخوارزمية BCrypt المنيعة.
2. **لماذا نحتاجه؟** لمنع تخزين كلمات المرور كنصوص صريحة (Plain Text) وحمايتها من هجمات الـ Rainbow Tables.
3. **ما المشكلة التي يحلها؟** مخاطر تسريب قواعد البيانات.
4. **متى يتم استدعاؤه؟** عند تسجيل مستخدم جديد أو عند محاولة تسجيل الدخول.
5. **من يستدعيه؟** `AuthService`.
6. **ما الملفات التي يعتمد عليها؟** `BCrypt.Net-Next`.
7. **هل يحتاج المتدرب إلى تعديله؟** **لا.**
8. **مثال:**
   ```csharp
   var hash = PasswordHasher.Hash("MySecretPassword");
   bool isValid = PasswordHasher.Verify("MySecretPassword", hash);
   ```
