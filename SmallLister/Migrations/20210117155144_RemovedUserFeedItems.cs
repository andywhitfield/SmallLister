using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SmallLister.Migrations
{
    public partial class RemovedUserFeedItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFeedItems");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFeedItems",
                columns: table => new
                {
                    UserFeedItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdateDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UserFeedId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserItemId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFeedItems", x => x.UserFeedItemId);
                    table.ForeignKey(
                        name: "FK_UserFeedItems_UserFeeds_UserFeedId",
                        column: x => x.UserFeedId,
                        principalTable: "UserFeeds",
                        principalColumn: "UserFeedId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFeedItems_UserItems_UserItemId",
                        column: x => x.UserItemId,
                        principalTable: "UserItems",
                        principalColumn: "UserItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFeedItems_UserFeedId",
                table: "UserFeedItems",
                column: "UserFeedId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFeedItems_UserItemId",
                table: "UserFeedItems",
                column: "UserItemId");
        }
    }
}
