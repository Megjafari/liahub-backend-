using LIAhub.Infrastructure.Data;
using LIAhub.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LIAhub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SavedJobsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SavedJobsController(AppDbContext db)
    {
        _db = db;
    }

    // Get all saved jobs for the current user
    [HttpGet]
    public async Task<IActionResult> GetSavedJobs()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var jobs = await _db.SavedJobs
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.SavedAt)
            .ToListAsync();

        return Ok(jobs);
    }

    // Save a job
    [HttpPost]
    public async Task<IActionResult> SaveJob([FromBody] SaveJobRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var exists = await _db.SavedJobs
            .AnyAsync(j => j.UserId == userId && j.ExternalJobId == request.ExternalJobId);

        if (exists) return Conflict("Job already saved.");

        var savedJob = new SavedJob
        {
            Id = Guid.NewGuid(),
            UserId = userId.Value,
            ExternalJobId = request.ExternalJobId,
            JobTitle = request.JobTitle,
            Employer = request.Employer,
            Url = request.Url,
            SavedAt = DateTime.UtcNow
        };

        await _db.SavedJobs.AddAsync(savedJob);
        await _db.SaveChangesAsync();

        return Ok(savedJob);
    }

    // Delete a saved job
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSavedJob(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var job = await _db.SavedJobs
            .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId);

        if (job == null) return NotFound();

        _db.SavedJobs.Remove(job);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private Guid? GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue("sub");
        return id != null ? Guid.Parse(id) : null;
    }
}

public class SaveJobRequest
{
    public string ExternalJobId { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Employer { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}