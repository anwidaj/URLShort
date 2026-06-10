using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URLShort.UrlShort.Core.Data;
using URLShort.UrlShort.Core.Models;

namespace UrlShort.Web.Controllers;

public class RedirectController : Controller
{
    private readonly AppDb _context;
    
    public  RedirectController(AppDb context)
    {
        _context = context;
    }

    [HttpGet("r/{code}")]
    public async Task<IActionResult> HandleRedirect(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return NotFound();
        
        var shortUrl = await _context.ShortUrls.FirstOrDefaultAsync(u => u.ShortCode == code && u.IsActive);
        
        if (shortUrl == null) return View("LinkNotFound");

        var click = new Click
        {
            ShortUrlId = shortUrl.Id,
            ClickedAt = DateTime.UtcNow,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers["User-Agent"].ToString(),
            Referrer =  Request.Headers["Referer"].ToString()
        };
        
        _context.Add(click);
        
        await _context.SaveChangesAsync();
        
        return Redirect(shortUrl.OriginalUrl);
    }
}