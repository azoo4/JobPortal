# 🚀 بوابة العمل — Job Portal
### مشروع تخرج كامل | ASP.NET Core MVC 8

---

## 📋 نظرة عامة

**بوابة العمل** هي منصة توظيف متكاملة تربط أصحاب العمل بالباحثين عن العمل في العالم العربي.  
المشروع مبني بـ **ASP.NET Core MVC 8** مع واجهة عربية كاملة وتصميم احترافي.

---

## 🛠️ التقنيات المستخدمة

| التقنية | الاستخدام |
|---------|-----------|
| ASP.NET Core MVC 8 | إطار العمل الأساسي |
| Entity Framework Core 8 | ORM — Code-First |
| ASP.NET Core Identity | المصادقة وإدارة المستخدمين |
| SQLite / SQL Server | قاعدة البيانات |
| Razor Views | واجهات المستخدم |
| Bootstrap 5 RTL | التصميم العربي |
| Tajawal Font | خط عربي احترافي |
| Swagger / OpenAPI | توثيق REST API |
| Font Awesome 6 | الأيقونات |

---

## 🗂️ هيكل المشروع

```
JobPortal/
├── API/
│   └── JobsApiController.cs       ← REST API كاملة
├── Controllers/
│   ├── AccountController.cs       ← تسجيل / دخول / خروج
│   ├── HomeController.cs          ← الصفحة الرئيسية
│   ├── JobsController.cs          ← تصفح + التقديم
│   ├── EmployerController.cs      ← لوحة صاحب العمل
│   ├── ApplicantController.cs     ← لوحة الباحث
│   └── AdminController.cs         ← لوحة الإدارة
├── Models/
│   ├── ApplicationUser.cs         ← نموذج المستخدم (Identity)
│   ├── Job.cs                     ← نموذج الوظيفة
│   ├── Application.cs             ← نموذج الطلب
│   └── SavedJob.cs / Notification.cs
├── ViewModels/
│   └── ViewModels.cs              ← كل ViewModels
├── Data/
│   ├── ApplicationDbContext.cs    ← سياق قاعدة البيانات
│   └── DbSeeder.cs                ← بيانات أولية
├── Views/                         ← واجهات Razor كاملة
├── wwwroot/                       ← CSS, JS, Uploads
├── Extensions.cs                  ← Enum Display Helper
└── Program.cs                     ← نقطة البداية
```

---

## ⚡ كيفية تشغيل المشروع

### المتطلبات
- **.NET 8 SDK** — https://dotnet.microsoft.com/download
- **Visual Studio 2022** أو **VS Code**

### خطوات التشغيل

```bash
# 1. استنسخ أو افتح المجلد
cd JobPortal

# 2. استعد الـ packages
dotnet restore

# 3. أنشئ قاعدة البيانات (Code-First Migration)
dotnet ef migrations add InitialCreate
dotnet ef database update

# --- أو استخدم EnsureCreated (تلقائي في Program.cs) ---

# 4. شغّل المشروع
dotnet run

# 5. افتح المتصفح على
https://localhost:5001
```

### باستخدام SQL Server
في `appsettings.json` غيّر:
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=JobPortalDb;Trusted_Connection=True;"
```
وفي `Program.cs` استبدل `UseSqlite` بـ `UseSqlServer`.

---

## 👤 حسابات تجريبية جاهزة

| النوع | البريد الإلكتروني | كلمة المرور |
|-------|------------------|-------------|
| 🔴 مدير النظام | admin@jobportal.com | Admin@123456 |
| 🟡 صاحب عمل | employer@tech.com | Employer@123456 |
| 🟢 باحث عن عمل | applicant@gmail.com | Applicant@123456 |

---

## 🔑 المتطلبات المحققة

### ✅ التسجيل وإدارة المستخدمين
- [x] تسجيل مستخدمين جدد مع **Validation** كامل
- [x] **تشفير كلمة المرور** تلقائياً عبر ASP.NET Identity
- [x] **3 أدوار**: Admin / Employer / Applicant
- [x] رسائل خطأ بالعربية مخصصة (`ArabicIdentityErrorDescriber`)
- [x] قفل الحساب بعد 5 محاولات خاطئة

### ✅ المصادقة والتفويض
- [x] **Cookie Authentication** عبر ASP.NET Core Identity
- [x] حماية المسارات بـ `[Authorize(Roles = "...")]`
- [x] إعادة التوجيه التلقائي حسب الدور
- [x] تسجيل الدخول / الخروج

### ✅ REST API (Postman-Ready)
```
GET    /api/jobs                 ← جلب كل الوظائف + بحث + فلترة
GET    /api/jobs/{id}            ← جلب وظيفة محددة
POST   /api/jobs                 ← إضافة وظيفة جديدة [مصادقة مطلوبة]
PUT    /api/jobs/{id}            ← تعديل وظيفة [مصادقة مطلوبة]
DELETE /api/jobs/{id}            ← حذف وظيفة [مصادقة مطلوبة]
GET    /api/jobs/categories      ← جلب الفئات
GET    /api/jobs/stats           ← إحصائيات عامة
```
توثيق Swagger متاح على: `/api-docs`

### ✅ الميزة الإضافية (Bonus Feature)
**نظام تتبع حالة الطلبات + الإشعارات:**
- تتبع 7 حالات: قيد المراجعة → مراجعة → مقابلة → اختبار → مقبول/مرفوض/منسحب
- إشعارات فورية للمتقدم عند تغيير حالة طلبه
- إشعار لصاحب العمل عند استقبال طلب جديد

---

## 🎯 الميزات الكاملة

### للباحث عن عمل
- ✅ تسجيل وإنشاء ملف شخصي كامل
- ✅ تصفح وبحث متقدم في الوظائف (كلمة مفتاحية، موقع، مجال، نوع، خبرة)
- ✅ رفع السيرة الذاتية (PDF/DOC)
- ✅ تقديم الطلبات مع خطاب تغطية
- ✅ تتبع حالة كل طلب
- ✅ حفظ الوظائف للمراجعة لاحقاً
- ✅ لوحة تحكم مع إحصائيات
- ✅ مؤشر اكتمال الملف الشخصي
- ✅ وظائف مقترحة بناءً على المهارات
- ✅ إشعارات لحالة الطلبات
- ✅ سحب الطلب

### لصاحب العمل
- ✅ نشر وظائف مع كل التفاصيل
- ✅ تعديل وإيقاف وحذف الوظائف
- ✅ عرض قائمة المتقدمين مع سيرهم الذاتية
- ✅ تحديث حالة كل طلب مع ملاحظات
- ✅ لوحة تحكم مع إحصائيات
- ✅ ملف شركة كامل مع شعار

### للمدير
- ✅ لوحة إدارة شاملة
- ✅ إدارة المستخدمين (تفعيل/تعطيل)
- ✅ إدارة الوظائف (تمييز/حذف)
- ✅ عرض كل الطلبات
- ✅ إحصائيات شاملة

---

## 📡 اختبار API في Postman

### 1. الحصول على Cookie مصادقة
```
POST https://localhost:5001/Account/Login
Body (form-data):
  Email: employer@tech.com
  Password: Employer@123456
  __RequestVerificationToken: [from page]
```

### 2. جلب الوظائف (عام)
```
GET https://localhost:5001/api/jobs?keyword=مطور&page=1&pageSize=5
```

### 3. إضافة وظيفة
```
POST https://localhost:5001/api/jobs
Headers: Cookie: [auth cookie]
Body (JSON):
{
  "title": "مطور Full Stack",
  "description": "وظيفة رائعة...",
  "requirements": "خبرة 2 سنة",
  "location": "الرياض",
  "jobType": 0,
  "experienceLevel": 1,
  "salaryMin": 10000,
  "salaryMax": 15000,
  "salaryCurrency": "SAR",
  "showSalary": true,
  "skills": "React, ASP.NET",
  "category": "تطوير البرمجيات"
}
```

---

## 🎨 التصميم

- واجهة **عربية RTL** كاملة باستخدام Bootstrap 5 RTL
- خط **Tajawal** العربي الاحترافي
- تصميم **Gradient Dark** عصري
- **Responsive** يعمل على كل الأجهزة
- مؤثرات بصرية ناعمة (hover, transitions)

---

## 👨‍💻 صُنع بـ ASP.NET Core 8 + C# + Entity Framework Core

