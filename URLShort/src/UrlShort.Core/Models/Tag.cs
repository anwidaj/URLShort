namespace URLShort.UrlShort.Core.Models;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public List<ShortUrl> ShortUrls { get; set; } = new();
}