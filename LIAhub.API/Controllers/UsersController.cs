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

    [HttpPost("me")]
    public async Task<IActionResult> GetOrCreateProfile()
    {
        var supabaseId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub");

        if (supabaseId == null) return Unauthorized();

        var email = User.FindFirstValue(ClaimTypes.Email)
                    ?? User.FindFirstValue("email") ?? string.Empty;

        var user = await _db.Users
            .Include(u => u.TechStacks)
            .FirstOrDefaultAsync(u => u.Id == Guid.Parse(supabaseId));

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

        return Ok(new
        {
            user.Id,
            user.Email,
            user.Name,
            user.City,
            TechStacks = user.TechStacks.Select(t => t.Tech).ToList()
        });
    }

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

        if (request.TechStacks != null)
        {
            var existing = await _db.UserTechStacks
                .Where(t => t.UserId == user.Id)
                .ToListAsync();
            _db.UserTechStacks.RemoveRange(existing);
            await _db.SaveChangesAsync();

            var newTechs = request.TechStacks
                .Select(t => new UserTechStack { Id = Guid.NewGuid(), UserId = user.Id, Tech = t })
                .ToList();
            await _db.UserTechStacks.AddRangeAsync(newTechs);
        }

        await _db.SaveChangesAsync();

        var updatedUser = await _db.Users
            .Include(u => u.TechStacks)
            .FirstAsync(u => u.Id == user.Id);

        return Ok(new
        {
            updatedUser.Id,
            updatedUser.Email,
            updatedUser.Name,
            updatedUser.City,
            TechStacks = updatedUser.TechStacks.Select(t => t.Tech).ToList()
        });
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