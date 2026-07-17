using System.ComponentModel.DataAnnotations;
using JobPortal.Models;

namespace JobPortal.ViewModels
{
    // ===== Auth ViewModels =====
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "بريد إلكتروني غير صحيح")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(100, ErrorMessage = "كلمة المرور يجب أن تكون {2} حرف على الأقل", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور")]
        [Compare("Password", ErrorMessage = "كلمتا المرور غير متطابقتين")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "نوع الحساب مطلوب")]
        [Display(Name = "نوع الحساب")]
        public string AccountType { get; set; } = "Applicant"; // Employer or Applicant

        // Employer only
        [Display(Name = "اسم الشركة")]
        public string? CompanyName { get; set; }

        [Display(Name = "القطاع")]
        public string? Industry { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "بريد إلكتروني غير صحيح")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "تذكرني")]
        public bool RememberMe { get; set; }
    }

    // ===== Profile ViewModels =====
    public class ApplicantProfileViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? Skills { get; set; }
        public string? CurrentPosition { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? LinkedInUrl { get; set; }
        public IFormFile? CvFile { get; set; }
        public IFormFile? ProfilePicture { get; set; }
        public string? ExistingCvPath { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class EmployerProfileViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? CompanyDescription { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? Industry { get; set; }
        public string? CompanyLocation { get; set; }
        public IFormFile? LogoFile { get; set; }
        public string? PhoneNumber { get; set; }
    }

    // ===== Job ViewModels =====
    public class JobSearchViewModel
    {
        public string? Keyword { get; set; }
        public string? Location { get; set; }
        public string? Category { get; set; }
        public JobType? JobType { get; set; }
        public ExperienceLevel? ExperienceLevel { get; set; }
        public bool? IsRemote { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public string SortBy { get; set; } = "newest";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public List<Job> Jobs { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public List<string> Locations { get; set; } = new();
    }

    public class ApplyViewModel
    {
        public int JobId { get; set; }
        public Job? Job { get; set; }

        [StringLength(1000, ErrorMessage = "خطاب التقديم لا يمكن أن يتجاوز 1000 حرف")]
        [Display(Name = "خطاب التقديم")]
        public string? CoverLetter { get; set; }

        public bool UseExistingCv { get; set; } = true;

        [Display(Name = "رفع سيرة ذاتية جديدة")]
        public IFormFile? NewCv { get; set; }

        public string? ExistingCvPath { get; set; }
    }

    // ===== Dashboard ViewModels =====
    public class ApplicantDashboardViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public List<Application> RecentApplications { get; set; } = new();
        public List<SavedJob> SavedJobs { get; set; } = new();
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int AcceptedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public List<Job> RecommendedJobs { get; set; } = new();
    }

    public class EmployerDashboardViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public List<Job> ActiveJobs { get; set; } = new();
        public List<Application> RecentApplications { get; set; } = new();
        public int TotalJobs { get; set; }
        public int TotalApplications { get; set; }
        public int NewApplications { get; set; }
        public Dictionary<string, int> ApplicationsByStatus { get; set; } = new();
    }

    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalEmployers { get; set; }
        public int TotalApplicants { get; set; }
        public int TotalJobs { get; set; }
        public int ActiveJobs { get; set; }
        public int TotalApplications { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int NewJobsThisMonth { get; set; }
        public List<ApplicationUser> LatestUsers { get; set; } = new();
        public List<Job> LatestJobs { get; set; } = new();
    }

    // ===== Application Status ViewModel =====
    public class UpdateApplicationStatusViewModel
    {
        public int ApplicationId { get; set; }
        public ApplicationStatus Status { get; set; }
        public string? Notes { get; set; }
    }
}
