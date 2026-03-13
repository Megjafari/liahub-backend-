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
public class ApplicationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ApplicationsController(AppDbContext db)
    {
        _db = db;
    }

    // Get all applications for the current user
    [HttpGet]
    public async Task<IActionResult> GetApplications()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var applications = await _db.Applications
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        return Ok(applications);
    }

    // Log a new application
    [HttpPost]
    public async Task<IActionResult> CreateApplication([FromBody] CreateApplicationRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var application = new Application
        {
            Id = Guid.NewGuid(),
            UserId = userId.Value,
            ExternalJobId = request.ExternalJobId,
            JobTitle = request.JobTitle,
            Employer = request.Employer,
            Location = request.Location,
            Source = request.Source,
            Link = request.Link,
            Notes = request.Notes,
            IsManual = request.IsManual,
            ContactName = request.ContactName,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            Status = "Sökt",
            AppliedAt = request.AppliedAt ?? DateTime.UtcNow
        };

        await _db.Applications.AddAsync(application);
        await _db.SaveChangesAsync();

        return Ok(application);
    }

    // Update application status
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateApplication(Guid id, [FromBody] UpdateApplicationRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var application = await _db.Applications
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (application == null) return NotFound();

        application.Status = request.Status ?? application.Status;
        application.ContactName = request.ContactName ?? application.ContactName;
        application.ContactEmail = request.ContactEmail ?? application.ContactEmail;
        application.ContactPhone = request.ContactPhone ?? application.ContactPhone;

        await _db.SaveChangesAsync();

        return Ok(application);
    }

    // Delete an application
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteApplication(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var application = await _db.Applications
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (application == null) return NotFound();

        _db.Applications.Remove(application);
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

public class CreateApplicationRequest
{
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
    public DateTime? AppliedAt { get; set; }
}

public class UpdateApplicationRequest
{
    public string? Status { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
}