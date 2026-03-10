public class SavedJob
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ExternalJobId { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Employer { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}