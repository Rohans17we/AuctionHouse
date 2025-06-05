using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Domain.ServiceContracts;

namespace TheAuctionHouse.Domain.Services;

public class DashboardService : IDashboardService
{
    private readonly IAppUnitOfWork _appUnitOfWork;

    public DashboardService(IAppUnitOfWork appUnitOfWork)
    {
        _appUnitOfWork = appUnitOfWork;
    }

    public Result<DashboardResponse> GetDashboardDataAsync(int userId)
    {
        try
        {
            // Get live auctions sorted by expiry
            var liveAuctionsResult = GetLiveAuctionsSortedByExpiryAsync();
            if (!liveAuctionsResult.IsSuccess)
                return liveAuctionsResult.Error;

            // Get user's bid history
            var userBidHistory = _appUnitOfWork.AuctionRepository.GetBidHistoriesByUserIdAsync(userId).Result;
            var userBids = userBidHistory.Select(bid => {
                var bidder = _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(bid.BidderId).Result;
                
                return new BidHistoryResponse
                {
                    BidId = bid.Id,
                    AuctionId = bid.AuctionId,
                    UserId = bid.BidderId,
                    BidAmount = bid.BidAmount,
                    BidTime = bid.BidDate,
                    UserName = bidder?.Name ?? ""
                };
            }).ToList();

            // Get auctions where user has the highest bid
            var userHighestBidsResult = GetUserHighestBidsAsync(userId);
            if (!userHighestBidsResult.IsSuccess)
                return userHighestBidsResult.Error;            var response = new DashboardResponse
            {
                LiveAuctions = liveAuctionsResult.Value ?? new List<AuctionResponse>(),
                UserBids = userBids,
                UserHighestBids = userHighestBidsResult.Value ?? new List<AuctionResponse>()
            };

            return response;
        }
        catch (Exception ex)
        {
            return Error.InternalServerError(ex.Message);
        }
    }

    public Result<List<AuctionResponse>> GetLiveAuctionsSortedByExpiryAsync()
    {
        try
        {
            var allLiveAuctions = _appUnitOfWork.AuctionRepository.GetAllAsync().Result
                .Where(a => a.Status == AuctionStatus.Live && !a.IsExpired())
                .OrderBy(a => a.GetRemainingTimeInMinutes())
                .ToList();

            var response = allLiveAuctions.Select(auction => {
                var asset = _appUnitOfWork.AssetRepository.GetByIdAsync(auction.AssetId).Result;
                var highestBidder = auction.CurrentHighestBidderId > 0 
                    ? _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(auction.CurrentHighestBidderId).Result 
                    : null;

                return new AuctionResponse
                {
                    AuctionId = auction.Id,
                    UserId = auction.UserId,
                    AssetId = auction.AssetId,
                    AssetTitle = asset?.Title ?? "",
                    AssetDescription = asset?.Description ?? "",
                    RetailValue = asset?.RetailValue ?? 0,
                    CurrentHighestBid = auction.CurrentHighestBid,
                    CurrentHighestBidderId = auction.CurrentHighestBidderId,
                    HighestBidderName = highestBidder?.Name ?? "",
                    MinimumBidIncrement = auction.MinimumBidIncrement,
                    StartDate = auction.StartDate,
                    TotalMinutesToExpiry = auction.TotalMinutesToExpiry,
                    Status = auction.Status.ToString()
                };
            }).ToList();

            return response;
        }
        catch (Exception ex)
        {
            return Error.InternalServerError(ex.Message);
        }
    }

    public Result<List<AuctionResponse>> GetUserHighestBidsAsync(int userId)
    {
        try
        {
            var userHighestBidAuctions = _appUnitOfWork.AuctionRepository.GetAllAsync().Result
                .Where(a => a.Status == AuctionStatus.Live && !a.IsExpired() && a.CurrentHighestBidderId == userId)
                .OrderBy(a => a.GetRemainingTimeInMinutes())
                .ToList();

            var response = userHighestBidAuctions.Select(auction => {
                var asset = _appUnitOfWork.AssetRepository.GetByIdAsync(auction.AssetId).Result;
                var highestBidder = _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(auction.CurrentHighestBidderId).Result;

                return new AuctionResponse
                {
                    AuctionId = auction.Id,
                    UserId = auction.UserId,
                    AssetId = auction.AssetId,
                    AssetTitle = asset?.Title ?? "",
                    AssetDescription = asset?.Description ?? "",
                    CurrentHighestBid = auction.CurrentHighestBid,
                    CurrentHighestBidderId = auction.CurrentHighestBidderId,
                    HighestBidderName = highestBidder?.Name ?? "",
                    MinimumBidIncrement = auction.MinimumBidIncrement,
                    StartDate = auction.StartDate,
                    TotalMinutesToExpiry = auction.TotalMinutesToExpiry,
                    Status = auction.Status.ToString()
                };
            }).ToList();

            return response;
        }
        catch (Exception ex)
        {
            return Error.InternalServerError(ex.Message);
        }
    }
}
