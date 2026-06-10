

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SQLitePCL;
using UrlShort.Core.Services;
using URLShort.UrlShort.Core.Data;
using UrlShort.Web.Models;

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
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
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
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
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

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };
        
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe
        };
        
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, 
            new ClaimsPrincipal(claimsIdentity), 
            authProperties);
        
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet]
    public IActionResult Profile()
    {
        return View();
    }

    [Authorize]
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
        
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
        {
            return RedirectToAction("Login");
        }
        
        var user = await _context.Users.FindAsync(currentUserId);
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
    