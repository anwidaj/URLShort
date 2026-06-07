namespace URLShort.UrlShort.Core.Models;

public class ShortUrl
{
    public int Id { get; set; }
    public string OriginalUrl { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Foreign key to user
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    // Foreign key to category
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public List<Tag> Tags { get; set; } = new();
    public List<Click> Clicks { get; set; } = new();
}