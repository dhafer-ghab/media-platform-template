using Microsoft.EntityFrameworkCore;

namespace Host.WebApi.ArtworkViews;

internal sealed class ArtworkViewsDbContext(DbContextOptions<ArtworkViewsDbContext> options)
    : DbContext(options)
{
    public DbSet<ArtworkView> ArtworkViews => Set<ArtworkView>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("analytics");
        modelBuilder.Entity<ArtworkView>(entity =>
        {
            entity.ToTable("artwork_view_counts", table =>
            {
                table.HasCheckConstraint(
                    "ck_artwork_view_counts_slug",
                    "char_length(artwork_slug) BETWEEN 1 AND 120 AND artwork_slug ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
                table.HasCheckConstraint("ck_artwork_view_counts_count", "view_count > 0");
            });
            entity.HasKey(view => view.ArtworkSlug);
            entity.Property(view => view.ArtworkSlug)
                .HasColumnName("artwork_slug")
                .HasMaxLength(120);
            entity.Property(view => view.ViewCount).HasColumnName("view_count");
            entity.Property(view => view.LastViewedAt).HasColumnName("last_viewed_at");
        });
    }
}
