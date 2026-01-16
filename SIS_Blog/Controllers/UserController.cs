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


    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }


    [HttpGet]
    public IActionResult UserOverview()
    {
        return View();
    }

    [HttpGet]
    public IActionResult EditUser(int id)
    {
        ViewBag.UserId = id;
        return View();
    }


    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
