# دليل اختبار الـ API وتجربة العمليات (API Testing Guide)
### دليل عملي للمتدربين لاختبار العمليات باستخدام Scalar و Swagger مع أمثلة Payload واقعية وحالات الصلاحيات والتدقيق

---

## 1. واجهات الاختبار المتاحة

عند تشغيل التطبيق محلياً (`http://localhost:5207`)، يتوفر لديك واجهتان للاختبار التفاعلي:
1. **واجهة Scalar الحديثة (المستحسنة):**  
   رابط الوصول: [http://localhost:5207/scalar/v1](http://localhost:5207/scalar/v1)
2. **واجهة Swagger UI التقليدية:**  
   رابط الوصول: [http://localhost:5207/swagger](http://localhost:5207/swagger)

---

## 2. الخطوة الأولى: تسجيل الدخول وتفعيل التوكن (Authentication)

كل نقاط الاتصال (ما عدا تسجيل الدخول) محمية وتتطلب توكن **JWT Bearer**.

### 1. إرسال طلب تسجيل الدخول (Login)
- **Method:** `POST`
- **URL:** `http://localhost:5207/api/auth/login`
- **Headers:** `Content-Type: application/json`
- **Request Body:**
```json
{
  "userName": "admin",
  "password": "Admin@12345"
}
```

### 2. الاستجابة المتوقعة (200 OK):
```json
{
  "userId": "UkLWZg9D",
  "userName": "admin",
  "fullName": "مدير النظام التدريبي",
  "role": "Admin",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-09-30T20:15:22.8427771Z"
}
```

### 3. تفعيل التوكن في واجهة الاختبار:
- في **Scalar**: اضغط على خانة **Auth** في الشريط الجانبي واختر **Bearer** وضع نص التوكن.
- في **Swagger UI**: اضغط على زر **Authorize** الأخضر في أعلى الصفحة، وأدخل التوكن:  
  `Bearer YOUR_TOKEN_HERE` ثم اضغط **Authorize**.

---

## 3. اختبار عمليات الأقسام الدراسية (Departments CRUD & Lookup)

### A. استعراض الأقسام كـ Lookup للقوائم المنسدلة (Lookup Endpoint)
- **Method:** `GET`
- **URL:** `http://localhost:5207/api/department/lookup`
- **الاستجابة المتوقعة (200 OK):**
```json
[
  {
    "id": "UkLWZg9D",
    "name": "علوم الحاسوب (Computer Science)",
    "code": "CS"
  },
  {
    "id": "gbHJdmfr",
    "name": "هندسة البرمجيات (Software Engineering)",
    "code": "SE"
  }
]
```

### B. عرض جميع الأقسام بنظام الترقيم
- **Method:** `GET`
- **URL:** `http://localhost:5207/api/department?pageNumber=1&pageSize=10`

### C. إضافة قسم دراسي جديد (خاص بالـ Admin)
- **Method:** `POST`
- **URL:** `http://localhost:5207/api/department`
- **Request Body:**
```json
{
  "name": "الأمن السيبراني (Cybersecurity)",
  "code": "CYBER"
}
```

### D. تعديل قسم موجود (خاص بالـ Admin)
- **Method:** `PUT`
- **URL:** `http://localhost:5207/api/department/{id}`
- **Request Body:**
```json
{
  "name": "علوم الحاسوب والبيانات",
  "code": "CSD"
}
```

### E. حذف قسم (خاص بالـ Admin - حذف منطقي Soft Delete)
- **Method:** `DELETE`
- **URL:** `http://localhost:5207/api/department/{id}`
- **الاستجابة:** `true`

---

## 4. اختبار عمليات إدارة الطلاب (Students CRUD)

### A. إضافة طالب جديد
- **Method:** `POST`
- **URL:** `http://localhost:5207/api/student`
- **Request Body:**
```json
{
  "fullName": "كرار حيدر جاسم",
  "studentCode": "STU-2026-105",
  "email": "karrar.haidar@univ.edu",
  "phoneNumber": "07712345678",
  "departmentId": "UkLWZg9D",
  "stage": 2,
  "birthDate": "2004-03-12"
}
```
- **الاستجابة المتوقعة (200 OK):**
```json
{
  "id": "Xm49LK2v",
  "fullName": "كرار حيدر جاسم",
  "studentCode": "STU-2026-105",
  "email": "karrar.haidar@univ.edu",
  "phoneNumber": "07712345678",
  "departmentId": "UkLWZg9D",
  "departmentName": "علوم الحاسوب (Computer Science)",
  "departmentCode": "CS",
  "stage": 2,
  "birthDate": "2004-03-12T00:00:00",
  "createdAt": "2026-09-24T00:10:00"
}
```

### B. جلب بيانات طالب بالمعرف
- **Method:** `GET`
- **URL:** `http://localhost:5207/api/student/{id}`

### C. البحث وتصفية قائمة الطلاب (Search & Paging)
- **Method:** `GET`
- **URL:** `http://localhost:5207/api/student?fullName=علي&stage=3&pageNumber=1&pageSize=5`

### D. تعديل بيانات طالب
- **Method:** `PUT`
- **URL:** `http://localhost:5207/api/student/{id}`
- **Request Body:**
```json
{
  "fullName": "كرار حيدر جاسم الموسوي",
  "studentCode": "STU-2026-105",
  "email": "karrar.mousawi@univ.edu",
  "phoneNumber": "07799998888",
  "departmentId": "UkLWZg9D",
  "stage": 3,
  "birthDate": "2004-03-12"
}
```

### E. حذف طالب (خاص بالـ Admin فقط)
- **Method:** `DELETE`
- **URL:** `http://localhost:5207/api/student/{id}`
- **الاستجابة:** `true`

---

## 5. اختبار استعراض سجلات التدقيق (Audit Logs - Admin Only)

- **Method:** `GET`
- **URL:** `http://localhost:5207/api/auditlog?pageNumber=1&pageSize=20`
- **الاستجابة:** قائمة مرقمة بكافة العمليات التي حدثت في النظام مع التوثيق المالي والإداري للحركات.

---

## 6. اختبار سيناريوهات الأخطاء وفحص الحماية (Security & Validation Scenarios)

### 1. استدعاء بدون توكن (401 Unauthorized)
- استدعاء `GET /api/student` بدون إرفاق التوكن في الـ Header.
- **النتيجة:** `401 Unauthorized`.

### 2. محاولة مستخدم عادي تنفيذ عملية خاصة بالمسؤول (403 Forbidden)
- سجل الدخول بحساب برتبة `User`، ثم حاول حذف قسم أو طالب (`DELETE /api/student/1`) أو فتح سجلات التدقيق (`GET /api/auditlog`).
- **النتيجة:** رفض الطلب بحالة **`403 Forbidden`**.

### 3. إضافة طالب برقم جامعي مكرر (Business Validation Error)
- أرسل طالب جديد يحمل نفس `studentCode` المسجل مسبقاً (`STU-2026-001`).
- **النتيجة:** استجابة `400 Bad Request`:
```json
{
  "message": "الرقم الجامعي للطالب مسجل مسبقاً لطالب آخر."
}
```

### 4. أخطاء بنية المدخلات (FluentValidation)
- أرسل بريد إلكتروني بدون `@` أو رقم هاتف غير عراقي أو مرحلة دراسية رقم 8.
- **النتيجة:** استجابة `400 Bad Request` مع تفاصيل الحقول غير المستوفية للشروط.
