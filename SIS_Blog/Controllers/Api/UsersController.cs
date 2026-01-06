using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SIS_Blog.Data;
using SIS_Blog.ApiModels;

namespace SIS_Blog.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly BlogDbContext _db;
    private readonly Microsoft.AspNetCore.Antiforgery.IAntiforgery _antiforgery;

    public UsersController(BlogDbContext db, Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery)
    {
        _db = db;
        _antiforgery = antiforgery;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Users.ToListAsync();
        var dto = users.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email
        }).ToList();

        return Ok(dto);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId)) return Unauthorized();
        if (u.Id != currentUserId) return Forbid();

        // Validate antiforgery token for AJAX
        await _antiforgery.ValidateRequestAsync(HttpContext);

        _db.Users.Remove(u);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UserDto model)
    {
        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId)) return Unauthorized();
        if (u.Id != currentUserId) return Forbid();

        u.Username = model.Username;
        u.Email = model.Email;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound();

        var dto = new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email
        };

        return Ok(dto);
    }
}
