using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URLShort.UrlShort.Core.Data;
using UrlShort.Web.Filters;
using UrlShort.Web.Models;

namespace UrlShort.Web.Controllers;

[SessionAuthorize]
public class StatsController : Controller
{
    private readonly AppDb _context;

    public StatsController(AppDb context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var currentUserId = HttpContext.Session.GetInt32("UserId");
        if (currentUserId == null) return RedirectToAction("Login", "Auth");

        var urls = await _context.ShortUrls
            .Include(u => u.Category)
            .Include(u => u.Clicks)
            .Where(u => u.UserId == currentUserId.Value)
            .ToListAsync();

        var viewModel = new StatsViewModel();
        
        viewModel.TotalClicks = urls.Sum(u => u.Clicks.Count);
        viewModel.TopUrls = urls.OrderByDescending(u => u.Clicks.Count).Take(5).ToList();

        var allClicks = urls.SelectMany(u => u.Clicks).ToList();

        // Browser parsing
        viewModel.BrowserStats = allClicks
            .GroupBy(c => ParseBrowser(c.UserAgent))
            .ToDictionary(g => g.Key, g => g.Count());

        // Referrer parsing
        viewModel.ReferrerStats = allClicks
            .GroupBy(c => string.IsNullOrWhiteSpace(c.Referrer) ? "Direct" : c.Referrer)
            .ToDictionary(g => g.Key, g => g.Count());

        // Category parsing
        viewModel.CategoryStats = urls
            .SelectMany(u => u.Clicks.Select(c => new { Click = c, CategoryName = u.Category?.Name ?? "No Category" }))
            .GroupBy(x => x.CategoryName)
            .ToDictionary(g => g.Key, g => g.Count());

        return View(viewModel);
    }

    private string ParseBrowser(string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent)) return "Unknown";
        
        // Browsers built on Chromium often include "Chrome" in their user agent,
        // so we must check for their specific identifiers BEFORE checking for Chrome.
        if (userAgent.Contains("OPR") || userAgent.Contains("Opera")) return "Opera";
        if (userAgent.Contains("Edg")) return "Edge";
        if (userAgent.Contains("Firefox")) return "Firefox";
        if (userAgent.Contains("Chrome")) return "Chrome";
        if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome") && !userAgent.Contains("Chromium")) return "Safari";
        
        return "Other";
    }
}
