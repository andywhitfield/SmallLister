using Microsoft.EntityFrameworkCore.Migrations;

namespace SmallLister.Migrations
{
    public partial class LastSelectedUserList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastSelectedUserListId",
                table: "UserAccounts",
                type: "INTEGER",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSelectedUserListId",
                table: "UserAccounts");
        }
    }
}
