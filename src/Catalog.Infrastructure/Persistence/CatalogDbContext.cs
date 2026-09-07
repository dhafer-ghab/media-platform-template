using Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

internal sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    internal const string Schema = "Catalog";

    public DbSet<Actor> Actors => Set<Actor>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<LegacyRoute> LegacyRoutes => Set<LegacyRoute>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.Entity<Actor>(entity =>
        {
            entity.ToTable("Actors", table =>
            {
                table.HasCheckConstraint("CK_Actors_Id", "\"Id\" <> '00000000-0000-0000-0000-000000000000'");
                table.HasCheckConstraint("CK_Actors_Slug", "\"Slug\" ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
                table.HasCheckConstraint("CK_Actors_RequiredText", "char_length(btrim(\"DisplayName\")) > 0 AND char_length(btrim(\"Profession\")) > 0 AND char_length(btrim(\"Bio\")) > 0 AND char_length(btrim(\"AvatarStorageKey\")) > 0");
                table.HasCheckConstraint("CK_Actors_AvatarStorageKey", StorageKeyConstraint("\"AvatarStorageKey\""));
                table.HasCheckConstraint("CK_Actors_Counts", "\"FollowerCount\" >= 0 AND \"EditorialRank\" >= 0");
                table.HasCheckConstraint("CK_Actors_Timestamps", "\"PublishedAt\" > '-infinity' AND \"UpdatedAt\" >= \"PublishedAt\"");
            });
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("UX_Actors_Slug");
            entity.HasIndex(x => new { x.EditorialRank, x.PublishedAt, x.Id })
                .IsDescending(true, true, false)
                .HasDatabaseName("IX_Actors_DiscoveryOrder");
            entity.HasIndex(x => x.DisplayName).HasMethod("gin").HasOperators("gin_trgm_ops")
                .HasDatabaseName("IX_Actors_DisplayName_Search");
            entity.Property(x => x.Slug).HasMaxLength(160).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Profession).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Bio).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.AvatarStorageKey).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<Album>(entity =>
        {
            entity.ToTable("Albums", table =>
            {
                table.HasCheckConstraint("CK_Albums_Ids", "\"Id\" <> '00000000-0000-0000-0000-000000000000' AND \"ActorId\" <> '00000000-0000-0000-0000-000000000000'");
                table.HasCheckConstraint("CK_Albums_Slug", "\"Slug\" ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
                table.HasCheckConstraint("CK_Albums_RequiredText", "char_length(btrim(\"Title\")) > 0 AND char_length(btrim(\"Description\")) > 0 AND char_length(btrim(\"CoverStorageKey\")) > 0 AND char_length(btrim(\"CoverAltText\")) > 0");
                table.HasCheckConstraint("CK_Albums_CoverStorageKey", StorageKeyConstraint("\"CoverStorageKey\""));
                table.HasCheckConstraint("CK_Albums_Values", "\"CoverAspectRatio\" > 0 AND \"CoverAspectRatio\" <> 'NaN'::double precision AND \"CoverAspectRatio\" <> 'Infinity'::double precision AND \"EditorialRank\" >= 0 AND cardinality(\"Tags\") <= 100");
                table.HasCheckConstraint("CK_Albums_Timestamps", "\"PublishedAt\" > '-infinity' AND \"UpdatedAt\" >= \"PublishedAt\"");
            });
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ActorId, x.Slug }).IsUnique()
                .HasDatabaseName("UX_Albums_ActorId_Slug");
            entity.HasIndex(x => x.Tags).HasMethod("gin").HasDatabaseName("IX_Albums_Tags");
            entity.HasIndex(x => x.Title).HasMethod("gin").HasOperators("gin_trgm_ops")
                .HasDatabaseName("IX_Albums_Title_Search");
            entity.HasIndex(x => x.Description).HasMethod("gin").HasOperators("gin_trgm_ops")
                .HasDatabaseName("IX_Albums_Description_Search");
            entity.HasIndex(x => new { x.ActorId, x.EditorialRank, x.PublishedAt, x.Id })
                .IsDescending(false, true, true, false)
                .HasDatabaseName("IX_Albums_Actor_DiscoveryOrder");
            entity.HasIndex(x => new { x.EditorialRank, x.PublishedAt, x.Id })
                .IsDescending(true, true, false)
                .HasDatabaseName("IX_Albums_DiscoveryOrder");
            entity.Property(x => x.Slug).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(250).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.CoverStorageKey).HasMaxLength(500).IsRequired();
            entity.Property(x => x.CoverAltText).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Tags).HasColumnType("text[]").IsRequired();
            entity.HasOne(x => x.Actor).WithMany().HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("Posts", table =>
            {
                table.HasCheckConstraint("CK_Posts_Ids", "\"Id\" <> '00000000-0000-0000-0000-000000000000' AND \"AlbumId\" <> '00000000-0000-0000-0000-000000000000'");
                table.HasCheckConstraint("CK_Posts_Slug", "\"Slug\" ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
                table.HasCheckConstraint("CK_Posts_RequiredText", "char_length(btrim(\"StorageKey\")) > 0 AND char_length(btrim(\"MimeType\")) > 0 AND char_length(btrim(\"AltText\")) > 0");
                table.HasCheckConstraint("CK_Posts_StorageKey", StorageKeyConstraint("\"StorageKey\""));
                table.HasCheckConstraint("CK_Posts_Values", "\"Width\" > 0 AND \"Height\" > 0 AND \"ByteSize\" > 0 AND \"DisplayOrder\" >= 0 AND \"EditorialRank\" >= 0 AND cardinality(\"Tags\") <= 100");
                table.HasCheckConstraint("CK_Posts_Media", "(\"MediaType\" = 'Photo' AND \"DurationSeconds\" IS NULL AND \"MimeType\" ILIKE 'image/%') OR (\"MediaType\" = 'Video' AND \"DurationSeconds\" > 0 AND \"MimeType\" ILIKE 'video/%')");
                table.HasCheckConstraint("CK_Posts_Timestamps", "\"PublishedAt\" > '-infinity' AND \"UpdatedAt\" >= \"PublishedAt\"");
            });
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("UX_Posts_Slug");
            entity.HasIndex(x => x.Tags).HasMethod("gin").HasDatabaseName("IX_Posts_Tags");
            entity.HasIndex(x => new { x.AlbumId, x.DisplayOrder })
                .IsUnique()
                .HasDatabaseName("UX_Posts_AlbumId_DisplayOrder");
            entity.Property(x => x.Slug).HasMaxLength(160).IsRequired();
            entity.Property(x => x.StorageKey).HasMaxLength(500).IsRequired();
            entity.Property(x => x.MediaType).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.MimeType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Caption).HasMaxLength(2000);
            entity.Property(x => x.AltText).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Tags).HasColumnType("text[]").IsRequired();
            entity.HasOne(x => x.Album).WithMany().HasForeignKey(x => x.AlbumId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LegacyRoute>(entity =>
        {
            entity.ToTable("LegacyRoutes", table =>
            {
                table.HasCheckConstraint("CK_LegacyRoutes_Id", "\"Id\" <> '00000000-0000-0000-0000-000000000000'");
                table.HasCheckConstraint("CK_LegacyRoutes_SourcePath", "\"SourcePath\" = lower(\"SourcePath\") AND (\"SourcePath\" = '/' OR \"SourcePath\" ~ '^/([^/?#%\\\\]+/)*[^/?#%\\\\]+$') AND NOT \"SourcePath\" ~ '(^|/)\\.{1,2}(/|$)' AND NOT \"SourcePath\" ~ '[[:cntrl:]]' AND \"SourcePath\" = btrim(\"SourcePath\")");
                table.HasCheckConstraint("CK_LegacyRoutes_DestinationPath", "(\"DestinationPath\" = '/' OR \"DestinationPath\" ~ '^/([^/?#%\\\\]+/)*[^/?#%\\\\]+$') AND NOT \"DestinationPath\" ~ '(^|/)\\.{1,2}(/|$)' AND NOT \"DestinationPath\" ~ '[[:cntrl:]]' AND \"DestinationPath\" = btrim(\"DestinationPath\")");
                table.HasCheckConstraint("CK_LegacyRoutes_Timestamps", "\"CreatedAt\" > '-infinity' AND \"UpdatedAt\" >= \"CreatedAt\"");
            });
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.SourcePath).IsUnique().HasDatabaseName("UX_LegacyRoutes_SourcePath");
            entity.Property(x => x.SourcePath).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.DestinationPath).HasMaxLength(1000).IsRequired();
            });
    }

    private static string StorageKeyConstraint(string column) =>
        $"{column} !~ '(^/|[\\\\%?#:]|[[:cntrl:]]|(^|/)\\.{{1,2}}(/|$)|//)'";
}
