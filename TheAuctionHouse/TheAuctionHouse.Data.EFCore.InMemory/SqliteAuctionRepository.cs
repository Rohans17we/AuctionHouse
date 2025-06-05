using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TheAuctionHouse.Data.EFCore.InMemory
{
    public class SqliteAuctionRepository : IAuctionRepository
    {
        private readonly SqliteDbContext _context;
        private readonly DbSet<Auction> _auctions;
        private readonly DbSet<BidHistory> _bidHistories;

        public SqliteAuctionRepository(SqliteDbContext context)
        {
            _context = context;
            _auctions = context.Auctions;
            _bidHistories = context.BidHistories;
        }

        public async Task<IEnumerable<Auction>> GetAllAsync()
        {
            return await _auctions.ToListAsync();
        }

        public async Task<Auction?> GetByIdAsync(int id)
        {
            return await _auctions.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(Auction entity)
        {
            await _auctions.AddAsync(entity);
        }

        public async Task UpdateAsync(Auction entity)
        {
            _auctions.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _auctions.FindAsync(id);
            if (entity != null)
            {
                _auctions.Remove(entity);
            }
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Auction entity)
        {
            _auctions.Remove(entity);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<Auction>> FindAsync(Func<Auction, bool> predicate)
        {
            return await Task.FromResult(_auctions.Where(predicate));
        }

        public async Task<List<Auction>> GetAuctionsByUserIdAsync(int userId)
        {            return await _auctions.Where(a => a.UserId == userId).ToListAsync();
        }

        public async Task<List<BidHistory>> GetBidHistoriesByUserIdAsync(int userId)
        {
            return await _bidHistories
                .Where(b => b.BidderId == userId)
                .ToListAsync();
        }

        public async Task<List<BidHistory>> GetBidHistoriesByAuctionIdAsync(int auctionId)
        {
            return await _bidHistories
                .Where(b => b.AuctionId == auctionId)
                .ToListAsync();
        }

        public async Task<BidHistory?> GetHighestBidForAuctionAsync(int auctionId)
        {
            return await _bidHistories
                .Where(b => b.AuctionId == auctionId)
                .OrderByDescending(b => b.BidAmount)
                .FirstOrDefaultAsync();
        }
    }
}
