using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SmallLister.Migrations
{
    public partial class ListAndItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserLists",
                columns: table => new
                {
                    UserListId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdateDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedDateTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLists", x => x.UserListId);
                    table.ForeignKey(
                        name: "FK_UserLists_UserAccounts_UserAccountId",
                        column: x => x.UserAccountId,
                        principalTable: "UserAccounts",
                        principalColumn: "UserAccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserItems",
                columns: table => new
                {
                    UserItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserListId = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    NextDueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Repeat = table.Column<int>(type: "INTEGER", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdateDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedDateTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserItems", x => x.UserItemId);
                    table.ForeignKey(
                        name: "FK_UserItems_UserAccounts_UserAccountId",
                        column: x => x.UserAccountId,
                        principalTable: "UserAccounts",
                        principalColumn: "UserAccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserItems_UserLists_UserListId",
                        column: x => x.UserListId,
                        principalTable: "UserLists",
                        principalColumn: "UserListId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserItems_UserAccountId",
                table: "UserItems",
                column: "UserAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_UserItems_UserListId",
                table: "UserItems",
                column: "UserListId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLists_UserAccountId",
                table: "UserLists",
                column: "UserAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserItems");

            migrationBuilder.DropTable(
                name: "UserLists");
        }
    }
}
