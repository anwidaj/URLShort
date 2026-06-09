using Microsoft.EntityFrameworkCore;
using URLShort.UrlShort.Core.Data;
using URLShort.UrlShort.Core.Models;

namespace UrlShort.Core.Services;

public class CategoryService
{
    private readonly AppDb _context;
    
    public CategoryService(AppDb context)
    {
        _context = context;
    }

    public async Task<Category?> CreateCategory(string name, int userId)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Name.ToLower() == name.ToLower() && c.UserId == userId);
            
        if (categoryExists)
        {
            return null; 
        }

        var newCategory = new Category
        {
            Name = name.Trim(),
            UserId = userId
        };

        _context.Categories.Add(newCategory);
        await _context.SaveChangesAsync();

        return newCategory;
    }

    public async Task<List<Category>> GetUserCategories(int userId)
    {
        return await _context.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}