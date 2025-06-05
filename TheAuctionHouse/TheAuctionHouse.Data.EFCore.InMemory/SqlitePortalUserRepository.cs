using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TheAuctionHouse.Data.EFCore.InMemory
{
    public class SqlitePortalUserRepository : IPortalUserRepository
    {
        private readonly DbSet<PortalUser> _users;
        private readonly SqliteDbContext _context;

        public SqlitePortalUserRepository(SqliteDbContext context)
        {
            _context = context;
            _users = context.Set<PortalUser>();
        }

        public async Task<IEnumerable<PortalUser>> GetAllAsync()
        {
            return await _users.ToListAsync();
        }

        public async Task<PortalUser?> GetByIdAsync(int id)
        {
            return await _users.FindAsync(id);
        }

        public async Task<PortalUser?> GetByEmailAsync(string email)
        {
            return await _users.FirstOrDefaultAsync(u => u.EmailId == email);
        }

        public async Task AddAsync(PortalUser entity)
        {
            await _users.AddAsync(entity);
        }

        public async Task UpdateAsync(PortalUser entity)
        {
            _users.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            await Task.CompletedTask;
        }        public async Task DeleteAsync(int id)
        {
            var entity = await _users.FindAsync(id);
            if (entity != null)
            {
                _users.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }public async Task DeleteAsync(PortalUser entity)
        {
            _users.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AnyAsync(Func<PortalUser, bool> predicate)
        {
            return await Task.FromResult(_users.Any(predicate));
        }

        public async Task<IEnumerable<PortalUser>> FindAsync(Func<PortalUser, bool> predicate)
        {
            return await Task.FromResult(_users.Where(predicate));
        }

        public async Task<PortalUser?> GetUserByUserIdAsync(int userId)
        {
            return await _users.FindAsync(userId);
        }

        public async Task<PortalUser?> GetUserByEmailAsync(string email)
        {
            return await _users.FirstOrDefaultAsync(u => u.EmailId == email);
        }

        public void DepositWalletBalance(int userId, int amount)
        {
            var user = _users.Find(userId);
            if (user != null)
            {
                user.WalletBalance += amount;
                _context.Entry(user).State = EntityState.Modified;
            }
        }        public void WithdrawWalletBalance(int userId, int amount)
        {
            var user = _users.Find(userId);
            if (user != null)
            {
                // Calculate available balance (total balance minus blocked amount)
                int availableBalance = user.WalletBalance - user.WalletBalanceBlocked;
                
                if (availableBalance >= amount)
                {
                    user.WalletBalance -= amount;
                    _context.Entry(user).State = EntityState.Modified;
                }
                else
                {
                    throw new InvalidOperationException($"Insufficient available balance. Available: ${availableBalance}, Requested: ${amount}");
                }
            }
        }
    }
}
