using Microsoft.EntityFrameworkCore;
using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Data.EFCore.InMemory;

namespace TheAuctionHouse.Data.EFCore.InMemory
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        protected readonly IAppDbContext _context; // Changed type to IAppDbContext
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(IAppDbContext context) // Changed parameter type to IAppDbContext
        {
            _context = context;
            _dbSet = _context.GetDbSet<T>()!;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Attach(entity);
            (_context as DbContext)!.Entry(entity).State = EntityState.Modified;
            await Task.FromResult(0);
        }

        public virtual async Task DeleteAsync(int id)
        {
            T? entity = await _dbSet.FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException();
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await Task.FromResult(0);
            }
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate)
        {
            List<T>? Result = _dbSet.Where(predicate).ToList();
            return await Task.FromResult(Result) ?? throw new InvalidOperationException("Entity not found.");
        }    public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await Task.FromResult(0);
        }
    }
}