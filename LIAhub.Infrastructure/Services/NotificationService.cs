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
        var platformUrl = "https://liahub.meghdadjafari.dev";

        var jobList = string.Join("\n\n", jobs.Select(j =>
            $"{j.Title} – {j.Employer}{(j.City != null ? $" i {j.City}" : "")}\n{j.Url}"));

        var body = $"""
            Hej!

            Vi hittade {jobs.Count} nya jobb som matchar din tech stack på LIAHub.

            Här är de senaste matchningarna:

            {jobList}

            Se alla matchningar:
            {platformUrl}

            Lycka till med ditt LIA-sökande!
            / LIAHub
            """;

        var message = new EmailMessage
        {
            From = "LIAHub <noreply@liahub.meghdadjafari.dev>",
            To = { email },
            Subject = $"🔎 {jobs.Count} nya jobb matchar din tech stack",
            TextBody = body
        };

        await _resend.EmailSendAsync(message);
    }
}