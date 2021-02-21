using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SmallLister.Migrations
{
    public partial class AddUserItemPostponedUntilDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PostponedUntilDate",
                table: "UserItems",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostponedUntilDate",
                table: "UserItems");
        }
    }
}
