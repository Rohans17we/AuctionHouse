using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheAuctionHouse.Data.EFCore.InMemory.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabaseModel : Migration
    {
        /// <inheritdoc />
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

        /// <inheritdoc />
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
