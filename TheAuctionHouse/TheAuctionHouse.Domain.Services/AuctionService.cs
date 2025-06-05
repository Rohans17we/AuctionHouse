using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Common.Validation;
using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Domain.ServiceContracts;

namespace TheAuctionHouse.Domain.Services;

public class AuctionService :  IAuctionService
{    private readonly IAppUnitOfWork _appUnitOfWork;
    
    public AuctionService(IAppUnitOfWork appUnitOfWork)
    {
        _appUnitOfWork = appUnitOfWork;
    }    public Result<bool> PostAuctionAsync(PostAuctionRequest postAuctionRequest)
    {
        try
        {
            // Validate the request
            Error validationError = Error.ValidationFailures();
            if (!ValidationHelper.Validate(postAuctionRequest, validationError))
            {
                return validationError;
            }

            // Check if asset exists and is owned by the user FIRST
            var asset = _appUnitOfWork.AssetRepository.GetByIdAsync(postAuctionRequest.AssetId).GetAwaiter().GetResult();
            if (asset == null)
                return Error.NotFound("Asset not found");

            if (asset.UserId != postAuctionRequest.OwnerId)
                return Error.BadRequest("You can only auction your own assets");            
            
            if (asset.Status != AssetStatus.Open)
                return Error.BadRequest("Asset must be in 'Open' status");

            // Additional business rule validations AFTER asset checks
            var validationResult = ValidatePostAuctionRequest(postAuctionRequest);
            if (!validationResult.IsSuccess)
                return validationResult.Error;

            // Create new auction
            var auction = new Auction
            {
                UserId = postAuctionRequest.OwnerId,
                AssetId = postAuctionRequest.AssetId,
                ReservedPrice = postAuctionRequest.ReservedPrice,
                CurrentHighestBid = 0, // Start with no bids
                CurrentHighestBidderId = 0,
                MinimumBidIncrement = postAuctionRequest.MinimumBidIncrement,
                StartDate = DateTime.UtcNow,
                TotalMinutesToExpiry = postAuctionRequest.TotalMinutesToExpiry,
                Status = AuctionStatus.Live
            };

            _appUnitOfWork.AuctionRepository.AddAsync(auction);

            // Update asset status to indicate it's being auctioned
            asset.Status = AssetStatus.ClosedForAuction;
            _appUnitOfWork.AssetRepository.UpdateAsync(asset);

            _appUnitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            return Error.InternalServerError(ex.Message);
        }
    }    public async Task<Result<bool>> CheckAuctionExpiriesAsync()
    {
        try
        {
            var liveAuctions = (await _appUnitOfWork.AuctionRepository.GetAllAsync())
                .Where(a => a.Status == AuctionStatus.Live && a.IsExpired()).ToList();

            foreach (var auction in liveAuctions)
            {
                var asset = await _appUnitOfWork.AssetRepository.GetByIdAsync(auction.AssetId);
                if (asset == null) continue;

                if (auction.IsExpiredWithoutBids())
                {
                    // No bids received - revert asset to Open status per SRS 4.1.5
                    auction.Status = AuctionStatus.ExpiredWithoutBids;
                    asset.Status = AssetStatus.Open;
                    
                    await _appUnitOfWork.AuctionRepository.UpdateAsync(auction);
                    await _appUnitOfWork.AssetRepository.UpdateAsync(asset);
                }
                else
                {
                    // Auction has bids - handle ownership transfer per SRS 4.1.6
                    var buyer = await _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(auction.CurrentHighestBidderId);
                    var seller = await _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(auction.UserId);

                    if (buyer != null && seller != null)
                    {
                        // Transfer the amount from buyer to seller
                        buyer.WalletBalance -= auction.CurrentHighestBid; // Deduct from total balance
                        buyer.WalletBalanceBlocked -= auction.CurrentHighestBid; // Remove blocked amount
                        seller.WalletBalance += auction.CurrentHighestBid; // Credit to seller

                        // Update both users
                        await _appUnitOfWork.PortalUserRepository.UpdateAsync(buyer);
                        await _appUnitOfWork.PortalUserRepository.UpdateAsync(seller);

                        // Transfer asset ownership to highest bidder and update statuses
                        auction.Status = AuctionStatus.Expired;
                        
                        // Change ownership and set status to Open per SRS 4.1.6
                        asset.UserId = auction.CurrentHighestBidderId; // Transfer to buyer
                        asset.Status = AssetStatus.Open; // Set to Open so new owner can auction it                        // Save changes
                        await _appUnitOfWork.AuctionRepository.UpdateAsync(auction);
                        await _appUnitOfWork.AssetRepository.UpdateAsync(asset);
                    }
                }
            }

            await _appUnitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            return Error.InternalServerError(ex.Message);
        }
    }    public Result<AuctionResponse> GetAuctionByIdAsync(int auctionId)
    {
        try
        {
            var auction = _appUnitOfWork.AuctionRepository.GetByIdAsync(auctionId).GetAwaiter().GetResult();
            if (auction == null)
                return Error.NotFound("Auction not found");

            var asset = _appUnitOfWork.AssetRepository.GetByIdAsync(auction.AssetId).GetAwaiter().GetResult();
            var highestBidder = auction.CurrentHighestBidderId > 0 
                ? _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(auction.CurrentHighestBidderId).GetAwaiter().GetResult()
                : null;

            var response = new AuctionResponse
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

            return response;
        }
        catch (Exception ex)
        {
            return Error.InternalServerError(ex.Message);
        }
    }    public Result<List<AuctionResponse>> GetAuctionsByUserIdAsync(int userId)
    {
        try
        {
            var auctions = _appUnitOfWork.AuctionRepository.GetAuctionsByUserIdAsync(userId).GetAwaiter().GetResult();
            
            var response = auctions.Select(auction => {
                var asset = _appUnitOfWork.AssetRepository.GetByIdAsync(auction.AssetId).GetAwaiter().GetResult();
                var highestBidder = auction.CurrentHighestBidderId > 0 
                    ? _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(auction.CurrentHighestBidderId).GetAwaiter().GetResult()
                    : null;

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
    }    public Result<List<AuctionResponse>> GetAllOpenAuctionsByUserIdAsync()
    {
        try
        {
            // TODO: Get current user ID from context
            int currentUserId = 1; // Placeholder

            var allLiveAuctions = _appUnitOfWork.AuctionRepository.GetAllAsync().GetAwaiter().GetResult()
                .Where(a => a.Status == AuctionStatus.Live && !a.IsExpired())
                .ToList();

            // Get user's bid history to identify auctions they've bid on
            var userBidHistory = _appUnitOfWork.AuctionRepository.GetBidHistoriesByUserIdAsync(currentUserId).GetAwaiter().GetResult();
            var auctionsWithUserBids = userBidHistory.Select(b => b.AuctionId).Distinct().ToList();

            // Sort: auctions with user's highest bid first, then by nearest expiry
            var sortedAuctions = allLiveAuctions
                .OrderByDescending(a => auctionsWithUserBids.Contains(a.Id) && a.CurrentHighestBidderId == currentUserId)
                .ThenBy(a => a.GetRemainingTimeInMinutes())
                .ToList();

            var response = sortedAuctions.Select(auction => {
                var asset = _appUnitOfWork.AssetRepository.GetByIdAsync(auction.AssetId).GetAwaiter().GetResult();
                var highestBidder = auction.CurrentHighestBidderId > 0 
                    ? _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(auction.CurrentHighestBidderId).GetAwaiter().GetResult()
                    : null;

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

    public async Task<Result<IEnumerable<Auction>>> GetLiveAuctionsAsync()
    {
        try
        {
            var auctions = await _appUnitOfWork.AuctionRepository.GetAllAsync();
            var liveAuctions = auctions.Where(a => a.Status == AuctionStatus.Live);
            return Result<IEnumerable<Auction>>.Success(liveAuctions);
        }
        catch (Exception ex)
        {
            return Error.InternalServerError(ex.Message);
        }
    }

    private Result<bool> ValidatePostAuctionRequest(PostAuctionRequest request)
    {
        // Reserve Price validation
        if (request.ReservedPrice <= 0 || request.ReservedPrice > 9999)
            return Error.BadRequest("Reserve price must be between $1 and $9999");

        // Incremental Value validation
        if (request.MinimumBidIncrement <= 0 || request.MinimumBidIncrement > 999)
            return Error.BadRequest("Minimum bid increment must be between $1 and $999");

        // Minimum bid increment must be less than reserve price
        if (request.MinimumBidIncrement >= request.ReservedPrice)
            return Error.BadRequest("Minimum bid increment must be less than the reserve price");

        // Minimum bid increment must be at least 1% of reserve price
        if (request.MinimumBidIncrement < Math.Max(1, Math.Floor(request.ReservedPrice / 100.0)))
            return Error.BadRequest("Minimum bid increment must be at least 1% of the reserve price");

        // Expiration Time validation
        if (request.TotalMinutesToExpiry <= 0 || request.TotalMinutesToExpiry > 10080) // 7 days = 10080 minutes
            return Error.BadRequest("Expiration time must be between 1 minute and 10080 minutes (7 days)");

        return true;
    }
}
