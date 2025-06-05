using TheAuctionHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TheAuctionHouse.Data.EFCore.InMemory
{
    public interface IAppDbContext : IDisposable // Added IDisposable
    {
        DbSet<T>? GetDbSet<T>() where T : class;
        IQueryable<PortalUser> PortalUsers { get; }
        IQueryable<Asset> Assets { get; }
        IQueryable<Auction> Auctions { get; }
        IQueryable<BidHistory> BidHistories { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default); // Added SaveChangesAsync
    }
}