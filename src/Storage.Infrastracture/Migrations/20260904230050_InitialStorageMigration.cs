using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Storage.Infrastracture.Migrations
{
    /// <inheritdoc />
    public partial class InitialStorageMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Storage");

            migrationBuilder.CreateTable(
                name: "MediaAssets",
                schema: "Storage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    StorageProvider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    BucketName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAssets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_CreatedDate",
                schema: "Storage",
                table: "MediaAssets",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_StorageKey",
                schema: "Storage",
                table: "MediaAssets",
                column: "StorageKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaAssets",
                schema: "Storage");
        }
    }
}
