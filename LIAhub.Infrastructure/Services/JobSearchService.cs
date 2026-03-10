using System.Text.Json;
using System.Text.RegularExpressions;
using LIAhub.Core.Models;

namespace LIAhub.Infrastructure.Services;

public class JobSearchService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://jobsearch.api.jobtechdev.se";

    public JobSearchService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CachedJob>> FetchLiaJobsAsync()
    {
        var allJobs = new List<CachedJob>();

        // Search with multiple keyword combinations to maximize results
        var searchQueries = new[]
        {
            "LIA systemutvecklare",
            "LIA utvecklare",
            "praktik systemutvecklare",
            "praktik utvecklare",
            "internship developer",
            "LIA .NET",
            "LIA backend",
            "LIA frontend"
        };

        foreach (var keyword in searchQueries)
        {
            var offset = 0;
            var limit = 100;
            var hasMore = true;

            while (hasMore)
            {
                var url = $"{BaseUrl}/search?q={Uri.EscapeDataString(keyword)}&limit={limit}&offset={offset}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JobSearchResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Stop if no results returned
                if (result?.Hits == null || result.Hits.Count == 0)
                {
                    hasMore = false;
                    break;
                }

                var filteredJobs = result.Hits
                    // Filter out anything that is not a real LIA/internship listing
                    .Where(hit => IsLiaJob(hit.Headline, hit.Description?.Text))
                    // Avoid duplicates from overlapping searches
                    .Where(hit => allJobs.All(j => j.ExternalId != hit.Id))
                    .Select(hit => new CachedJob
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = hit.Id,
                        Title = hit.Headline ?? string.Empty,
                        Employer = hit.Employer?.Name ?? string.Empty,
                        City = hit.WorkplaceAddress?.City,
                        Description = hit.Description?.Text,
                        // Extract which technologies are mentioned in the listing
                        TechTags = ExtractTechTags(hit.Description?.Text),
                        Url = hit.WebPage ?? $"https://arbetsformedlingen.se/platsbanken/annonser/{hit.Id}",
                        FetchedAt = DateTime.UtcNow,
                        // Cache the listing for 6 hours, then fetch fresh data
                        ExpiresAt = DateTime.UtcNow.AddHours(6)
                    }).ToList();

                allJobs.AddRange(filteredJobs);

                // If we got fewer than limit, there are no more pages
                if (result.Hits.Count < limit)
                    hasMore = false;
                else
                    offset += limit;
            }
        }

        return allJobs;
    }

    private bool IsLiaJob(string? title, string? description)
    {
        var titleText = (title ?? string.Empty).ToLower();
        var fullText = $"{title} {description}".ToLower();

        // Match "lia" as a whole word — not part of "reliability" or "compliance"
        bool liaInTitle = Regex.IsMatch(titleText, @"\blia\b") ||
                          titleText.Contains("lärande i arbete") ||
                          titleText.Contains("praktik") ||
                          titleText.Contains("praktikant") ||
                          titleText.Contains("internship");

        // If the listing contains these words it is likely not a student position
        var excludeKeywords = new[]
        {
            "senior", "erfaren", "minst 3 år", "minst 5 år",
            "experienced", "at least 3 years", "at least 5 years"
        };

        bool isSenior = excludeKeywords.Any(k => fullText.Contains(k));

        // Listing is relevant only if it has a LIA keyword AND is not a senior position
        return liaInTitle && !isSenior;
    }

    private List<string> ExtractTechTags(string? description)
    {
        if (string.IsNullOrEmpty(description)) return new List<string>();

        var tags = new List<string>();

        // Full list of technologies — users will later filter listings by these via their profile
        var techKeywords = new[]
        {
            // .NET ecosystem
            ".NET", "C#", "ASP.NET", "Blazor", "Entity Framework", "MAUI",
            // Frontend
            "React", "Angular", "Vue", "TypeScript", "JavaScript", "HTML", "CSS",
            // Backend
            "Java", "Python", "Node.js", "PHP", "Go", "Rust", "Kotlin",
            // Database
            "SQL", "PostgreSQL", "MySQL", "MongoDB", "Redis", "SQLite",
            // Cloud & DevOps
            "Azure", "AWS", "Docker", "Kubernetes", "CI/CD", "GitHub Actions",
            // Other
            "REST", "API", "Git", "Linux", "Scrum", "Agile"
        };

        foreach (var keyword in techKeywords)
        {
            if (description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                tags.Add(keyword);
        }

        return tags;
    }
}

// Models for deserializing the response from the JobSearch API
public class JobSearchResponse
{
    public List<JobHit>? Hits { get; set; }
}

public class JobHit
{
    public string Id { get; set; } = string.Empty;
    public string? Headline { get; set; }
    public string? WebPage { get; set; }
    public EmployerInfo? Employer { get; set; }
    public WorkplaceAddress? WorkplaceAddress { get; set; }
    public DescriptionInfo? Description { get; set; }
}

public class EmployerInfo
{
    public string? Name { get; set; }
}

public class WorkplaceAddress
{
    public string? City { get; set; }
}

public class DescriptionInfo
{
    public string? Text { get; set; }
}