namespace LIAhub.Core.Models;
public class Application
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ExternalJobId { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Employer { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Source { get; set; }
    public string? Link { get; set; }
    public string? Notes { get; set; }
    public bool IsManual { get; set; } = false;
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Sökt";

    public User User { get; set; } = null!;
}