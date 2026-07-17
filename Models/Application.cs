using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.Models
{
    public class Application
    {
        public int Id { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.Now;

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        [StringLength(1000)]
        public string? CoverLetter { get; set; }

        public string? CvPath { get; set; }

        public string? EmployerNotes { get; set; }

        public DateTime? LastUpdated { get; set; }

        public bool IsViewedByEmployer { get; set; } = false;

        // Foreign Keys
        public string ApplicantId { get; set; } = string.Empty;
        public int JobId { get; set; }

        [ForeignKey("ApplicantId")]
        public virtual ApplicationUser Applicant { get; set; } = null!;

        [ForeignKey("JobId")]
        public virtual Job Job { get; set; } = null!;
    }

    public enum ApplicationStatus
    {
        [Display(Name = "قيد المراجعة")] Pending,
        [Display(Name = "تمت المراجعة")] Reviewed,
        [Display(Name = "مقابلة")] Interview,
        [Display(Name = "اختبار")] Assessment,
        [Display(Name = "مقبول")] Accepted,
        [Display(Name = "مرفوض")] Rejected,
        [Display(Name = "منسحب")] Withdrawn
    }
}
