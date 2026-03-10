public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? School { get; set; }
    public string? LiaPeriod { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserTechStack> TechStacks { get; set; } = new List<UserTechStack>();
    public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    public ICollection<Application> Applications { get; set; } = new List<Application>();
    public NotificationSetting? NotificationSetting { get; set; }
}