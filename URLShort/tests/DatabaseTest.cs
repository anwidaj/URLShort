using Microsoft.EntityFrameworkCore;
using URLShort.UrlShort.Core.Data;
using UrlShort.Core.Services;

namespace tests;

public class DatabaseTest
{

    private AppDb GetInMemoryDbContext(){
        var options = new DbContextOptionsBuilder<AppDb>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options; // Always recreate DB for each test
        
        return new AppDb(options);
    }
    [Fact]
    public async Task CreateShortUrl()
    {
        using var context = GetInMemoryDbContext();
        var service = new UrlShortenerService(context);
        string url = "https://github.com/anwidaj/URLShort";
        int userId = 90;

        var result = await service.CreateShortUrl(url, userId);

        Assert.NotNull(result);
        Assert.Equal(url, result.OriginalUrl);
        Assert.Equal(userId, result.UserId);
        Assert.False(string.IsNullOrEmpty(result.ShortCode));
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateShortUrlWithCustomCode()
    {
        using var context = GetInMemoryDbContext();
        var service = new UrlShortenerService(context);
        string url = "https://github.com/anwidaj/URLShort";
        int userId = 90;

        var result = await service.CreateShortUrl(url, userId, customCode: "custom");

        Assert.NotNull(result);
        Assert.Equal(url, result.OriginalUrl);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("custom", result.ShortCode);
        Assert.True(result.IsActive);
    }
}
