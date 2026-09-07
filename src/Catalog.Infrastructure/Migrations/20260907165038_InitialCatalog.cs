using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Catalog");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateTable(
                name: "Actors",
                schema: "Catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Profession = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Bio = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    AvatarStorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FollowerCount = table.Column<long>(type: "bigint", nullable: false),
                    EditorialRank = table.Column<int>(type: "integer", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actors", x => x.Id);
                    table.CheckConstraint("CK_Actors_AvatarStorageKey", "\"AvatarStorageKey\" !~ '(^/|[\\\\%?#:]|[[:cntrl:]]|(^|/)\\.{1,2}(/|$)|//)'");
                    table.CheckConstraint("CK_Actors_Counts", "\"FollowerCount\" >= 0 AND \"EditorialRank\" >= 0");
                    table.CheckConstraint("CK_Actors_Id", "\"Id\" <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_Actors_RequiredText", "char_length(btrim(\"DisplayName\")) > 0 AND char_length(btrim(\"Profession\")) > 0 AND char_length(btrim(\"Bio\")) > 0 AND char_length(btrim(\"AvatarStorageKey\")) > 0");
                    table.CheckConstraint("CK_Actors_Slug", "\"Slug\" ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
                    table.CheckConstraint("CK_Actors_Timestamps", "\"PublishedAt\" > '-infinity' AND \"UpdatedAt\" >= \"PublishedAt\"");
                });

            migrationBuilder.CreateTable(
                name: "LegacyRoutes",
                schema: "Catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourcePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DestinationPath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegacyRoutes", x => x.Id);
                    table.CheckConstraint("CK_LegacyRoutes_DestinationPath", "(\"DestinationPath\" = '/' OR \"DestinationPath\" ~ '^/([^/?#%\\\\]+/)*[^/?#%\\\\]+$') AND NOT \"DestinationPath\" ~ '(^|/)\\.{1,2}(/|$)' AND NOT \"DestinationPath\" ~ '[[:cntrl:]]' AND \"DestinationPath\" = btrim(\"DestinationPath\")");
                    table.CheckConstraint("CK_LegacyRoutes_Id", "\"Id\" <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_LegacyRoutes_SourcePath", "\"SourcePath\" = lower(\"SourcePath\") AND (\"SourcePath\" = '/' OR \"SourcePath\" ~ '^/([^/?#%\\\\]+/)*[^/?#%\\\\]+$') AND NOT \"SourcePath\" ~ '(^|/)\\.{1,2}(/|$)' AND NOT \"SourcePath\" ~ '[[:cntrl:]]' AND \"SourcePath\" = btrim(\"SourcePath\")");
                    table.CheckConstraint("CK_LegacyRoutes_Timestamps", "\"CreatedAt\" > '-infinity' AND \"UpdatedAt\" >= \"CreatedAt\"");
                });

            migrationBuilder.CreateTable(
                name: "Albums",
                schema: "Catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CoverStorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CoverAltText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CoverAspectRatio = table.Column<double>(type: "double precision", nullable: false),
                    IsSeries = table.Column<bool>(type: "boolean", nullable: false),
                    Tags = table.Column<string[]>(type: "text[]", nullable: false),
                    EditorialRank = table.Column<int>(type: "integer", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.Id);
                    table.CheckConstraint("CK_Albums_CoverStorageKey", "\"CoverStorageKey\" !~ '(^/|[\\\\%?#:]|[[:cntrl:]]|(^|/)\\.{1,2}(/|$)|//)'");
                    table.CheckConstraint("CK_Albums_Ids", "\"Id\" <> '00000000-0000-0000-0000-000000000000' AND \"ActorId\" <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_Albums_RequiredText", "char_length(btrim(\"Title\")) > 0 AND char_length(btrim(\"Description\")) > 0 AND char_length(btrim(\"CoverStorageKey\")) > 0 AND char_length(btrim(\"CoverAltText\")) > 0");
                    table.CheckConstraint("CK_Albums_Slug", "\"Slug\" ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
                    table.CheckConstraint("CK_Albums_Timestamps", "\"PublishedAt\" > '-infinity' AND \"UpdatedAt\" >= \"PublishedAt\"");
                    table.CheckConstraint("CK_Albums_Values", "\"CoverAspectRatio\" > 0 AND \"CoverAspectRatio\" <> 'NaN'::double precision AND \"CoverAspectRatio\" <> 'Infinity'::double precision AND \"EditorialRank\" >= 0 AND cardinality(\"Tags\") <= 100");
                    table.ForeignKey(
                        name: "FK_Albums_Actors_ActorId",
                        column: x => x.ActorId,
                        principalSchema: "Catalog",
                        principalTable: "Actors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                schema: "Catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlbumId = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MediaType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ByteSize = table.Column<long>(type: "bigint", nullable: false),
                    Caption = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AltText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Tags = table.Column<string[]>(type: "text[]", nullable: false),
                    EditorialRank = table.Column<int>(type: "integer", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                    table.CheckConstraint("CK_Posts_Ids", "\"Id\" <> '00000000-0000-0000-0000-000000000000' AND \"AlbumId\" <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_Posts_Media", "(\"MediaType\" = 'Photo' AND \"DurationSeconds\" IS NULL AND \"MimeType\" ILIKE 'image/%') OR (\"MediaType\" = 'Video' AND \"DurationSeconds\" > 0 AND \"MimeType\" ILIKE 'video/%')");
                    table.CheckConstraint("CK_Posts_RequiredText", "char_length(btrim(\"StorageKey\")) > 0 AND char_length(btrim(\"MimeType\")) > 0 AND char_length(btrim(\"AltText\")) > 0");
                    table.CheckConstraint("CK_Posts_Slug", "\"Slug\" ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
                    table.CheckConstraint("CK_Posts_StorageKey", "\"StorageKey\" !~ '(^/|[\\\\%?#:]|[[:cntrl:]]|(^|/)\\.{1,2}(/|$)|//)'");
                    table.CheckConstraint("CK_Posts_Timestamps", "\"PublishedAt\" > '-infinity' AND \"UpdatedAt\" >= \"PublishedAt\"");
                    table.CheckConstraint("CK_Posts_Values", "\"Width\" > 0 AND \"Height\" > 0 AND \"ByteSize\" > 0 AND \"DisplayOrder\" >= 0 AND \"EditorialRank\" >= 0 AND cardinality(\"Tags\") <= 100");
                    table.ForeignKey(
                        name: "FK_Posts_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalSchema: "Catalog",
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Actors_DiscoveryOrder",
                schema: "Catalog",
                table: "Actors",
                columns: new[] { "EditorialRank", "PublishedAt", "Id" },
                descending: new[] { true, true, false });

            migrationBuilder.CreateIndex(
                name: "IX_Actors_DisplayName_Search",
                schema: "Catalog",
                table: "Actors",
                column: "DisplayName")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "UX_Actors_Slug",
                schema: "Catalog",
                table: "Actors",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Albums_Actor_DiscoveryOrder",
                schema: "Catalog",
                table: "Albums",
                columns: new[] { "ActorId", "EditorialRank", "PublishedAt", "Id" },
                descending: new[] { false, true, true, false });

            migrationBuilder.CreateIndex(
                name: "IX_Albums_Description_Search",
                schema: "Catalog",
                table: "Albums",
                column: "Description")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Albums_DiscoveryOrder",
                schema: "Catalog",
                table: "Albums",
                columns: new[] { "EditorialRank", "PublishedAt", "Id" },
                descending: new[] { true, true, false });

            migrationBuilder.CreateIndex(
                name: "IX_Albums_Tags",
                schema: "Catalog",
                table: "Albums",
                column: "Tags")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_Title_Search",
                schema: "Catalog",
                table: "Albums",
                column: "Title")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "UX_Albums_ActorId_Slug",
                schema: "Catalog",
                table: "Albums",
                columns: new[] { "ActorId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_LegacyRoutes_SourcePath",
                schema: "Catalog",
                table: "LegacyRoutes",
                column: "SourcePath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_Tags",
                schema: "Catalog",
                table: "Posts",
                column: "Tags")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "UX_Posts_AlbumId_DisplayOrder",
                schema: "Catalog",
                table: "Posts",
                columns: new[] { "AlbumId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Posts_Slug",
                schema: "Catalog",
                table: "Posts",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LegacyRoutes",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "Posts",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "Albums",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "Actors",
                schema: "Catalog");
        }
    }
}
