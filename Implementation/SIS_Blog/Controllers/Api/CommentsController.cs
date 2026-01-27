using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SIS_Blog.Data;
using SIS_Blog.ApiModels;

namespace SIS_Blog.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly BlogDbContext _db;
    private readonly Microsoft.AspNetCore.Antiforgery.IAntiforgery _antiforgery;

    public CommentsController(BlogDbContext db, Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery)
    {
        _db = db;
        _antiforgery = antiforgery;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CommentDto model)
    {
        if (model == null) return BadRequest();

        var post = await _db.Posts.FindAsync(model.BlogpostId);
        if (post == null) return NotFound("Post not found");

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId)) return Unauthorized();

        var comment = new SIS_Blog.Models.Comment
        {
            Content = model.Content,
            CreatedAt = System.DateTime.UtcNow.ToString("o"),
            UpdatedAt = System.DateTime.UtcNow.ToString("o"),
            UserId = currentUserId,
            BlogpostId = model.BlogpostId
        };


        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        model.Id = comment.Id;
        return CreatedAtAction(nameof(Get), new { id = comment.Id }, model);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var c = await _db.Comments.Include(c => c.User).FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();

        var dto = new CommentDto
        {
            Id = c.Id,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            UserId = c.UserId,
            BlogpostId = c.BlogpostId,
            Username = c.User?.Username
        };

        return Ok(dto);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] CommentDto model)
    {
        var c = await _db.Comments.FindAsync(id);
        if (c == null) return NotFound();
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId)) return Unauthorized();
        if (c.UserId != currentUserId) return Forbid();
        await _antiforgery.ValidateRequestAsync(HttpContext);

        c.Content = model.Content;
        c.UpdatedAt = System.DateTime.UtcNow.ToString("o");

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.Comments.FindAsync(id);
        if (c == null) return NotFound();
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId)) return Unauthorized();
        if (c.UserId != currentUserId) return Forbid();

        await _antiforgery.ValidateRequestAsync(HttpContext);

        _db.Comments.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
