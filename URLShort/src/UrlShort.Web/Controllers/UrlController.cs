using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShort.Core.Services;
using URLShort.UrlShort.Core.Data;
using URLShort.UrlShort.Core.Models;
using UrlShort.Web.Filters;

namespace UrlShort.Web.Controllers;

[SessionAuthorize]
public class UrlController : Controller
{
    private readonly UrlShortenerService  _urlShortenerService;
    private readonly CategoryService _categoryService;
    private readonly AppDb _context;
    
    
    public UrlController(UrlShortenerService urlShortenerService, CategoryService categoryService, AppDb context)
    {
        _urlShortenerService = urlShortenerService;
        _categoryService = categoryService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUserId = HttpContext.Session.GetInt32("UserId");
        if (currentUserId == null)
        {
            return RedirectToAction("Login", "Auth");
        }
        
        var categories = await _categoryService.GetUserCategories(currentUserId.Value);
        ViewBag.Categories = categories;

        var user = await _context.Users.FindAsync(currentUserId.Value);
        ViewBag.ApiKey = user?.ApiKey;
        
        var userUrls = await _context.ShortUrls
            .Include(u => u.Clicks)
            .Include(u => u.Category)
            .Where(u => u.UserId == currentUserId.Value)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
        
        return View(userUrls);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string url, int? categoryId, string? customCode)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            TempData["Error"] = "Url cannot be empty";
            return RedirectToAction(nameof(Index));
        }
        
        var currentUserId = HttpContext.Session.GetInt32("UserId");
        if (currentUserId == null)
        {
            return RedirectToAction("Login", "Auth");
        }
        
        var shortUrl = await _urlShortenerService.CreateShortUrl(url, currentUserId.Value, categoryId, customCode);

        if (shortUrl == null && !string.IsNullOrWhiteSpace(customCode))
        {
            TempData["Error"] = "This custom short code is already taken. Please choose another one.";
            return RedirectToAction(nameof(Index));
        }

        if (shortUrl != null && categoryId.HasValue)
        {
            await _urlShortenerService.UpdateUrlCategory(shortUrl.Id, currentUserId.Value, categoryId.Value);
        }
        
        TempData["Success"] = "Url shortened successfully";
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            TempData["Error"] = "Category name cannot be empty!";
            return RedirectToAction(nameof(Index));
        }

        var currentUserId = HttpContext.Session.GetInt32("UserId");
        if (currentUserId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var newCategory = await _categoryService.CreateCategory(categoryName, currentUserId.Value);

        if (newCategory == null)
        {
            TempData["Error"] = "Category with this name already exists!";
        }
        else
        {
            TempData["Success"] = $"Category '{categoryName}' added successfully!";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = HttpContext.Session.GetInt32("UserId");
        if (currentUserId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var success = await _urlShortenerService.DeleteShortUrl(id, currentUserId.Value);
        if (success)
        {
            TempData["Success"] = "Link deleted successfully!";
        }
        else
        {
            TempData["Error"] = "Could not delete the link.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var currentUserId = HttpContext.Session.GetInt32("UserId");
        if (currentUserId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var success = await _categoryService.DeleteCategory(id, currentUserId.Value);
        if (success)
        {
            TempData["Success"] = "Category deleted successfully!";
        }
        else
        {
            TempData["Error"] = "Could not delete the category.";
        }
        return RedirectToAction(nameof(Index));
    }
}