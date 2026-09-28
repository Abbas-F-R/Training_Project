// تعليق تدريبي: نقطة انطلاق التطبيق (Program.cs)
// تطبيقاً لمعمارية OC_System، يبقى هذا الملف نظيفاً ومختصراً للغاية
// ويفوض كافة عمليات التسجيل والتهيئة إلى الـ Extensions المخصصة
// تم الاعتماد كلياً على appsettings.json بدلاً من ملفات .env

var builder = WebApplication.CreateBuilder(args);

// 1. تسجيل المتحكمات ومحولات التشفير Sqids و FluentValidation
builder.Services.AddControllersExtension();

// 2. تسجيل الأمان والـ JWT Bearer وإعدادات Swagger
builder.Services.AddSecurityExtension(builder.Configuration);

// 3. تسجيل الخدمات وقاعدة البيانات والـ Auto-DI عبر Scrutor
builder.Services.AddApplicationServices(builder.Configuration);

// 4. تسجيل سياسة الـ CORS
builder.Services.AddCustomCors();

var app = builder.Build();

// 5. التأكد من تهيئة مستخدم Admin الافتراضي
await DatabaseSeeder.SeedAsync(app.Services);

// 6. تشغيل مسار المعالجة الكامل (Middleware, Auth, Swagger, Scalar, Controllers)
app.UseApplicationPipeline();

app.Run();
