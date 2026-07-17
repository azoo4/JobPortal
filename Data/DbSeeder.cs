using JobPortal.Models;
using Microsoft.AspNetCore.Identity;

namespace JobPortal.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Create Roles
            string[] roles = { "Admin", "Employer", "Applicant" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Create Admin User
            if (await userManager.FindByEmailAsync("admin@jobportal.com") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@jobportal.com",
                    Email = "admin@jobportal.com",
                    FullName = "مدير النظام",
                    EmailConfirmed = true,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(admin, "Admin@123456");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Create Sample Employer
            ApplicationUser? employer = await userManager.FindByEmailAsync("employer@tech.com");
            if (employer == null)
            {
                employer = new ApplicationUser
                {
                    UserName = "employer@tech.com",
                    Email = "employer@tech.com",
                    FullName = "أحمد الشمري",
                    CompanyName = "تك سوليوشنز العربية",
                    CompanyDescription = "شركة رائدة في مجال تطوير البرمجيات والحلول التقنية المتكاملة",
                    Industry = "تكنولوجيا المعلومات",
                    CompanyLocation = "الرياض، المملكة العربية السعودية",
                    CompanyWebsite = "https://techsolutions.sa",
                    EmailConfirmed = true,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(employer, "Employer@123456");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(employer, "Employer");
            }

            // Create Sample Applicant
            ApplicationUser? applicant = await userManager.FindByEmailAsync("applicant@gmail.com");
            if (applicant == null)
            {
                applicant = new ApplicationUser
                {
                    UserName = "applicant@gmail.com",
                    Email = "applicant@gmail.com",
                    FullName = "سارة العمري",
                    Bio = "مطورة برمجيات ذات خبرة 3 سنوات في تطوير تطبيقات الويب",
                    Skills = "C#, ASP.NET, JavaScript, React, SQL",
                    CurrentPosition = "مطورة برمجيات",
                    YearsOfExperience = 3,
                    EmailConfirmed = true,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(applicant, "Applicant@123456");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(applicant, "Applicant");
            }

            // Seed Jobs
            if (!context.Jobs.Any())
            {
                var jobs = new List<Job>
                {
                    new Job
                    {
                        Title = "مطور .NET متقدم",
                        Description = "نبحث عن مطور .NET ذو خبرة عالية للانضمام إلى فريقنا الهندسي المتنامي. ستعمل على تطوير تطبيقات enterprise-level باستخدام أحدث تقنيات Microsoft.",
                        Requirements = "خبرة 3+ سنوات في ASP.NET Core\nإتقان Entity Framework Core\nمعرفة بـ SQL Server\nخبرة في REST APIs",
                        Responsibilities = "تطوير وصيانة التطبيقات\nمراجعة الكود\nالتعاون مع الفريق",
                        Location = "الرياض",
                        JobType = JobType.FullTime,
                        ExperienceLevel = ExperienceLevel.Senior,
                        SalaryMin = 12000,
                        SalaryMax = 18000,
                        SalaryCurrency = "SAR",
                        ShowSalary = true,
                        Skills = "C#, ASP.NET Core, EF Core, SQL Server",
                        Category = "تطوير البرمجيات",
                        IsFeatured = true,
                        EmployerId = employer!.Id
                    },
                    new Job
                    {
                        Title = "مصمم UI/UX",
                        Description = "نحتاج إلى مصمم إبداعي لتحسين تجربة مستخدمي منتجاتنا الرقمية.",
                        Requirements = "خبرة في Figma وAdobe XD\nفهم عميق لـ UX\nمحفظة أعمال متميزة",
                        Location = "جدة",
                        JobType = JobType.FullTime,
                        ExperienceLevel = ExperienceLevel.Mid,
                        SalaryMin = 8000,
                        SalaryMax = 14000,
                        SalaryCurrency = "SAR",
                        ShowSalary = true,
                        Skills = "Figma, Adobe XD, Photoshop, UX Research",
                        Category = "تصميم",
                        EmployerId = employer!.Id
                    },
                    new Job
                    {
                        Title = "محلل بيانات",
                        Description = "فرصة رائعة لمحلل بيانات شغوف للعمل مع فرق متعددة لاستخراج رؤى قيّمة.",
                        Requirements = "خبرة في Python أو R\nإتقان SQL\nمعرفة بـ Power BI",
                        Location = "دبي",
                        IsRemote = true,
                        JobType = JobType.FullTime,
                        ExperienceLevel = ExperienceLevel.Mid,
                        SalaryMin = 10000,
                        SalaryMax = 16000,
                        SalaryCurrency = "AED",
                        ShowSalary = true,
                        Skills = "Python, SQL, Power BI, Excel",
                        Category = "تحليل البيانات",
                        IsFeatured = true,
                        EmployerId = employer!.Id
                    },
                    new Job
                    {
                        Title = "مدير مشاريع تقنية",
                        Description = "قيادة مشاريع تقنية من الفكرة حتى التسليم في بيئة سريعة ومتطورة.",
                        Requirements = "شهادة PMP أو ما يعادلها\nخبرة 5+ سنوات في إدارة المشاريع التقنية\nإتقان Agile/Scrum",
                        Location = "عمّان",
                        JobType = JobType.FullTime,
                        ExperienceLevel = ExperienceLevel.Lead,
                        SalaryMin = 15000,
                        SalaryMax = 25000,
                        SalaryCurrency = "JOD",
                        ShowSalary = false,
                        Skills = "PMP, Agile, Scrum, JIRA",
                        Category = "إدارة المشاريع",
                        EmployerId = employer!.Id
                    },
                    new Job
                    {
                        Title = "متدرب تطوير ويب",
                        Description = "فرصة تدريبية ممتازة لخريجي علوم الحاسوب لاكتساب خبرة عملية.",
                        Requirements = "طالب أو خريج حديث\nمعرفة أساسية بـ HTML, CSS, JavaScript\nشغف بالتعلم",
                        Location = "القاهرة",
                        JobType = JobType.Internship,
                        ExperienceLevel = ExperienceLevel.Entry,
                        ShowSalary = false,
                        Skills = "HTML, CSS, JavaScript",
                        Category = "تطوير الويب",
                        EmployerId = employer!.Id
                    }
                };

                context.Jobs.AddRange(jobs);
                await context.SaveChangesAsync();
            }
        }
    }
}
