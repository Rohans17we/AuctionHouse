using Microsoft.EntityFrameworkCore;
using TheAuctionHouse.Common;
using TheAuctionHouse.Domain.Entities;

namespace TheAuctionHouse.Data.EFCore.InMemory;

public static class DatabaseSeeder
{
    public static void SeedDatabase(SqliteDbContext context)
    {
        // Ensure database is created and migrations are applied
        context.Database.Migrate();

        // Add admin user if it doesn't exist
        if (!context.PortalUsers.Any(u => u.EmailId == "admin@theauctionhouse.com"))
        {
            var adminUser = new PortalUser
            {
                Name = "Admin User",
                EmailId = "admin@theauctionhouse.com",
                HashedPassword = PasswordHelper.HashPassword("Admin@123"),
                WalletBalance = 1000000, // Give admin a large initial balance
                WalletBalanceBlocked = 0,
                IsAdmin = true
            };            context.PortalUsers.Add(adminUser);
        }

        context.SaveChanges();
    }
}
