using JobPortal.Data;
using JobPortal.Models;
using JobPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Controllers
{
    [Authorize(Roles = "Applicant")]
    public class ApplicantController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ApplicantController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
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

            var applications = await _context.Applications
                .Include(a => a.Job).ThenInclude(j => j.Employer)
                .Where(a => a.ApplicantId == userId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            var savedJobs = await _context.SavedJobs
                .Include(s => s.Job).ThenInclude(j => j.Employer)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SavedAt)
                .Take(5)
                .ToListAsync();

            // Recommended jobs based on skills
            var skills = user?.Skills?.Split(',').Select(s => s.Trim().ToLower()).ToList() ?? new();
            var recommended = await _context.Jobs
                .Include(j => j.Employer)
                .Where(j => j.IsActive)
                .ToListAsync();

            if (skills.Any())
                recommended = recommended
                    .Where(j => skills.Any(s => j.Skills?.ToLower().Contains(s) == true ||
                                                j.Title.ToLower().Contains(s)))
                    .Take(6)
                    .ToList();
            else
                recommended = recommended.OrderByDescending(j => j.PostedAt).Take(6).ToList();

            var model = new ApplicantDashboardViewModel
            {
                User = user!,
                RecentApplications = applications.Take(5).ToList(),
                SavedJobs = savedJobs,
                TotalApplications = applications.Count,
                PendingApplications = applications.Count(a => a.Status == ApplicationStatus.Pending),
                AcceptedApplications = applications.Count(a => a.Status == ApplicationStatus.Accepted),
                RejectedApplications = applications.Count(a => a.Status == ApplicationStatus.Rejected),
                RecommendedJobs = recommended
            };

            return View(model);
        }

        // My Applications
        public async Task<IActionResult> MyApplications(string? status = null)
        {
            var userId = _userManager.GetUserId(User)!;
            var query = _context.Applications
                .Include(a => a.Job).ThenInclude(j => j.Employer)
                .Where(a => a.ApplicantId == userId);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ApplicationStatus>(status, out var statusEnum))
                query = query.Where(a => a.Status == statusEnum);

            var applications = await query.OrderByDescending(a => a.AppliedAt).ToListAsync();
            ViewBag.FilterStatus = status;
            return View(applications);
        }

        // Withdraw application
        [HttpPost]
        public async Task<IActionResult> WithdrawApplication(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var application = await _context.Applications.FirstOrDefaultAsync(a => a.Id == id && a.ApplicantId == userId);
            if (application == null) return NotFound();

            application.Status = ApplicationStatus.Withdrawn;
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم سحب الطلب بنجاح";
            return RedirectToAction("MyApplications");
        }

        // Saved Jobs
        public async Task<IActionResult> SavedJobs()
        {
            var userId = _userManager.GetUserId(User)!;
            var saved = await _context.SavedJobs
                .Include(s => s.Job).ThenInclude(j => j.Employer)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SavedAt)
                .ToListAsync();
            return View(saved);
        }

        // GET: Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            var model = new ApplicantProfileViewModel
            {
                FullName = user!.FullName,
                Bio = user.Bio,
                Skills = user.Skills,
                CurrentPosition = user.CurrentPosition,
                YearsOfExperience = user.YearsOfExperience,
                LinkedInUrl = user.LinkedInUrl,
                ExistingCvPath = user.CvPath,
                PhoneNumber = user.PhoneNumber
            };
            return View(model);
        }

        // POST: Update Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ApplicantProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            user!.FullName = model.FullName;
            user.Bio = model.Bio;
            user.Skills = model.Skills;
            user.CurrentPosition = model.CurrentPosition;
            user.YearsOfExperience = model.YearsOfExperience;
            user.LinkedInUrl = model.LinkedInUrl;
            user.PhoneNumber = model.PhoneNumber;

            if (model.CvFile != null)
            {
                var dir = Path.Combine(_env.WebRootPath, "uploads", "cvs");
                Directory.CreateDirectory(dir);
                var fileName = $"{user.Id}_{DateTime.Now.Ticks}{Path.GetExtension(model.CvFile.FileName)}";
                using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
                await model.CvFile.CopyToAsync(stream);
                user.CvPath = $"/uploads/cvs/{fileName}";
            }

            if (model.ProfilePicture != null)
            {
                var dir = Path.Combine(_env.WebRootPath, "uploads", "profiles");
                Directory.CreateDirectory(dir);
                var fileName = $"{user.Id}{Path.GetExtension(model.ProfilePicture.FileName)}";
                using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
                await model.ProfilePicture.CopyToAsync(stream);
                user.ProfilePicture = $"/uploads/profiles/{fileName}";
            }

            await _userManager.UpdateAsync(user);
            TempData["Success"] = "تم تحديث ملفك الشخصي بنجاح";
            return RedirectToAction("Dashboard");
        }

        // Notifications
        public async Task<IActionResult> Notifications()
        {
            var userId = _userManager.GetUserId(User)!;
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            // Mark all as read
            foreach (var n in notifications.Where(n => !n.IsRead))
                n.IsRead = true;
            await _context.SaveChangesAsync();

            return View(notifications);
        }
    }
}
