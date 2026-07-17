using JobPortal.Data;
using JobPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API
{
    [ApiController]
    [Route("api/jobs")]
    [Produces("application/json")]
    public class JobsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobsApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// GET /api/jobs — جلب كل الوظائف مع دعم البحث والتصفية
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? keyword,
            [FromQuery] string? location,
            [FromQuery] string? category,
            [FromQuery] JobType? jobType,
            [FromQuery] ExperienceLevel? experienceLevel,
            [FromQuery] bool? isRemote,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Jobs
                .Include(j => j.Employer)
                .Where(j => j.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(j =>
                    j.Title.ToLower().Contains(kw) ||
                    j.Description.ToLower().Contains(kw) ||
                    j.Skills!.ToLower().Contains(kw));
            }

            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(j => j.Location.Contains(location));

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(j => j.Category == category);

            if (jobType.HasValue)
                query = query.Where(j => j.JobType == jobType.Value);

            if (experienceLevel.HasValue)
                query = query.Where(j => j.ExperienceLevel == experienceLevel.Value);

            if (isRemote.HasValue)
                query = query.Where(j => j.IsRemote == isRemote.Value);

            var total = await query.CountAsync();
            var jobs = await query
                .OrderByDescending(j => j.IsFeatured)
                .ThenByDescending(j => j.PostedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(j => new
                {
                    j.Id,
                    j.Title,
                    j.Location,
                    j.IsRemote,
                    j.JobType,
                    j.ExperienceLevel,
                    j.SalaryMin,
                    j.SalaryMax,
                    j.SalaryCurrency,
                    j.ShowSalary,
                    j.Category,
                    j.Skills,
                    j.PostedAt,
                    j.IsFeatured,
                    j.ViewCount,
                    ApplicationsCount = j.Applications.Count,
                    Employer = new
                    {
                        j.Employer.FullName,
                        j.Employer.CompanyName,
                        j.Employer.CompanyLogo,
                        j.Employer.Industry,
                        j.Employer.CompanyLocation
                    }
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = jobs,
                pagination = new
                {
                    page,
                    pageSize,
                    total,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize)
                }
            });
        }

        /// <summary>
        /// GET /api/jobs/{id} — جلب وظيفة محددة
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var job = await _context.Jobs
                .Include(j => j.Employer)
                .Include(j => j.Applications)
                .FirstOrDefaultAsync(j => j.Id == id && j.IsActive);

            if (job == null)
                return NotFound(new { success = false, message = "الوظيفة غير موجودة" });

            return Ok(new
            {
                success = true,
                data = new
                {
                    job.Id,
                    job.Title,
                    job.Description,
                    job.Requirements,
                    job.Responsibilities,
                    job.Location,
                    job.IsRemote,
                    job.JobType,
                    job.ExperienceLevel,
                    job.SalaryMin,
                    job.SalaryMax,
                    job.SalaryCurrency,
                    job.ShowSalary,
                    job.Category,
                    job.Skills,
                    job.PostedAt,
                    job.ExpiresAt,
                    job.IsFeatured,
                    job.ViewCount,
                    ApplicationsCount = job.Applications.Count,
                    Employer = new
                    {
                        job.Employer.FullName,
                        job.Employer.CompanyName,
                        job.Employer.CompanyDescription,
                        job.Employer.CompanyWebsite,
                        job.Employer.CompanyLogo,
                        job.Employer.Industry,
                        job.Employer.CompanyLocation
                    }
                }
            });
        }

        /// <summary>
        /// POST /api/jobs — إضافة وظيفة جديدة (صاحب عمل فقط)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Employer,Admin")]
        public async Task<IActionResult> Create([FromBody] JobCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            var userId = _userManager.GetUserId(User)!;

            var job = new Job
            {
                Title = dto.Title,
                Description = dto.Description,
                Requirements = dto.Requirements,
                Responsibilities = dto.Responsibilities,
                Location = dto.Location,
                IsRemote = dto.IsRemote,
                JobType = dto.JobType,
                ExperienceLevel = dto.ExperienceLevel,
                SalaryMin = dto.SalaryMin,
                SalaryMax = dto.SalaryMax,
                SalaryCurrency = dto.SalaryCurrency ?? "USD",
                ShowSalary = dto.ShowSalary,
                Skills = dto.Skills,
                Category = dto.Category,
                ExpiresAt = dto.ExpiresAt,
                EmployerId = userId
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = job.Id },
                new { success = true, message = "تم نشر الوظيفة بنجاح", data = new { job.Id } });
        }

        /// <summary>
        /// PUT /api/jobs/{id} — تحديث وظيفة
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Employer,Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] JobCreateDto dto)
        {
            var userId = _userManager.GetUserId(User)!;
            var isAdmin = User.IsInRole("Admin");

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id &&
                (isAdmin || j.EmployerId == userId));

            if (job == null)
                return NotFound(new { success = false, message = "الوظيفة غير موجودة أو ليس لديك صلاحية تعديلها" });

            job.Title = dto.Title;
            job.Description = dto.Description;
            job.Requirements = dto.Requirements;
            job.Responsibilities = dto.Responsibilities;
            job.Location = dto.Location;
            job.IsRemote = dto.IsRemote;
            job.JobType = dto.JobType;
            job.ExperienceLevel = dto.ExperienceLevel;
            job.SalaryMin = dto.SalaryMin;
            job.SalaryMax = dto.SalaryMax;
            job.ShowSalary = dto.ShowSalary;
            job.Skills = dto.Skills;
            job.Category = dto.Category;
            job.ExpiresAt = dto.ExpiresAt;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "تم تحديث الوظيفة بنجاح" });
        }

        /// <summary>
        /// DELETE /api/jobs/{id} — حذف وظيفة
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Employer,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var isAdmin = User.IsInRole("Admin");

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id &&
                (isAdmin || j.EmployerId == userId));

            if (job == null)
                return NotFound(new { success = false, message = "الوظيفة غير موجودة" });

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "تم حذف الوظيفة بنجاح" });
        }

        /// <summary>
        /// GET /api/jobs/categories — جلب كل الفئات
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Jobs
                .Where(j => j.IsActive)
                .GroupBy(j => j.Category)
                .Select(g => new { name = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .ToListAsync();

            return Ok(new { success = true, data = categories });
        }

        /// <summary>
        /// GET /api/jobs/stats — إحصائيات
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            return Ok(new
            {
                success = true,
                data = new
                {
                    totalJobs = await _context.Jobs.CountAsync(j => j.IsActive),
                    totalApplications = await _context.Applications.CountAsync(),
                    jobsByType = await _context.Jobs
                        .Where(j => j.IsActive)
                        .GroupBy(j => j.JobType)
                        .Select(g => new { type = g.Key.ToString(), count = g.Count() })
                        .ToListAsync(),
                    topCategories = await _context.Jobs
                        .Where(j => j.IsActive)
                        .GroupBy(j => j.Category)
                        .Select(g => new { category = g.Key, count = g.Count() })
                        .OrderByDescending(x => x.count)
                        .Take(5)
                        .ToListAsync()
                }
            });
        }
    }

    // DTO for job creation/update
    public class JobCreateDto
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Title { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required]
        public string Description { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required]
        public string Requirements { get; set; } = string.Empty;
        public string? Responsibilities { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        public string Location { get; set; } = string.Empty;
        public bool IsRemote { get; set; }
        public JobType JobType { get; set; }
        public ExperienceLevel ExperienceLevel { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string? SalaryCurrency { get; set; }
        public bool ShowSalary { get; set; } = true;
        public string? Skills { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        public string Category { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }
}
