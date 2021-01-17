using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SmallLister.Migrations
{
    public partial class AddUserFeeds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFeeds",
                columns: table => new
                {
                    UserFeedId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserFeedIdentifier = table.Column<string>(type: "TEXT", nullable: false),
                    UserAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    FeedType = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemDisplay = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdateDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedDateTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFeeds", x => x.UserFeedId);
                    table.ForeignKey(
                        name: "FK_UserFeeds_UserAccounts_UserAccountId",
                        column: x => x.UserAccountId,
                        principalTable: "UserAccounts",
                        principalColumn: "UserAccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFeedItems",
                columns: table => new
                {
                    UserFeedItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserFeedId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdateDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedDateTime = table.Column<DateTime>(type: "TEXT", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_UserFeeds_UserAccountId",
                table: "UserFeeds",
                column: "UserAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFeedItems");

            migrationBuilder.DropTable(
                name: "UserFeeds");
        }
    }
}
