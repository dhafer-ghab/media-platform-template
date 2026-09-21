using Microsoft.EntityFrameworkCore;
using Storage.Domain;

namespace Storage.Infrastracture.Persistence;

internal class StorageDbContext : DbContext
{
    public StorageDbContext(DbContextOptions<StorageDbContext> options) : base(options)
    {
    }

    public DbSet<MediaAsset> MediaAssets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Storage");

        modelBuilder.Entity<MediaAsset>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.FileName)
                .IsRequired()
                .HasMaxLength(512);

            entity.Property(m => m.OriginalFileName)
                .IsRequired()
                .HasMaxLength(512);

            entity.Property(m => m.ContentType)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(m => m.FileSize)
                .IsRequired();

            entity.Property(m => m.StorageProvider)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(m => m.BucketName)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(m => m.StorageKey)
                .IsRequired()
                .HasMaxLength(1024);

            entity.Property(m => m.Url)
                .IsRequired()
                .HasMaxLength(2048);

            entity.HasIndex(m => m.StorageKey);
            entity.HasIndex(m => m.CreatedDate);
        });

        base.OnModelCreating(modelBuilder);
    }
}
