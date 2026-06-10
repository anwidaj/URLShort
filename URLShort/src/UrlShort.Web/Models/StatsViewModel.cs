namespace UrlShort.Web.Models;

public class StatsViewModel 
{
    public int TotalClicks { get; set; }
    public List<URLShort.UrlShort.Core.Models.ShortUrl> TopUrls { get; set; } = new();
    public Dictionary<string, int> BrowserStats { get; set; } = new();
    public Dictionary<string, int> ReferrerStats { get; set; } = new();
    public Dictionary<string, int> CategoryStats { get; set; } = new();
}
