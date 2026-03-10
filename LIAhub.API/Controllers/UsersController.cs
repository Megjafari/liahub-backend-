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
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    // Get or create user profile on first login
    [HttpPost("me")]
    public async Task<IActionResult> GetOrCreateProfile()
    {
        var supabaseId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub");

        if (supabaseId == null) return Unauthorized();

        var email = User.FindFirstValue(ClaimTypes.Email)
                    ?? User.FindFirstValue("email") ?? string.Empty;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse(supabaseId));

        if (user == null)
        {
            user = new User
            {
                Id = Guid.Parse(supabaseId),
                Email = email,
                Name = string.Empty,
                CreatedAt = DateTime.UtcNow
            };
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
        }

        return Ok(user);
    }

    // Update user profile
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var supabaseId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub");

        if (supabaseId == null) return Unauthorized();

        var user = await _db.Users
            .Include(u => u.TechStacks)
            .FirstOrDefaultAsync(u => u.Id == Guid.Parse(supabaseId));

        if (user == null) return NotFound();

        user.Name = request.Name ?? user.Name;
        user.City = request.City;
        user.School = request.School;
        user.LiaPeriod = request.LiaPeriod;

        // Update tech stacks
        if (request.TechStacks != null)
        {
            _db.UserTechStacks.RemoveRange(user.TechStacks);
            user.TechStacks = request.TechStacks
                .Select(t => new UserTechStack { Id = Guid.NewGuid(), UserId = user.Id, Tech = t })
                .ToList();
        }

        await _db.SaveChangesAsync();
        return Ok(user);
    }
}

public class UpdateProfileRequest
{
    public string? Name { get; set; }
    public string? City { get; set; }
    public string? School { get; set; }
    public string? LiaPeriod { get; set; }
    public List<string>? TechStacks { get; set; }
}