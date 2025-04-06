using Microsoft.EntityFrameworkCore;

namespace CatStealer.Data;

public class CatDbContext : DbContext
{
    public CatDbContext(DbContextOptions<CatDbContext> options) : base(options)
    {
    }

    public DbSet<CatEntity> Cats { get; set; }
    public DbSet<TagEntity> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure many-to-many relationship
        modelBuilder.Entity<CatTag>()
            .HasKey(ct => new { ct.CatId, ct.TagId });

        modelBuilder.Entity<CatTag>()
            .HasOne(ct => ct.Cat)
            .WithMany(c => c.CatTags)
            .HasForeignKey(ct => ct.CatId);

        modelBuilder.Entity<CatTag>()
            .HasOne(ct => ct.Tag)
            .WithMany(t => t.CatTags)
            .HasForeignKey(ct => ct.TagId);
    }
}