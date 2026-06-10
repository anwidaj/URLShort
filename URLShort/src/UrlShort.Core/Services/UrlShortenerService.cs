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

    public async Task<ShortUrl?> CreateShortUrl(string url, int userId, int? categoryId = null, string? customCode = null)
    {
        var existingUrl = await _context.ShortUrls.FirstOrDefaultAsync(u => u.OriginalUrl == url && u.UserId == userId);
        if (existingUrl != null)
        {
            return existingUrl;
        }
        
        string code;
        if (!string.IsNullOrWhiteSpace(customCode))
        {
            bool customExists = await _context.ShortUrls.AnyAsync(u => u.ShortCode == customCode);
            if (customExists) return null;
            code = customCode;
        }
        else
        {
            code = await GenerateUniqueRandomCode(4);
        }
        
        var shortUrlObj = new ShortUrl
        {
            OriginalUrl = url,
            ShortCode = code,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            UserId = userId,
            CategoryId = categoryId
        };
        
        _context.ShortUrls.Add(shortUrlObj);
        await _context.SaveChangesAsync();
        
        return shortUrlObj;
    }

    private async Task<string> GenerateUniqueRandomCode(int length)
    {
        while (true)
        {
            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = Alphabet[Random.Shared.Next(Alphabet.Length)];
            }
            var code = new string(chars);

            bool exists = await _context.ShortUrls.AnyAsync(u => u.ShortCode == code);
            if (!exists)
            {
                return code;
            }
        }
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
    
    public async Task<bool> DeleteShortUrl(int urlId, int userId)
    {
        var url = await _context.ShortUrls.FirstOrDefaultAsync(u => u.Id == urlId && u.UserId == userId);
        if (url == null) return false;
        
        _context.ShortUrls.Remove(url);
        await _context.SaveChangesAsync();
        return true;
    }
}