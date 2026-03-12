using System.Text.Json.Serialization;

namespace LIAhub.Core.Models;
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? School { get; set; }
    public string? LiaPeriod { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<UserTechStack> TechStacks { get; set; } = new List<UserTechStack>();
    [JsonIgnore]
    public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    [JsonIgnore]
    public ICollection<Application> Applications { get; set; } = new List<Application>();
    [JsonIgnore]
    public NotificationSetting? NotificationSetting { get; set; }
}