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
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public NotificationsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var settings = await _db.NotificationSettings
            .FirstOrDefaultAsync(n => n.UserId == userId);

        if (settings == null)
            return Ok(new { active = false, frequency = "daily" });

        return Ok(new { active = settings.Active, frequency = settings.Frequency });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateNotificationRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var settings = await _db.NotificationSettings
            .FirstOrDefaultAsync(n => n.UserId == userId);

        if (settings == null)
        {
            settings = new NotificationSetting
            {
                Id = Guid.NewGuid(),
                UserId = userId.Value,
                Active = request.Active,
                Frequency = request.Frequency ?? "daily"
            };
            await _db.NotificationSettings.AddAsync(settings);
        }
        else
        {
            settings.Active = request.Active;
            settings.Frequency = request.Frequency ?? settings.Frequency;
        }

        await _db.SaveChangesAsync();
        return Ok(new { active = settings.Active, frequency = settings.Frequency });
    }

    private Guid? GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue("sub");
        return id != null ? Guid.Parse(id) : null;
    }
}

public class UpdateNotificationRequest
{
    public bool Active { get; set; }
    public string? Frequency { get; set; }
}