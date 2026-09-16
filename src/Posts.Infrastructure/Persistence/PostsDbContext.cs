using Microsoft.EntityFrameworkCore;
using Posts.Domain;

namespace Posts.Infrastructure.Persistence;

internal sealed class PostsDbContext(DbContextOptions<PostsDbContext> options) : DbContext(options)
{
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Posts");

        modelBuilder.Entity<Post>(post =>
        {
            post.HasKey(value => value.Id);
            post.Property(value => value.AuthorId).IsRequired();
            post.Property(value => value.MediaAssetId).IsRequired();
            post.Property(value => value.Title).HasMaxLength(200).IsRequired();
            post.Property(value => value.Description).HasMaxLength(2_000).IsRequired();
            post.Property(value => value.Category).HasMaxLength(100).IsRequired();
            post.Property(value => value.NormalizedCategory).HasMaxLength(100).IsRequired();
            post.Property(value => value.MediaUrl).HasMaxLength(2_048).IsRequired();
            post.Property(value => value.AltText).HasMaxLength(500).IsRequired();
            post.Property(value => value.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            post.Property(value => value.ViewCount).HasDefaultValue(0L).IsRequired();
            post.HasIndex(value => new { value.Status, value.PublishedAt });
            post.HasIndex(value => new { value.Status, value.ViewCount });
            post.HasIndex(value => new { value.Status, value.NormalizedCategory });
            post.HasIndex(value => value.AuthorId);
            post.HasIndex(value => value.MediaAssetId);

            post.HasMany(value => value.Tags)
                .WithMany(value => value.Posts)
                .UsingEntity<Dictionary<string, object>>(
                    "PostTag",
                    right => right.HasOne<Tag>().WithMany().HasForeignKey("TagId").OnDelete(DeleteBehavior.Cascade),
                    left => left.HasOne<Post>().WithMany().HasForeignKey("PostId").OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.ToTable("PostTags", "Posts");
                        join.HasKey("PostId", "TagId");
                    });

            post.Navigation(value => value.Tags).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Tag>(tag =>
        {
            tag.HasKey(value => value.Id);
            tag.Property(value => value.Name).HasMaxLength(50).IsRequired();
            tag.Property(value => value.NormalizedName).HasMaxLength(50).IsRequired();
            tag.HasIndex(value => value.NormalizedName).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}
