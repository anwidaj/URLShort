
using Microsoft.EntityFrameworkCore;
using URLShort.UrlShort.Core.Data;
using URLShort.UrlShort.Core.Models;

namespace UrlShort.Core.Services;

public class AuthService
{
    private readonly AppDb _context;

    public AuthService(AppDb context)
    {
        _context = context;
    }

    public async Task<User?> RegisterAsync(string username, string rawPassword)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Username == username);
        if (userExists) return null;
        
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);
        var newUser = new User
        {
            Username = username,
            PasswordHash = passwordHash,
            Role = "User",
            ApiKey = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();
        return newUser;
    }

    public async Task<User?> LoginAsync(string username, string rawPassword)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return null;
        
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(rawPassword, user.PasswordHash);
        if (!isPasswordValid) return null;

        return user;
    }
}