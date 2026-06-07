namespace URLShort.UrlShort.Core.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // Foreign key to User
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    
    public List<ShortUrl> ShortUrls { get; set; } = new();
}