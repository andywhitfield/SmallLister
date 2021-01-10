using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SmallLister.Migrations
{
    public partial class ExternalApi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiClients",
                columns: table => new
                {
                    ApiClientId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    AppKey = table.Column<string>(type: "TEXT", nullable: false),
                    AppSecretSalt = table.Column<string>(type: "TEXT", nullable: false),
                    AppSecretHash = table.Column<string>(type: "TEXT", nullable: false),
                    RedirectUri = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedById = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdateDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedDateTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiClients", x => x.ApiClientId);
                    table.ForeignKey(
                        name: "FK_ApiClients_UserAccounts_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "UserAccounts",
                        principalColumn: "UserAccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAccountApiAccesses",
                columns: table => new
                {
                    UserAccountApiAccessId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApiClientId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    RefreshToken = table.Column<string>(type: "TEXT", nullable: false),
                    RevokedDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdateDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedDateTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccountApiAccesses", x => x.UserAccountApiAccessId);
                    table.ForeignKey(
                        name: "FK_UserAccountApiAccesses_ApiClients_ApiClientId",
                        column: x => x.ApiClientId,
                        principalTable: "ApiClients",
                        principalColumn: "ApiClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAccountApiAccesses_UserAccounts_UserAccountId",
                        column: x => x.UserAccountId,
                        principalTable: "UserAccounts",
                        principalColumn: "UserAccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAccountTokens",
                columns: table => new
                {
                    UserAccountTokenId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserAccountApiAccessId = table.Column<int>(type: "INTEGER", nullable: false),
                    TokenData = table.Column<string>(type: "TEXT", nullable: false),
                    ExpiryDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdateDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedDateTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccountTokens", x => x.UserAccountTokenId);
                    table.ForeignKey(
                        name: "FK_UserAccountTokens_UserAccountApiAccesses_UserAccountApiAccessId",
                        column: x => x.UserAccountApiAccessId,
                        principalTable: "UserAccountApiAccesses",
                        principalColumn: "UserAccountApiAccessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiClients_CreatedById",
                table: "ApiClients",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccountApiAccesses_ApiClientId",
                table: "UserAccountApiAccesses",
                column: "ApiClientId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccountApiAccesses_UserAccountId",
                table: "UserAccountApiAccesses",
                column: "UserAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccountTokens_UserAccountApiAccessId",
                table: "UserAccountTokens",
                column: "UserAccountApiAccessId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAccountTokens");

            migrationBuilder.DropTable(
                name: "UserAccountApiAccesses");

            migrationBuilder.DropTable(
                name: "ApiClients");
        }
    }
}
