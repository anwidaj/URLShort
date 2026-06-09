using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShort.Core.Services;
using URLShort.UrlShort.Core.Data;
using URLShort.UrlShort.Core.Models;

namespace UrlShort.Web.Controllers;

[Authorize]
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
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
        {
            return RedirectToAction("Login", "Auth");
        }
        
        var categories = await _categoryService.GetUserCategories(currentUserId);
        ViewBag.Categories = categories;
        
        var userUrls = await _context.ShortUrls
            .Include(u => u.Clicks)
            .Include(u => u.Category)
            .Where(u => u.UserId == currentUserId)
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
        
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
        {
            return RedirectToAction("Login", "Auth");
        }
        
        var shortUrl = await _urlShortenerService.CreateShortUrl(url, currentUserId, categoryId, customCode);

        if (shortUrl == null && !string.IsNullOrWhiteSpace(customCode))
        {
            TempData["Error"] = "This custom short code is already taken. Please choose another one.";
            return RedirectToAction(nameof(Index));
        }

        if (shortUrl != null && categoryId.HasValue)
        {
            await _urlShortenerService.UpdateUrlCategory(shortUrl.Id, currentUserId, categoryId.Value);
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
            TempData["Error"] = "Nazwa kategorii nie może być pusta!";
            return RedirectToAction(nameof(Index));
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var newCategory = await _categoryService.CreateCategory(categoryName, currentUserId);

        if (newCategory == null)
        {
            TempData["Error"] = "Kategoria o takiej nazwie już istnieje!";
        }
        else
        {
            TempData["Success"] = $"Kategoria '{categoryName}' została dodana!";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var success = await _urlShortenerService.DeleteShortUrl(id, currentUserId);
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
    
}