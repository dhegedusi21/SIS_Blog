using Microsoft.AspNetCore.Mvc;

namespace SIS_Blog.Controllers;

public class BlogController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        // API-driven: view is a shell; client JS will call /api/posts to load data
        return View();
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        ViewBag.PostId = id;
        return View();
    }

    [HttpGet]
    public IActionResult EditComment(int id)
    {
        ViewBag.CommentId = id;
        return View();
    }
}
