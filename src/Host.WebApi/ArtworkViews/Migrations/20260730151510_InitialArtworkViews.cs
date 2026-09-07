using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Host.WebApi.ArtworkViews.Migrations
{
    /// <inheritdoc />
    public partial class InitialArtworkViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "analytics");

            migrationBuilder.CreateTable(
                name: "artwork_view_counts",
                schema: "analytics",
                columns: table => new
                {
                    artwork_slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    view_count = table.Column<long>(type: "bigint", nullable: false),
                    last_viewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_artwork_view_counts", x => x.artwork_slug);
                    table.CheckConstraint("ck_artwork_view_counts_count", "view_count > 0");
                    table.CheckConstraint("ck_artwork_view_counts_slug", "char_length(artwork_slug) BETWEEN 1 AND 120 AND artwork_slug ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "artwork_view_counts",
                schema: "analytics");
        }
    }
}
