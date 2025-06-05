namespace TheAuctionHouse.Domain.DataContracts;

using System.Collections.Generic;
using TheAuctionHouse.Domain.Entities;

public interface IAssetRepository : IRepository<Asset>
{
    Task<List<Asset>> GetAssetsByUserIdAsync(int userId);
    Task AddAsync(Asset asset);
    Task UpdateAsync(Asset asset);
    Task DeleteAsync(Asset asset);
    Task<Asset?> GetByIdAsync(int id);
    Task<IEnumerable<Asset>> GetAllAsync();
    Task SaveChangesAsync();
}