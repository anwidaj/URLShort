using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShort.Core.Services;
using URLShort.UrlShort.Core.Data;
using UrlShort.Web.Filters;
using UrlShort.Web.Models;

namespace UrlShort.Web.Controllers;

[SessionAuthorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDb _context;
    private readonly AuthService _authService;

    public AdminController(AppDb context, AuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _context.Users.ToListAsync();
        return View(users);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _authService.RegisterAsync(model.Username, model.Password);

        if (user == null)
        {
            ModelState.AddModelError("Username", "Username is in use");
            return View(model);
        }
        
        TempData["Success"] = $"User {user.Username} created successfully.";
        return RedirectToAction(nameof(Index));
    }
}
