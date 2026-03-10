using System.Text.Json;
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
        var keywords = "LIA praktik systemutvecklare .NET";
        var url = $"{BaseUrl}/search?q={Uri.EscapeDataString(keywords)}&limit=100";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JobSearchResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result?.Hits == null) return new List<CachedJob>();

        return result.Hits.Select(hit => new CachedJob
        {
            Id = Guid.NewGuid(),
            ExternalId = hit.Id,
            Title = hit.Headline ?? string.Empty,
            Employer = hit.Employer?.Name ?? string.Empty,
            City = hit.WorkplaceAddress?.City,
            Description = hit.Description?.Text,
            TechTags = ExtractTechTags(hit.Description?.Text),
            Url = hit.WebPage ?? $"https://arbetsformedlingen.se/platsbanken/annonser/{hit.Id}",
            FetchedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(6)
        }).ToList();
    }

    private List<string> ExtractTechTags(string? description)
    {
        if (string.IsNullOrEmpty(description)) return new List<string>();

        var tags = new List<string>();
        var keywords = new[] { ".NET", "C#", "React", "TypeScript", "JavaScript", "Java", "Python", "Azure", "SQL", "Angular" };

        foreach (var keyword in keywords)
        {
            if (description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                tags.Add(keyword);
        }

        return tags;
    }
}

// Response models
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