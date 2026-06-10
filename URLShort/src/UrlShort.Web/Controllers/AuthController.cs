

using Microsoft.AspNetCore.Mvc;
using UrlShort.Core.Services;
using URLShort.UrlShort.Core.Data;
using UrlShort.Web.Models;
using UrlShort.Web.Filters;

namespace UrlShort.Web.Controllers;

public class AuthController : Controller
{
    private readonly AuthService _authService;
    private readonly AppDb _context;

    public AuthController(AuthService authService, AppDb context)
    {
        _authService = authService;
        _context = context;
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (HttpContext.Session.GetInt32("UserId") != null) return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _authService.RegisterAsync(model.Username, model.Password);

        if (user == null)
        {
            ModelState.AddModelError("Username", "Username is in use");
            return View(model);
        }
        
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetInt32("UserId") != null) return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        
        var user = await _authService.LoginAsync(model.Username, model.Password);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Wrong username or password");
            return View(model);
        }

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("Username", user.Username);
        HttpContext.Session.SetString("Role", user.Role);
        
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    [SessionAuthorize]
    [HttpGet]
    public IActionResult Profile()
    {
        return View();
    }

    [SessionAuthorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmNewPassword)
    {
        if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
        {
            TempData["Error"] = "Passwords cannot be empty";
            return RedirectToAction(nameof(Profile));
        }

        if (newPassword != confirmNewPassword)
        {
            TempData["Error"] =  "Passwords do not match";
            return RedirectToAction(nameof(Profile));
        }
        
        var currentUserId = HttpContext.Session.GetInt32("UserId");
        if (currentUserId == null)
        {
            return RedirectToAction("Login");
        }
        
        var user = await _context.Users.FindAsync(currentUserId.Value);
        if (user == null)
        {
            return RedirectToAction("Login");
        }
        
        if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
        {
            TempData["Error"] = "Old password is incorrect";
            return RedirectToAction(nameof(Profile));
        }
        
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await  _context.SaveChangesAsync();
        
        TempData["Success"] = "Password has been changed";
        return RedirectToAction(nameof(Profile));
    }
    
}
    