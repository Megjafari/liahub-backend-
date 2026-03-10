namespace LIAhub.Core.Models;
public class Application
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ExternalJobId { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Employer { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Sökt"; // Sökt, Intervju, Avslag, Erbjudande

    public User User { get; set; } = null!;
}