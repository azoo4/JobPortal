using Microsoft.AspNetCore.Identity;

namespace JobPortal.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // Employer specific
        public string? CompanyName { get; set; }
        public string? CompanyDescription { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? CompanyLogo { get; set; }
        public string? Industry { get; set; }
        public string? CompanyLocation { get; set; }

        // Applicant specific
        public string? Bio { get; set; }
        public string? Skills { get; set; }
        public string? CurrentPosition { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? CvPath { get; set; }
        public string? LinkedInUrl { get; set; }

        public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
        public virtual ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
