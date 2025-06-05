using Microsoft.EntityFrameworkCore;
using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Domain.DataContracts;

namespace TheAuctionHouse.Data.EFCore.InMemory
{    public class SqliteDbContext : DbContext, IAppDbContext
    {
        public SqliteDbContext(DbContextOptions<SqliteDbContext> options)
            : base(options)
        {
        }

        public DbSet<PortalUser> PortalUsers { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<BidHistory> BidHistories { get; set; }        IQueryable<PortalUser> IAppDbContext.PortalUsers => PortalUsers;
        IQueryable<Asset> IAppDbContext.Assets => Assets;
        IQueryable<Auction> IAppDbContext.Auctions => Auctions;
        IQueryable<BidHistory> IAppDbContext.BidHistories => BidHistories;

        public DbSet<T>? GetDbSet<T>() where T : class
        {
            if (typeof(T) == typeof(PortalUser))
                return PortalUsers as DbSet<T>;
            if (typeof(T) == typeof(Asset))
                return Assets as DbSet<T>;
            if (typeof(T) == typeof(Auction))
                return Auctions as DbSet<T>;
            if (typeof(T) == typeof(BidHistory))
                return BidHistories as DbSet<T>;

            throw new ArgumentException("Invalid type");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PortalUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.EmailId).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EmailId).IsRequired().HasMaxLength(255);
                entity.Property(e => e.HashedPassword).IsRequired().HasMaxLength(255);
            });

            modelBuilder.Entity<Asset>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
            });

            modelBuilder.Entity<Auction>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<BidHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }
    }
}
