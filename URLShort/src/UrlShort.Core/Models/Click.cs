namespace URLShort.UrlShort.Core.Models;

public class Click
{
    public int Id { get; set; }
    public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } =  string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string Referrer { get; set; } = string.Empty;
    
    // Foreign key to ShortUrl
    public int ShortUrlId { get; set; }
    public ShortUrl ShortUrl { get; set; } = null!;

}