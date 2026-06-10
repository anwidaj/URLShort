using Microsoft.EntityFrameworkCore;
using URLShort.UrlShort.Core.Models;

namespace URLShort.UrlShort.Core.Data;

public class AppDb : DbContext 
{
    public AppDb(DbContextOptions<AppDb> options) : base(options)
    {
        
    }

    public DbSet<User> Users { get; set; }
    public DbSet<ShortUrl> ShortUrls { get; set; }
    public DbSet<Click> Clicks { get; set; }
    public DbSet<Category> Categories { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder){
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ShortUrl>()
            .HasIndex(s => s.ShortCode)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
    }
}