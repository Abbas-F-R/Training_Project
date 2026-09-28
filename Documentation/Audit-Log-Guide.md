# دليل سجل التدقيق والتتبع (Audit Log Guide)
### التتبع المالي والإداري للعمليات، سلامة البيانات، وتدقيق الإجراءات المخزنة

---

## 1. ما هو سجل التدقيق (Audit Log) ولماذا نحتاجه؟

في أي نظام Backend إنتاجي حقيقي، لا يكفي أن يقوم النظام بتنفيذ العمليات؛ بل يجب أن يكون قادراً على الإجابة بشكل قاطع عن الأسئلة التالية:
- **من** قام بهذه العملية؟ (`UserId`)
- **ماذا** فعل بالضبط؟ (`Action`: إضافة، تعديل، حذف، محاولة دخول)
- **على أي سجل** حدث التغيير؟ (`EntityName` و `EntityId`)
- **متى** حدث ذلك بدقة؟ (`CreatedAt`)
- **ما هي البيانات** التي أُدخلت أو عُدلت؟ (`Changes`)
- **هل نجحت** العملية أم فشلت؟ (`IsSuccess`)

### الفروق الجوهرية التي يجب أن يفهمها المتدرب:
| المعيار | سجل التدقيق (Audit Log) | سجلات التطبيق (Application Logs) | سجلات الأخطاء (Error Logs) |
|---------|-------------------------|-----------------------------------|----------------------------|
| **الجمهور المستهدف** | مدراء النظام، المدققون الماليون والأمنيون | مطورو البرمجيات وفريق الـ DevOps | فريق الصيانة وحل المشاكل الفنية |
| **مكان الحفظ** | جدول دائم في قاعدة البيانات (`AuditLogs`) | ملفات نصية أو أدوات مثل Seq/Elasticsearch | أدوات مراقبة مثل Sentry أو ملفات Log |
| **طبيعة البيانات** | أحداث العمل والحركات (Business Events) | تفاصيل مسار المعالجة وسرعة التنفيذ | رسائل الاستثناءات والـ Stack Traces |
| **القابلية للحذف** | **غير قابل للحذف نهائياً (Append-Only)** | تُحذف أو تُؤرشف دورياً لتوفير المساحة | تُحذف بعد حل المشكلة |

---

## 2. تصميم جدول `AuditLogs` في النظام التدريبي

```sql
CREATE TABLE AuditLogs
(
    Id          BIGINT IDENTITY(1,1) NOT NULL,
    UserId      BIGINT               NULL,         -- معرف المستخدم (NULL لمحاولات الدخول الفاشلة)
    Action      NVARCHAR(50)         NOT NULL,     -- INSERT, UPDATE, DELETE, LOGIN_SUCCESS, LOGIN_FAILED
    EntityName  NVARCHAR(100)        NOT NULL,     -- Departments, Students, Auth, Users
    EntityId    NVARCHAR(100)        NULL,         -- المعرف الخاص بالسجل المتأثر
    Changes     NVARCHAR(MAX)        NULL,         -- تفاصيل التغييرات بصيغة JSON
    IpAddress   NVARCHAR(50)         NULL,         -- عنوان IP العميل
    UserAgent   NVARCHAR(255)        NULL,         -- المتصفح أو التطبيق المنفذ
    IsSuccess   BIT                  DEFAULT 1 NOT NULL, -- حالة نجاح العملية
    CreatedAt   DATETIME             DEFAULT GETDATE() NOT NULL,

    CONSTRAINT PK_AuditLogs PRIMARY KEY CLUSTERED (Id)
);
```

### أسباب اختيار هذه الحقول:
1. **`UserId` (يقبل NULL):** نحتاجه لربط العملية بمرتكبها، وسمحنا بقيم `NULL` لأن بعض العمليات الهامة تحدث قبل اكتمال المصادقة (مثل محاولة تسجيل دخول باسم مستخدم غير مسجل).
2. **`Action` و `EntityName`:** يتيحان التصفية الفورية لكافة العمليات (مثلاً: استخراج جميع عمليات `DELETE` التي تمت على كيان `Students`).
3. **`Changes` (JSON خفيف ومحدد):** نسجل الحقول المهمة فقط التي تساعد في استرجاع أو مراجعة ما تم، مع **حظر مطلق** لتسجيل كلمات المرور، أو رموز الـ JWT، أو الـ Hashes الحساسة.
4. **طبيعة الجدول Append-Only:** لا يحتوي الجدول على أعمدة `UpdatedAt` أو `IsDeleted`، ولا توجد أي Stored Procedure لتعديل أو حذف سجلات التدقيق، لمنع التلاعب بالسجلات حتى من قبل المبرمجين.

---

## 3. ربط سجل التدقيق بالـ Stored Procedures (التدقيق الذري In-Transaction)

### القاعدة الذهبية: الذرية (Atomicity) في التدقيق
ماذا يحدث لو قمنا بتعديل بيانات طالب، ثم تعطل الخادم قبل أن نكتب سجل الـ Audit Log؟  
النتيجة كارثية: بيانات تغيرت في النظام دون أي أثر للمستخدم الذي غيرها!  
لذلك، نعتمد أسلوب **In-Transaction Audit**:
- يتم إدراج سجل الـ Audit داخل نفس الـ Stored Procedure ونفس المعاملة (Transaction) التي تنفذ التغيير.
- إذا نجحت الإضافة، نجح التدقيق معها حتماً.
- إذا حدث خطأ أو مخالفة قيود (Constraint Violation)، يتراجع الـ SQL Server عن الإضافة والتدقيق معاً عبر `ROLLBACK`.

### مثال واقعي من كود المشروع (`StudentsInsert`):
```sql
CREATE OR ALTER PROCEDURE StudentsInsert
    @FullName     NVARCHAR(150),
    @StudentCode  NVARCHAR(50),
    @Email        NVARCHAR(100) = NULL,
    @PhoneNumber  NVARCHAR(30)  = NULL,
    @DepartmentId BIGINT,
    @Stage        INT = 1,
    @BirthDate    DATE = NULL,
    @CreatedBy    BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NewId BIGINT;

    -- 1. تنفيذ التغيير الأساسي
    INSERT INTO Students (FullName, StudentCode, Email, PhoneNumber, DepartmentId, Stage, BirthDate, IsDeleted, CreatedBy, CreatedAt)
    VALUES (@FullName, @StudentCode, @Email, @PhoneNumber, @DepartmentId, @Stage, @BirthDate, 0, @CreatedBy, GETDATE());

    SET @NewId = SCOPE_IDENTITY();

    -- 2. تسجيل التدقيق في نفس المعاملة فوراً
    INSERT INTO AuditLogs (UserId, Action, EntityName, EntityId, Changes, IsSuccess, CreatedAt)
    VALUES (
        @CreatedBy,
        'INSERT',
        'Students',
        CAST(@NewId AS NVARCHAR(100)),
        CONCAT(
            N'{"FullName":"', REPLACE(@FullName, '"', '\"'),
            N'","StudentCode":"', REPLACE(@StudentCode, '"', '\"'),
            N'","DepartmentId":', @DepartmentId,
            N',"Stage":', @Stage,
            N'}'
        ),
        1,
        GETDATE()
    );

    SELECT * FROM vw_Students WHERE Id = @NewId;
END
GO
```

---

## 4. مصفوفة تدقيق جميع الـ Stored Procedures في المشروع

| اسم الإجراء المخزن | نوع العملية | هل يتم تدقيقه؟ | سبب القرار |
|-------------------|-------------|----------------|------------|
| `DepartmentsInsert` | `INSERT` | **نعم (داخل الـ SP)** | عملية تغيير بيانات وإضافة قسم جديد تؤثر على سير الكلية. |
| `DepartmentsUpdate` | `UPDATE` | **نعم (داخل الـ SP)** | تغيير بيانات القسم أو رمزه يجب توثيقه. |
| `DepartmentsDelete` | `DELETE` | **نعم (داخل الـ SP)** | الحذف المنطقي لقسم دراسي عملية حرجة أمنياً. |
| `DepartmentsGetById` | `SELECT` | **لا** | عمليات القراءة الفردية لا تغير الحالة ولا تستهلك مساحة التتبع دون مبرر. |
| `DepartmentsGetAll` | `SELECT` | **لا** | استعراض الأقسام بنظام الصفحات لا يتطلب حفظ في Audit Log. |
| `DepartmentsLookup` | `SELECT` | **لا** | قراءة سريعة للقوائم المنسدلة لا تبرر إنشاء ملايين السجلات. |
| `StudentsInsert` | `INSERT` | **نعم (داخل الـ SP)** | تسجيل طالب جديد في قاعدة البيانات. |
| `StudentsUpdate` | `UPDATE` | **نعم (داخل الـ SP)** | تعديل بيانات الطالب الشخصية أو مرحلته الدراسية. |
| `StudentsDelete` | `DELETE` | **نعم (داخل الـ SP)** | حذف طالب منطقياً من النظام. |
| `StudentsGetById` | `SELECT` | **لا** | قراءة بيانات طالب. |
| `StudentsGetAll` | `SELECT` | **لا** | بحث وتصفية قائمة الطلاب. |
| `UsersInsert` | `INSERT` | **نعم (داخل الـ SP)** | إنشاء مستخدم جديد في النظام (يسجل الدور والاسم دون الـ Hash). |
| `UsersGetByUserName` | `SELECT` | **لا** | فحص حساب المستخدم أثناء تسجيل الدخول. |
| `AuditLogsInsert` | `INSERT` | **لا (هو نفسه التدقيق)** | إجراء تسجيل التدقيق (لمنع الدوران اللانهائي). |
| `AuditLogsGetAll` | `SELECT` | **لا** | استعراض السجلات من قبل المدير المسؤول. |
| **أحداث تسجيل الدخول** | `LOGIN` | **نعم (عبر الـ Service)** | تسجيل `LOGIN_SUCCESS` و `LOGIN_FAILED` لكشف محاولات الاختراق. |

---

## 5. استعراض سجلات التدقيق في الـ API (خاص بالـ Admin)
- **المسار:** `GET /api/auditlog?pageNumber=1&pageSize=20`
- **الحماية:** `[Authorize(Roles = "Admin")]` (أي مستخدم برتبة `User` يحصل على `403 Forbidden`).
- **نموذج الاستجابة:**
```json
{
  "data": [
    {
      "id": "UkLWZg9D",
      "userId": "UkLWZg9D",
      "action": "INSERT",
      "entityName": "Students",
      "entityId": "1",
      "changes": "{\"FullName\":\"علي أحمد حسن\",\"StudentCode\":\"STU-2026-001\",\"DepartmentId\":1,\"Stage\":3}",
      "ipAddress": null,
      "userAgent": null,
      "isSuccess": true,
      "createdAt": "2026-09-24T00:15:30"
    },
    {
      "id": "aBcD1234",
      "userId": null,
      "action": "LOGIN_FAILED",
      "entityName": "Auth",
      "entityId": "hacker_attempt",
      "changes": "{\"Reason\":\"InvalidCredentials\"}",
      "ipAddress": null,
      "userAgent": null,
      "isSuccess": false,
      "createdAt": "2026-09-24T00:14:12"
    }
  ],
  "pagesCount": 1,
  "currentPage": 1,
  "totalCount": 2,
  "isLast": true
}
```
