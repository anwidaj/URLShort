namespace URLShort.UrlShort.Core.Models;

public class ShortUrlTag
{
    public int Id { get; set; }
    
    public int ShortUrlId { get; set; }
    public ShortUrl ShortUrl { get; set; } = null!;
    
    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
    
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}