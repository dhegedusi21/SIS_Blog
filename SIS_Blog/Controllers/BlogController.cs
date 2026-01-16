using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIS_Blog.Data;
using SIS_Blog.Models;

namespace SIS_Blog.Controllers;

public class BlogController : Controller
{
    private readonly BlogDbContext _db;

    public BlogController(BlogDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchString)
    {
        var query = _db.Posts
            .Include(p => p.User)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(p => EF.Functions.Like(p.Title, $"%{searchString}%") || EF.Functions.Like(p.Content, $"%{searchString}%"));
        }

        var posts = await query.OrderByDescending(p => p.Id).ToListAsync();
        return View(posts);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var post = await _db.Posts
            .Include(p => p.User)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return NotFound();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId))
            return Challenge();

        if (post.UserId != currentUserId)
            return Forbid();

        return View(post);
    }

    // Accept form POST from Views/Blog/Edit.cshtml
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Post model)
    {
        if (model == null) return BadRequest();

        var post = await _db.Posts.FindAsync(model.Id);
        if (post == null) return NotFound();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId))
            return Challenge();

        if (post.UserId != currentUserId)
            return Forbid();

        // apply changes
        if (!string.IsNullOrWhiteSpace(model.Title)) post.Title = model.Title;
        post.Content = model.Content ?? string.Empty;
        post.UpdatedAt = System.DateTime.UtcNow.ToString("o");

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> EditComment(int id)
    {
        var comment = await _db.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null) return NotFound();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId))
            return Challenge();

        if (comment.UserId != currentUserId)
            return Forbid();

        return View(comment);
    }

    // Accept form POST from Views/Blog/EditComment.cshtml
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditComment(Comment model)
    {
        if (model == null) return BadRequest();

        var comment = await _db.Comments.FindAsync(model.Id);
        if (comment == null) return NotFound();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId))
            return Challenge();

        if (comment.UserId != currentUserId)
            return Forbid();

        comment.Content = model.Content ?? string.Empty;
        comment.UpdatedAt = System.DateTime.UtcNow.ToString("o");

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // Replace GET delete with POST delete to avoid dangerous GET state changes
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId))
            return Challenge();

        if (post.UserId != currentUserId)
            return Forbid();

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCommentConfirmed(int id)
    {
        var comment = await _db.Comments.FindAsync(id);
        if (comment == null) return NotFound();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId))
            return Challenge();

        if (comment.UserId != currentUserId)
            return Forbid();

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
