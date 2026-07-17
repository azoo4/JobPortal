using JobPortal.Data;
using JobPortal.Models;
using JobPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var allUsers = await _context.Users.ToListAsync();
            var employers = await _userManager.GetUsersInRoleAsync("Employer");
            var applicants = await _userManager.GetUsersInRoleAsync("Applicant");
            var thisMonth = DateTime.Now.AddMonths(-1);

            var model = new AdminDashboardViewModel
            {
                TotalUsers = allUsers.Count,
                TotalEmployers = employers.Count,
                TotalApplicants = applicants.Count,
                TotalJobs = await _context.Jobs.CountAsync(),
                ActiveJobs = await _context.Jobs.CountAsync(j => j.IsActive),
                TotalApplications = await _context.Applications.CountAsync(),
                NewUsersThisMonth = allUsers.Count(u => u.CreatedAt >= thisMonth),
                NewJobsThisMonth = await _context.Jobs.CountAsync(j => j.PostedAt >= thisMonth),
                LatestUsers = allUsers.OrderByDescending(u => u.CreatedAt).Take(5).ToList(),
                LatestJobs = await _context.Jobs.Include(j => j.Employer).OrderByDescending(j => j.PostedAt).Take(5).ToListAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> Users(string? role = null)
        {
            var users = _userManager.Users.ToList();
            if (!string.IsNullOrEmpty(role))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                users = usersInRole.ToList();
            }
            ViewBag.FilterRole = role;
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = user.IsActive ? "تم تفعيل المستخدم" : "تم تعطيل المستخدم";
            return RedirectToAction("Users");
        }

        public async Task<IActionResult> Jobs()
        {
            var jobs = await _context.Jobs
                .Include(j => j.Employer)
                .Include(j => j.Applications)
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();
            return View(jobs);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFeatured(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            job.IsFeatured = !job.IsFeatured;
            await _context.SaveChangesAsync();
            TempData["Success"] = job.IsFeatured ? "تم تمييز الوظيفة" : "تم إلغاء تمييز الوظيفة";
            return RedirectToAction("Jobs");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();
            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف الوظيفة";
            return RedirectToAction("Jobs");
        }

        public async Task<IActionResult> Applications()
        {
            var applications = await _context.Applications
                .Include(a => a.Applicant)
                .Include(a => a.Job).ThenInclude(j => j.Employer)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
            return View(applications);
        }
    }
}
