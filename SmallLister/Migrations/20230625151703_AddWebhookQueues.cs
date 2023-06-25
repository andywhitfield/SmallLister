using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmallLister.Migrations
{
    /// <inheritdoc />
    public partial class AddWebhookQueues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserItemWebhookQueue",
                columns: table => new
                {
                    UserItemWebhookQueueId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    EventType = table.Column<int>(type: "INTEGER", nullable: false),
                    SentPayload = table.Column<string>(type: "TEXT", nullable: true),
                    SentDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdateDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedDateTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserItemWebhookQueue", x => x.UserItemWebhookQueueId);
                    table.ForeignKey(
                        name: "FK_UserItemWebhookQueue_UserItems_UserItemId",
                        column: x => x.UserItemId,
                        principalTable: "UserItems",
                        principalColumn: "UserItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserListWebhookQueue",
                columns: table => new
                {
                    UserListWebhookQueueId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserListId = table.Column<int>(type: "INTEGER", nullable: false),
                    EventType = table.Column<int>(type: "INTEGER", nullable: false),
                    SentPayload = table.Column<string>(type: "TEXT", nullable: true),
                    SentDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdateDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedDateTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserListWebhookQueue", x => x.UserListWebhookQueueId);
                    table.ForeignKey(
                        name: "FK_UserListWebhookQueue_UserLists_UserListId",
                        column: x => x.UserListId,
                        principalTable: "UserLists",
                        principalColumn: "UserListId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserItemWebhookQueue_UserItemId",
                table: "UserItemWebhookQueue",
                column: "UserItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserListWebhookQueue_UserListId",
                table: "UserListWebhookQueue",
                column: "UserListId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserItemWebhookQueue");

            migrationBuilder.DropTable(
                name: "UserListWebhookQueue");
        }
    }
}
