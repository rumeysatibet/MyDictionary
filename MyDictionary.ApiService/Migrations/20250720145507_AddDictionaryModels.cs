using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyDictionary.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddDictionaryModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Entries_CreatedByUserId",
                table: "Entries");

            migrationBuilder.DropIndex(
                name: "IX_Entries_TopicId",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "DownVotes",
                table: "Entries");

            migrationBuilder.RenameColumn(
                name: "UpVotes",
                table: "Entries",
                newName: "FavoriteCount");

            migrationBuilder.AddColumn<int>(
                name: "TopicCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Topics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EntryCount",
                table: "Topics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEntryAt",
                table: "Topics",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Topics",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Topics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ContentHtml",
                table: "Entries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsEdited",
                table: "Entries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Entries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntryFavorites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EntryId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryFavorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntryFavorites_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntryFavorites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntryLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntryId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntryLinks_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Topics_CategoryId_LastEntryAt",
                table: "Topics",
                columns: new[] { "CategoryId", "LastEntryAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Topics_Slug",
                table: "Topics",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entries_CreatedByUserId_CreatedAt",
                table: "Entries",
                columns: new[] { "CreatedByUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Entries_TopicId_CreatedAt",
                table: "Entries",
                columns: new[] { "TopicId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntryFavorites_EntryId",
                table: "EntryFavorites",
                column: "EntryId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryFavorites_UserId_CreatedAt",
                table: "EntryFavorites",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EntryFavorites_UserId_EntryId",
                table: "EntryFavorites",
                columns: new[] { "UserId", "EntryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntryLinks_EntryId",
                table: "EntryLinks",
                column: "EntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_Categories_CategoryId",
                table: "Topics",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Topics_Categories_CategoryId",
                table: "Topics");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "EntryFavorites");

            migrationBuilder.DropTable(
                name: "EntryLinks");

            migrationBuilder.DropIndex(
                name: "IX_Topics_CategoryId_LastEntryAt",
                table: "Topics");

            migrationBuilder.DropIndex(
                name: "IX_Topics_Slug",
                table: "Topics");

            migrationBuilder.DropIndex(
                name: "IX_Entries_CreatedByUserId_CreatedAt",
                table: "Entries");

            migrationBuilder.DropIndex(
                name: "IX_Entries_TopicId_CreatedAt",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "TopicCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "EntryCount",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "LastEntryAt",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "ContentHtml",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "IsEdited",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Entries");

            migrationBuilder.RenameColumn(
                name: "FavoriteCount",
                table: "Entries",
                newName: "UpVotes");

            migrationBuilder.AddColumn<int>(
                name: "DownVotes",
                table: "Entries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Entries_CreatedByUserId",
                table: "Entries",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_TopicId",
                table: "Entries",
                column: "TopicId");
        }
    }
}
