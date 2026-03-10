namespace LIAhub.Core.Models;
public class CachedJob
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Employer { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Description { get; set; }
    public List<string> TechTags { get; set; } = new();
    public string Url { get; set; } = string.Empty;
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
}