using LIAhub.Core.Models;
using LIAhub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Resend;

namespace LIAhub.Infrastructure.Services;

public class NotificationService
{
    private readonly AppDbContext _db;
    private readonly IResend _resend;

    public NotificationService(AppDbContext db, IResend resend)
    {
        _db = db;
        _resend = resend;
    }

    public async Task SendNewJobNotificationsAsync(List<CachedJob> newJobs)
    {
        if (!newJobs.Any()) return;

        // Get all users with active notifications
        var users = await _db.Users
            .Include(u => u.TechStacks)
            .Include(u => u.NotificationSetting)
            .Where(u => u.NotificationSetting != null && u.NotificationSetting.Active)
            .ToListAsync();

        foreach (var user in users)
        {
            // Find jobs matching user's tech stacks
            var matchingJobs = newJobs
                .Where(job => !user.TechStacks.Any() ||
                    job.TechTags.Any(tag => user.TechStacks.Any(t => t.Tech == tag)))
                .ToList();

            if (!matchingJobs.Any()) continue;

            await SendEmailAsync(user.Email, matchingJobs);
        }
    }

    private async Task SendEmailAsync(string email, List<CachedJob> jobs)
    {
        var jobList = string.Join("\n", jobs.Select(j =>
            $"- {j.Title} hos {j.Employer} {(j.City != null ? $"i {j.City}" : "")} → {j.Url}"));

        var message = new EmailMessage
        {
            From = "LIAhub <noreply@liahub.se>",
            To = { email },
            Subject = $" {jobs.Count} nya LIA-annonser matchar din profil!",
            TextBody = $"Hej!\n\nDet finns {jobs.Count} nya LIA-annonser som matchar din profil:\n\n{jobList}\n\nLycka till!\n/ LIAhub"
        };

        await _resend.EmailSendAsync(message);
    }
}