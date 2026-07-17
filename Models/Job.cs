using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.Models
{
    public class Job
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الوظيفة مطلوب")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "وصف الوظيفة مطلوب")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "المتطلبات مطلوبة")]
        public string Requirements { get; set; } = string.Empty;

        public string? Responsibilities { get; set; }

        [Required(ErrorMessage = "الموقع مطلوب")]
        public string Location { get; set; } = string.Empty;

        public bool IsRemote { get; set; } = false;

        [Required(ErrorMessage = "نوع الوظيفة مطلوب")]
        public JobType JobType { get; set; }

        public ExperienceLevel ExperienceLevel { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalaryMin { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalaryMax { get; set; }

        public string? SalaryCurrency { get; set; } = "USD";

        public bool ShowSalary { get; set; } = true;

        public string? Skills { get; set; }

        [Required(ErrorMessage = "القطاع مطلوب")]
        public string Category { get; set; } = string.Empty;

        public DateTime PostedAt { get; set; } = DateTime.Now;
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public int ViewCount { get; set; } = 0;

        // Foreign Key
        public string EmployerId { get; set; } = string.Empty;

        [ForeignKey("EmployerId")]
        public virtual ApplicationUser Employer { get; set; } = null!;

        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
        public virtual ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    }

    public enum JobType
    {
        [Display(Name = "دوام كامل")] FullTime,
        [Display(Name = "دوام جزئي")] PartTime,
        [Display(Name = "عقد")] Contract,
        [Display(Name = "تدريب")] Internship,
        [Display(Name = "مؤقت")] Temporary,
        [Display(Name = "عن بعد")] Remote
    }

    public enum ExperienceLevel
    {
        [Display(Name = "مبتدئ")] Entry,
        [Display(Name = "متوسط")] Mid,
        [Display(Name = "متقدم")] Senior,
        [Display(Name = "مدير")] Lead,
        [Display(Name = "مدير تنفيذي")] Executive
    }
}
