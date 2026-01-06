using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIS_Blog.Data;
using SIS_Blog.Models;

namespace SIS_Blog.Controllers;

public class UserController : Controller
{
    public UserController()
    {
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // Login is handled via API (/api/auth/login). Keep GET to return view shell.

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // Registration is handled via API (/api/auth/register). Keep GET Register view only.

    [HttpGet]
    public IActionResult UserOverview()
    {
        // API-driven: view is a shell; client JS will call /api/users to load data
        return View();
    }

    [HttpGet]
    public IActionResult EditUser(int id)
    {
        // API-driven: view is a shell; client JS will call /api/users/{id} to load data
        ViewBag.UserId = id;
        return View();
    }

    // User edits and deletes are handled via API (/api/users)

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
