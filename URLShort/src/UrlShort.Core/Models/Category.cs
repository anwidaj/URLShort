namespace URLShort.UrlShort.Core.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // Foreign key to User
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public List<ShortUrl> ShortUrls { get; set; } = new();
}