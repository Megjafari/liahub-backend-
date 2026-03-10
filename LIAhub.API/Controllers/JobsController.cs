using LIAhub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LIAhub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly AppDbContext _db;

    public JobsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetJobs(
        [FromQuery] string? city,
        [FromQuery] string? tech,
        [FromQuery] string? search)
    {
        var query = _db.CachedJobs
            .Where(j => j.ExpiresAt > DateTime.UtcNow)
            .AsQueryable();

        if (!string.IsNullOrEmpty(city))
            query = query.Where(j => j.City != null && 
                j.City.ToLower().Contains(city.ToLower()));

        if (!string.IsNullOrEmpty(tech))
            query = query.Where(j => j.TechTags.Contains(tech));

        if (!string.IsNullOrEmpty(search))
            query = query.Where(j => 
                j.Title.ToLower().Contains(search.ToLower()) ||
                j.Employer.ToLower().Contains(search.ToLower()));

        var jobs = await query
            .OrderByDescending(j => j.FetchedAt)
            .Select(j => new
            {
                j.Id,
                j.ExternalId,
                j.Title,
                j.Employer,
                j.City,
                j.TechTags,
                j.Url,
                j.FetchedAt
            })
            .ToListAsync();

        return Ok(jobs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetJob(Guid id)
    {
        var job = await _db.CachedJobs.FindAsync(id);
        if (job == null) return NotFound();
        return Ok(job);
    }
}