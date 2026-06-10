using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URLShort.UrlShort.Core.Data;
using URLShort.UrlShort.Core.Models;
using UrlShort.Core.Services;
using UrlShort.Web.Filters;

namespace UrlShort.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[ApiKeyAuth]
public class UrlController : ControllerBase
{
    private readonly AppDb _context;
    private readonly UrlShortenerService _urlService;
    private readonly CategoryService _categoryService;

    public UrlController(AppDb context, UrlShortenerService urlService, CategoryService categoryService)
    {
        _context = context;
        _urlService = urlService;
        _categoryService = categoryService;
    }

    private int GetCurrentUserId()
    {
        return (int)HttpContext.Items["ApiUserId"]!;
    }

    [HttpGet]
    public async Task<IActionResult> GetUrls()
    {
        var userId = GetCurrentUserId();
        var urls = await _context.ShortUrls
            .Where(u => u.UserId == userId)
            .Select(u => new { u.Id, u.OriginalUrl, u.ShortCode, u.CreatedAt, u.IsActive })
            .ToListAsync();
            
        return Ok(urls);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var userId = GetCurrentUserId();
        var categories = await _categoryService.GetUserCategories(userId);
        var result = categories.Select(c => new { c.Id, c.Name }).ToList();
        return Ok(result);
    }

    public class CreateUrlRequest
    {
        public string OriginalUrl { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public string? CustomCode { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> CreateUrl([FromBody] CreateUrlRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.OriginalUrl))
            return BadRequest("OriginalUrl is required");

        var userId = GetCurrentUserId();
        var shortUrl = await _urlService.CreateShortUrl(request.OriginalUrl, userId, request.CategoryId, request.CustomCode);
        
        if (shortUrl == null && !string.IsNullOrWhiteSpace(request.CustomCode))
            return BadRequest("Custom short code is already taken");

        if (shortUrl == null) return BadRequest("Could not create short URL");

        if (shortUrl != null && request.CategoryId.HasValue)
        {
            await _urlService.UpdateUrlCategory(shortUrl.Id, userId, request.CategoryId.Value);
        }
        
        return CreatedAtAction(nameof(GetUrls), new { id = shortUrl.Id }, new { shortUrl.Id, shortUrl.OriginalUrl, shortUrl.ShortCode, shortUrl.CreatedAt, shortUrl.IsActive, shortUrl.CategoryId });
    }

    public class UpdateUrlRequest
    {
        public string OriginalUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUrl(int id, [FromBody] UpdateUrlRequest request)
    {
        var userId = GetCurrentUserId();
        var url = await _context.ShortUrls.FirstOrDefaultAsync(u => u.Id == id && u.UserId == userId);
        
        if (url == null) return NotFound();

        url.OriginalUrl = request.OriginalUrl;
        url.IsActive = request.IsActive;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUrl(int id)
    {
        var userId = GetCurrentUserId();
        var url = await _context.ShortUrls.Include(u => u.Clicks).FirstOrDefaultAsync(u => u.Id == id && u.UserId == userId);
        
        if (url == null) return NotFound();

        _context.ShortUrls.Remove(url);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
