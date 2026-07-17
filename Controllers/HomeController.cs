using JobPortal.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPortal.Models.ViewModels;

namespace JobPortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredJobs = await _context.Jobs
                .Include(j => j.Employer)
                .Where(j => j.IsActive && j.IsFeatured)
                .OrderByDescending(j => j.PostedAt)
                .Take(6)
                .ToListAsync();

            var recentJobs = await _context.Jobs
                .Include(j => j.Employer)
                .Where(j => j.IsActive)
                .OrderByDescending(j => j.PostedAt)
                .Take(8)
                .ToListAsync();

            var categories = await _context.Jobs
    .Where(j => j.IsActive)
    .GroupBy(j => j.Category)
    .Select(g => new JobPortal.Models.ViewModels.CategoryItem
    {
        Category = g.Key,
        Count = g.Count()
    })
    .OrderByDescending(x => x.Count)
    .Take(8)
    .ToListAsync();

            ViewBag.FeaturedJobs = featuredJobs;
            ViewBag.RecentJobs = recentJobs;
            ViewBag.Categories = categories;
            ViewBag.TotalJobs = await _context.Jobs.CountAsync(j => j.IsActive);
            ViewBag.TotalEmployers = (await _context.Users.ToListAsync()).Count; // simplified
            ViewBag.TotalApplications = await _context.Applications.CountAsync();

            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }
}
