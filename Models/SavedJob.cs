using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.Models
{
    public class SavedJob
    {
        public int Id { get; set; }
        public DateTime SavedAt { get; set; } = DateTime.Now;

        public string UserId { get; set; } = string.Empty;
        public int JobId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("JobId")]
        public virtual Job Job { get; set; } = null!;
    }

    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Link { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public NotificationType Type { get; set; }

        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
    }

    public enum NotificationType
    {
        ApplicationReceived,
        ApplicationStatusChanged,
        NewJobMatch,
        General
    }
}
