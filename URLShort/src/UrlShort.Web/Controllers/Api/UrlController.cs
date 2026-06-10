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

    public UrlController(AppDb context, UrlShortenerService urlService)
    {
        _context = context;
        _urlService = urlService;
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

    public class CreateUrlRequest
    {
        public string OriginalUrl { get; set; } = string.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUrl([FromBody] CreateUrlRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.OriginalUrl))
            return BadRequest("OriginalUrl is required.");

        var userId = GetCurrentUserId();
        var shortUrl = await _urlService.CreateShortUrl(request.OriginalUrl, userId);
        
        if (shortUrl == null) return BadRequest("Could not create short URL.");
        
        return CreatedAtAction(nameof(GetUrls), new { id = shortUrl.Id }, new { shortUrl.Id, shortUrl.OriginalUrl, shortUrl.ShortCode, shortUrl.CreatedAt, shortUrl.IsActive });
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
