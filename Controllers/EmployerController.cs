using JobPortal.Data;
using JobPortal.Models;
using JobPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Controllers
{
    [Authorize(Roles = "Employer")]
    public class EmployerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public EmployerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User)!;
            var user = await _userManager.FindByIdAsync(userId);

            var jobs = await _context.Jobs
                .Where(j => j.EmployerId == userId)
                .Include(j => j.Applications)
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();

            var allApplications = await _context.Applications
                .Include(a => a.Job)
                .Include(a => a.Applicant)
                .Where(a => a.Job.EmployerId == userId)
                .OrderByDescending(a => a.AppliedAt)
                .Take(10)
                .ToListAsync();

            var model = new EmployerDashboardViewModel
            {
                User = user!,
                ActiveJobs = jobs.Where(j => j.IsActive).ToList(),
                RecentApplications = allApplications,
                TotalJobs = jobs.Count,
                TotalApplications = jobs.Sum(j => j.Applications.Count),
                NewApplications = jobs.SelectMany(j => j.Applications).Count(a => !a.IsViewedByEmployer),
                ApplicationsByStatus = Enum.GetValues<ApplicationStatus>()
                    .ToDictionary(s => s.ToString(),
                        s => jobs.SelectMany(j => j.Applications).Count(a => a.Status == s))
            };

            return View(model);
        }

        // My Jobs list
        public async Task<IActionResult> MyJobs()
        {
            var userId = _userManager.GetUserId(User)!;
            var jobs = await _context.Jobs
                .Where(j => j.EmployerId == userId)
                .Include(j => j.Applications)
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();
            return View(jobs);
        }

        // GET: Post new job
        [HttpGet]
        public IActionResult PostJob() => View(new Job());

        // POST: Post new job
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostJob(Job model)
        {
            ModelState.Remove("Employer");
            ModelState.Remove("EmployerId");
            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User)!;
            model.EmployerId = userId;
            model.PostedAt = DateTime.Now;

            _context.Jobs.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم نشر الوظيفة بنجاح!";
            return RedirectToAction("MyJobs");
        }

        // GET: Edit job
        [HttpGet]
        public async Task<IActionResult> EditJob(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.EmployerId == userId);
            if (job == null) return NotFound();
            return View(job);
        }

        // POST: Edit job
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJob(Job model)
        {
            ModelState.Remove("Employer");
            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User)!;
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == model.Id && j.EmployerId == userId);
            if (job == null) return NotFound();

            job.Title = model.Title;
            job.Description = model.Description;
            job.Requirements = model.Requirements;
            job.Responsibilities = model.Responsibilities;
            job.Location = model.Location;
            job.IsRemote = model.IsRemote;
            job.JobType = model.JobType;
            job.ExperienceLevel = model.ExperienceLevel;
            job.SalaryMin = model.SalaryMin;
            job.SalaryMax = model.SalaryMax;
            job.SalaryCurrency = model.SalaryCurrency;
            job.ShowSalary = model.ShowSalary;
            job.Skills = model.Skills;
            job.Category = model.Category;
            job.ExpiresAt = model.ExpiresAt;

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم تحديث الوظيفة بنجاح!";
            return RedirectToAction("MyJobs");
        }

        // POST: Toggle job active/inactive
        [HttpPost]
        public async Task<IActionResult> ToggleJobStatus(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.EmployerId == userId);
            if (job == null) return NotFound();

            job.IsActive = !job.IsActive;
            await _context.SaveChangesAsync();
            TempData["Success"] = job.IsActive ? "تم تفعيل الوظيفة" : "تم إيقاف الوظيفة";
            return RedirectToAction("MyJobs");
        }

        // POST: Delete job
        [HttpPost]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.EmployerId == userId);
            if (job == null) return NotFound();

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف الوظيفة";
            return RedirectToAction("MyJobs");
        }

        // GET: Applications for a job
        public async Task<IActionResult> Applications(int jobId)
        {
            var userId = _userManager.GetUserId(User)!;
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId && j.EmployerId == userId);
            if (job == null) return NotFound();

            var applications = await _context.Applications
                .Include(a => a.Applicant)
                .Where(a => a.JobId == jobId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            // Mark all as viewed
            foreach (var app in applications.Where(a => !a.IsViewedByEmployer))
                app.IsViewedByEmployer = true;
            await _context.SaveChangesAsync();

            ViewBag.Job = job;
            return View(applications);
        }

        // GET: Application details
        public async Task<IActionResult> ApplicationDetail(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var application = await _context.Applications
                .Include(a => a.Applicant)
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == id && a.Job.EmployerId == userId);

            if (application == null) return NotFound();
            return View(application);
        }

        // POST: Update application status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplicationStatus(UpdateApplicationStatusViewModel model)
        {
            var userId = _userManager.GetUserId(User)!;
            var application = await _context.Applications
                .Include(a => a.Job)
                .Include(a => a.Applicant)
                .FirstOrDefaultAsync(a => a.Id == model.ApplicationId && a.Job.EmployerId == userId);

            if (application == null) return NotFound();

            application.Status = model.Status;
            application.EmployerNotes = model.Notes;
            application.LastUpdated = DateTime.Now;

            // Notify applicant
            var statusArabic = model.Status switch
            {
                ApplicationStatus.Reviewed => "تمت مراجعة طلبك",
                ApplicationStatus.Interview => "تمت دعوتك لمقابلة",
                ApplicationStatus.Accepted => "تهانينا! تم قبول طلبك",
                ApplicationStatus.Rejected => "نأسف، تم رفض طلبك",
                _ => "تم تحديث حالة طلبك"
            };

            _context.Notifications.Add(new Notification
            {
                UserId = application.ApplicantId,
                Title = "تحديث طلب التوظيف",
                Message = $"{statusArabic} لوظيفة {application.Job.Title}",
                Link = "/Applicant/MyApplications",
                Type = NotificationType.ApplicationStatusChanged
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم تحديث حالة الطلب";
            return RedirectToAction("Applications", new { jobId = application.JobId });
        }

        // GET: Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            var model = new EmployerProfileViewModel
            {
                FullName = user!.FullName,
                CompanyName = user.CompanyName,
                CompanyDescription = user.CompanyDescription,
                CompanyWebsite = user.CompanyWebsite,
                Industry = user.Industry,
                CompanyLocation = user.CompanyLocation,
                PhoneNumber = user.PhoneNumber
            };
            return View(model);
        }

        // POST: Update profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(EmployerProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            user!.FullName = model.FullName;
            user.CompanyName = model.CompanyName;
            user.CompanyDescription = model.CompanyDescription;
            user.CompanyWebsite = model.CompanyWebsite;
            user.Industry = model.Industry;
            user.CompanyLocation = model.CompanyLocation;
            user.PhoneNumber = model.PhoneNumber;

            if (model.LogoFile != null)
            {
                var dir = Path.Combine(_env.WebRootPath, "uploads", "logos");
                Directory.CreateDirectory(dir);
                var fileName = $"{user.Id}{Path.GetExtension(model.LogoFile.FileName)}";
                using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
                await model.LogoFile.CopyToAsync(stream);
                user.CompanyLogo = $"/uploads/logos/{fileName}";
            }

            await _userManager.UpdateAsync(user);
            TempData["Success"] = "تم تحديث الملف الشخصي بنجاح";
            return RedirectToAction("Dashboard");
        }
    }
}
