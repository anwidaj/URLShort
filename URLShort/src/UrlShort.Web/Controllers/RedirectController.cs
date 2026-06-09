using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URLShort.UrlShort.Core.Data;

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
        
        return Redirect(shortUrl.OriginalUrl);
    }
}