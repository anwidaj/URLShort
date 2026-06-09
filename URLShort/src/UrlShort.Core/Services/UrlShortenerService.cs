using Microsoft.EntityFrameworkCore;
using URLShort.UrlShort.Core.Data;
using URLShort.UrlShort.Core.Models;

namespace UrlShort.Core.Services;

public class UrlShortenerService
{
    private readonly AppDb _context;
    private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly int Base = Alphabet.Length; //62
    
    public UrlShortenerService(AppDb context)
    {
        _context = context;
    }

    public async Task<ShortUrl?> CreateShortUrl(string url, int userId, int? categoryId = null)
    {
        var existingUrl = await _context.ShortUrls.FirstOrDefaultAsync(u => u.OriginalUrl == url && u.UserId == userId);
        if (existingUrl  != null)
        {
            return existingUrl;
        }
        
        var shortUrlObj = new ShortUrl
        {
            OriginalUrl = url,
            ShortCode = string.Empty,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            UserId = userId,
            CategoryId = categoryId
            
        };
        
        _context.ShortUrls.Add(shortUrlObj);
        await _context.SaveChangesAsync();
        
        string code = EncodeToBase62(shortUrlObj.Id);
        shortUrlObj.ShortCode = code;
        await _context.SaveChangesAsync();
        
        return shortUrlObj;
    }

    private string EncodeToBase62(long id)
    {
        if (id == 0) return Alphabet[0].ToString();
        
        var sb = new System.Text.StringBuilder();

        while (id > 0)
        {
            int remainder = (int)(id % Base);
            sb.Insert(0, Alphabet[remainder]);
            id /= Base;
        }
        
        return sb.ToString();
    }

    public async Task<ShortUrl?> UpdateUrlCategory(int urlId, int userId, int? categoryId = null)
    {
        var existingUrl = await _context.ShortUrls.FirstOrDefaultAsync(u => u.Id == urlId && u.UserId == userId);
        if (existingUrl == null)
        {
            return null;
        }

        if (categoryId.HasValue)
        {
            var isCategoryUsers = await _context.Categories.AnyAsync(c => c.Id == categoryId.Value && c.UserId == userId);
            if (!isCategoryUsers)
            {
                return null;
            }
            
            
        }
        
        
        existingUrl.CategoryId = categoryId;
        await _context.SaveChangesAsync();
        return existingUrl;
    }    
}