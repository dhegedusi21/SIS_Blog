using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SIS_Blog.Data;
using SIS_Blog.ApiModels;

namespace SIS_Blog.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly BlogDbContext _db;
    private readonly Microsoft.AspNetCore.Antiforgery.IAntiforgery _antiforgery;

    public PostsController(BlogDbContext db, Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery)
    {
        _db = db;
        _antiforgery = antiforgery;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var posts = await _db.Posts
            .Include(p => p.User)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        var dto = posts.Select(p => new PostDto
        {
            Id = p.Id,
            Title = p.Title,
            Content = p.Content,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            UserId = p.UserId,
            Username = p.User?.Username,
            Comments = p.Comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                UserId = c.UserId,
                BlogpostId = c.BlogpostId,
                Username = c.User?.Username
            }).ToList()
        }).ToList();

        return Ok(dto);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] PostDto model)
    {
        if (model == null) return BadRequest();
        if (string.IsNullOrWhiteSpace(model.Title)) return BadRequest("Title is required");

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId)) return Unauthorized();

        var post = new SIS_Blog.Models.Post
        {
            Title = model.Title,
            Content = model.Content,
            CreatedAt = DateTime.UtcNow.ToString("o"),
            UpdatedAt = DateTime.UtcNow.ToString("o"),
            UserId = currentUserId
        };

        // Validate antiforgery token for AJAX
        await _antiforgery.ValidateRequestAsync(HttpContext);

        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        model.Id = post.Id;
        model.CreatedAt = post.CreatedAt;
        model.UpdatedAt = post.UpdatedAt;

        return CreatedAtAction(nameof(Get), new { id = post.Id }, model);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] PostDto model)
    {
        if (model == null) return BadRequest();
        var p = await _db.Posts.FindAsync(id);
        if (p == null) return NotFound();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId)) return Unauthorized();
        if (p.UserId != currentUserId) return Forbid();

        // Validate antiforgery token for AJAX
        try { await _antiforgery.ValidateRequestAsync(HttpContext); } catch { }

        if (!string.IsNullOrWhiteSpace(model.Title)) p.Title = model.Title;
        p.Content = model.Content;
        p.UpdatedAt = DateTime.UtcNow.ToString("o");

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Posts.FindAsync(id);
        if (p == null) return NotFound();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId)) return Unauthorized();
        if (p.UserId != currentUserId) return Forbid();

        // Validate antiforgery token for AJAX
        await _antiforgery.ValidateRequestAsync(HttpContext);

        _db.Posts.Remove(p);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var p = await _db.Posts
            .Include(p => p.User)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null) return NotFound();

        var dto = new PostDto
        {
            Id = p.Id,
            Title = p.Title,
            Content = p.Content,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            UserId = p.UserId,
            Username = p.User?.Username,
            Comments = p.Comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                UserId = c.UserId,
                BlogpostId = c.BlogpostId,
                Username = c.User?.Username
            }).ToList()
        };

        return Ok(dto);
    }
}
