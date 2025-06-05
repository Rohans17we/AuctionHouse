using TheAuctionHouse.Domain.DataContracts;

namespace TheAuctionHouse.Data.EFCore.InMemory
{
    public class SqliteUnitOfWork : IAppUnitOfWork, IDisposable
    {
        private readonly SqliteDbContext _context;
        private bool _disposed;        public SqliteUnitOfWork(SqliteDbContext context)
        {
            _context = context;
            PortalUserRepository = new SqlitePortalUserRepository(_context);
            AssetRepository = new SqliteAssetRepository(_context);
            AuctionRepository = new SqliteAuctionRepository(_context);
        }

        public IPortalUserRepository PortalUserRepository { get; private set; }
        public IAssetRepository AssetRepository { get; }
        public IAuctionRepository AuctionRepository { get; }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
