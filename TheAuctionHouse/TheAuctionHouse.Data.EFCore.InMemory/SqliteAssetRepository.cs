using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TheAuctionHouse.Data.EFCore.InMemory
{
    public class SqliteAssetRepository : IAssetRepository
    {
        private readonly SqliteDbContext _context;
        private readonly DbSet<Asset> _assets;

        public SqliteAssetRepository(SqliteDbContext context)
        {
            _context = context;
            _assets = context.Assets;
        }

        public async Task<IEnumerable<Asset>> GetAllAsync()
        {
            return await _assets.ToListAsync();
        }

        public async Task<Asset?> GetByIdAsync(int id)
        {
            return await _assets.FindAsync(id);
        }        public async Task AddAsync(Asset entity)
        {
            await _assets.AddAsync(entity);
        }        public async Task UpdateAsync(Asset entity)
        {
            _assets.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _assets.FindAsync(id);
            if (entity != null)
            {
                _assets.Remove(entity);
            }
        }

        public Task DeleteAsync(Asset entity)
        {
            _assets.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<Asset>> FindAsync(Func<Asset, bool> predicate)
        {
            return await Task.FromResult(_assets.Where(predicate));
        }        public async Task<List<Asset>> GetAssetsByUserIdAsync(int userId)
        {
            return await _assets.Where(a => a.UserId == userId).ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
