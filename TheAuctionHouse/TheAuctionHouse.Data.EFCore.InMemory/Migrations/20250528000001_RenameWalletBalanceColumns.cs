using Microsoft.EntityFrameworkCore.Migrations;

namespace TheAuctionHouse.Data.EFCore.InMemory.Migrations
{
    public partial class RenameWalletBalanceColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WalletBalenceBlocked",
                table: "PortalUsers",
                newName: "WalletBalanceBlocked");

            migrationBuilder.RenameColumn(
                name: "WalletBalence",
                table: "PortalUsers",
                newName: "WalletBalance");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WalletBalanceBlocked",
                table: "PortalUsers",
                newName: "WalletBalenceBlocked");

            migrationBuilder.RenameColumn(
                name: "WalletBalance",
                table: "PortalUsers",
                newName: "WalletBalence");
        }
    }
}
