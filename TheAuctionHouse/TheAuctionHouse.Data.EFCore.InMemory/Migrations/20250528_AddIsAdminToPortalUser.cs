using Microsoft.EntityFrameworkCore.Migrations;

namespace TheAuctionHouse.Data.EFCore.InMemory.Migrations
{
    public partial class AddIsAdminToPortalUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "PortalUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "PortalUsers");
        }
    }
}
