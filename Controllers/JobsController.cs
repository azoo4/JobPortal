using JobPortal.Data;
using JobPortal.Models;
using JobPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Controllers
{
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public JobsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // GET: /Jobs — Browse all jobs with search
        public async Task<IActionResult> Index(JobSearchViewModel model)
        {
            var query = _context.Jobs
                .Include(j => j.Employer)
                .Where(j => j.IsActive);

            // Keyword search
            if (!string.IsNullOrWhiteSpace(model.Keyword))
            {
                var kw = model.Keyword.Trim().ToLower();
                query = query.Where(j =>
                    j.Title.ToLower().Contains(kw) ||
                    j.Description.ToLower().Contains(kw) ||
                    j.Skills!.ToLower().Contains(kw) ||
                    j.Employer.CompanyName!.ToLower().Contains(kw));
            }

            if (!string.IsNullOrWhiteSpace(model.Location))
                query = query.Where(j => j.Location.Contains(model.Location));

            if (!string.IsNullOrWhiteSpace(model.Category))
                query = query.Where(j => j.Category == model.Category);

            if (model.JobType.HasValue)
                query = query.Where(j => j.JobType == model.JobType.Value);

            if (model.ExperienceLevel.HasValue)
                query = query.Where(j => j.ExperienceLevel == model.ExperienceLevel.Value);

            if (model.IsRemote.HasValue && model.IsRemote.Value)
                query = query.Where(j => j.IsRemote);

            if (model.MinSalary.HasValue)
                query = query.Where(j => j.SalaryMin >= model.MinSalary.Value);

            // Sort
            query = model.SortBy switch
            {
                "salary" => query.OrderByDescending(j => j.SalaryMax),
                "popular" => query.OrderByDescending(j => j.ViewCount),
                _ => query.OrderByDescending(j => j.IsFeatured).ThenByDescending(j => j.PostedAt)
            };

            model.TotalCount = await query.CountAsync();
            model.Jobs = await query
                .Skip((model.Page - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToListAsync();

            model.Categories = await _context.Jobs
                .Where(j => j.IsActive)
                .Select(j => j.Category)
                .Distinct()
                .ToListAsync();

            model.Locations = await _context.Jobs
                .Where(j => j.IsActive)
                .Select(j => j.Location)
                .Distinct()
                .ToListAsync();

            return View(model);
        }

        // GET: /Jobs/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var job = await _context.Jobs
                .Include(j => j.Employer)
                .Include(j => j.Applications)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null) return NotFound();

            // Increment view count
            job.ViewCount++;
            await _context.SaveChangesAsync();

            // Check if current user already applied
            bool alreadyApplied = false;
            bool isSaved = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                alreadyApplied = await _context.Applications
                    .AnyAsync(a => a.ApplicantId == userId && a.JobId == id);
                isSaved = await _context.SavedJobs
                    .AnyAsync(s => s.UserId == userId && s.JobId == id);
            }

            ViewBag.AlreadyApplied = alreadyApplied;
            ViewBag.IsSaved = isSaved;

            // Related jobs
            var related = await _context.Jobs
                .Include(j => j.Employer)
                .Where(j => j.IsActive && j.Id != id && j.Category == job.Category)
                .Take(4)
                .ToListAsync();
            ViewBag.RelatedJobs = related;

            return View(job);
        }

        // GET: Apply
        [Authorize(Roles = "Applicant")]
        public async Task<IActionResult> Apply(int id)
        {
            var job = await _context.Jobs.Include(j => j.Employer).FirstOrDefaultAsync(j => j.Id == id);
            if (job == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var alreadyApplied = await _context.Applications.AnyAsync(a => a.ApplicantId == userId && a.JobId == id);
            if (alreadyApplied)
            {
                TempData["Error"] = "لقد قدمت على هذه الوظيفة مسبقاً";
                return RedirectToAction("Details", new { id });
            }

            var user = await _userManager.GetUserAsync(User);
            var model = new ApplyViewModel
            {
                JobId = id,
                Job = job,
                ExistingCvPath = user?.CvPath
            };

            return View(model);
        }

        // POST: Apply
        [Authorize(Roles = "Applicant")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(ApplyViewModel model)
        {
            var userId = _userManager.GetUserId(User)!;
            var user = await _userManager.GetUserAsync(User);
            var job = await _context.Jobs.Include(j => j.Employer).FirstOrDefaultAsync(j => j.Id == model.JobId);

            if (job == null) return NotFound();

            string? cvPath = user?.CvPath;

            if (!model.UseExistingCv && model.NewCv != null)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "cvs");
                Directory.CreateDirectory(uploadsDir);
                var fileName = $"{userId}_{DateTime.Now.Ticks}{Path.GetExtension(model.NewCv.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await model.NewCv.CopyToAsync(stream);
                cvPath = $"/uploads/cvs/{fileName}";
            }

            if (string.IsNullOrEmpty(cvPath))
            {
                TempData["Error"] = "يجب رفع سيرة ذاتية للتقديم";
                model.Job = job;
                return View(model);
            }

            var application = new Application
            {
                ApplicantId = userId,
                JobId = model.JobId,
                CoverLetter = model.CoverLetter,
                CvPath = cvPath,
                Status = ApplicationStatus.Pending
            };

            _context.Applications.Add(application);

            // Notify employer
            var notification = new Notification
            {
                UserId = job.EmployerId,
                Title = "طلب توظيف جديد",
                Message = $"{user?.FullName} قدّم على وظيفة {job.Title}",
                Link = $"/Employer/Applications/{model.JobId}",
                Type = NotificationType.ApplicationReceived
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تقديم طلبك بنجاح! سيتواصل معك صاحب العمل قريباً.";
            return RedirectToAction("MyApplications", "Applicant");
        }

        // POST: Save/Unsave Job
        [Authorize(Roles = "Applicant")]
        [HttpPost]
        public async Task<IActionResult> ToggleSave(int jobId)
        {
            var userId = _userManager.GetUserId(User)!;
            var saved = await _context.SavedJobs.FirstOrDefaultAsync(s => s.UserId == userId && s.JobId == jobId);

            if (saved != null)
            {
                _context.SavedJobs.Remove(saved);
                await _context.SaveChangesAsync();
                return Json(new { saved = false, message = "تم إزالة الوظيفة من المحفوظات" });
            }
            else
            {
                _context.SavedJobs.Add(new SavedJob { UserId = userId, JobId = jobId });
                await _context.SaveChangesAsync();
                return Json(new { saved = true, message = "تم حفظ الوظيفة" });
            }
        }
    }
}
