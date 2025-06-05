using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Common.Validation;
using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Domain.ServiceContracts;

namespace TheAuctionHouse.Domain.Services;

public class BidService : IBidService
{
    private readonly IAppUnitOfWork _appUnitOfWork;

    public BidService(IAppUnitOfWork appUnitOfWork)
    {
        _appUnitOfWork = appUnitOfWork;
    }

    public async Task<Result<bool>> PlaceBidAsync(PlaceBidRequest placeBidRequest)
    {
        try
        {
            // Validate the request
            Error validationError = Error.ValidationFailures();
            if (!ValidationHelper.Validate(placeBidRequest, validationError))
            {
                return validationError;
            }

            // Get the auction
            var auction = await _appUnitOfWork.AuctionRepository.GetByIdAsync(placeBidRequest.AuctionId);
            if (auction == null)
                return Error.NotFound("Auction not found");

            // Check if auction is still live
            if (auction.Status != AuctionStatus.Live || auction.IsExpired())
                return Error.BadRequest("Auction is not active or has expired");

            // Get the bidder
            var bidder = await _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(placeBidRequest.BidderId);
            if (bidder == null)
                return Error.NotFound("Bidder not found");            // Check reserve price for first bid
            if (auction.CurrentHighestBid == 0 && placeBidRequest.BidAmount < auction.ReservedPrice)
            {
                return Error.BadRequest($"First bid must meet or exceed the reserve price of ${auction.ReservedPrice}");
            }

            // Validate bid amount against current highest bid and increment
            var nextMinimumBid = auction.CurrentHighestBid + auction.MinimumBidIncrement;
            if (auction.CurrentHighestBid > 0 && placeBidRequest.BidAmount < nextMinimumBid)
            {
                return Error.BadRequest($"Bid must be at least ${nextMinimumBid} (current bid + minimum increment)");
            }            // Check if bidder has sufficient available balance
            int availableBalance = bidder.WalletBalance - bidder.WalletBalanceBlocked;
            if (availableBalance < placeBidRequest.BidAmount)
            {
                return Error.BadRequest($"Insufficient funds. You need ${placeBidRequest.BidAmount} available balance to place this bid. Your balance: Total ${bidder.WalletBalance}, Blocked ${bidder.WalletBalanceBlocked}, Available ${availableBalance}");
            }

            // Validate that we're not exceeding maximum allowed blocked amount per user
            const int MaxBlockedAmount = 999999; // $999,999 maximum limit
            if (bidder.WalletBalanceBlocked + placeBidRequest.BidAmount > MaxBlockedAmount)
            {
                return Error.BadRequest($"Total blocked amount would exceed the maximum limit of ${MaxBlockedAmount}");
            }            // Prevent bidding on own auction
            var asset = await _appUnitOfWork.AssetRepository.GetByIdAsync(auction.AssetId);
            if (asset?.UserId == placeBidRequest.BidderId || auction.UserId == placeBidRequest.BidderId)
            {
                return Error.BadRequest("You cannot bid on your own asset");
            }            // If there's a previous bidder, unblock their bid amount
            if (auction.CurrentHighestBidderId > 0)
            {
                var previousBidder = await _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(auction.CurrentHighestBidderId);
                if (previousBidder != null)
                {
                    // Unblock the previous bid amount only if it's a different bidder
                    if (previousBidder.Id != placeBidRequest.BidderId)
                    {
                        previousBidder.WalletBalanceBlocked -= auction.CurrentHighestBid;
                        await _appUnitOfWork.PortalUserRepository.UpdateAsync(previousBidder);
                    }
                }
            }            // Block amount for new highest bidder - ensure blocked amount never exceeds total balance
            int amountToBlock = placeBidRequest.BidAmount;
            
            // If the user is already the highest bidder, only block the difference
            if (auction.CurrentHighestBidderId == placeBidRequest.BidderId)
            {
                amountToBlock = placeBidRequest.BidAmount - auction.CurrentHighestBid;
                
                // If the new bid is lower than the current bid (shouldn't happen due to validation),
                // don't block any additional amount
                if (amountToBlock <= 0)
                {
                    amountToBlock = 0;
                }
            }
            
            int newBlockedAmount = bidder.WalletBalanceBlocked + amountToBlock;
            if (newBlockedAmount > bidder.WalletBalance)
            {
                return Error.BadRequest("Bid would cause blocked amount to exceed wallet balance. Please deposit additional funds.");
            }
            
            bidder.WalletBalanceBlocked = newBlockedAmount;
            await _appUnitOfWork.PortalUserRepository.UpdateAsync(bidder);

            // Update auction with new highest bid
            auction.CurrentHighestBid = placeBidRequest.BidAmount;
            auction.CurrentHighestBidderId = placeBidRequest.BidderId;            await _appUnitOfWork.AuctionRepository.UpdateAsync(auction);

            // Create bid history record
            var bidHistory = new BidHistory
            {
                AuctionId = placeBidRequest.AuctionId,
                BidderId = placeBidRequest.BidderId,
                BidderName = bidder.Name.Length, // Note: Entity has int BidderName which seems incorrect
                BidAmount = placeBidRequest.BidAmount,
                BidDate = DateTime.UtcNow
            };

            await _appUnitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            return Error.InternalServerError(ex.Message);
        }
    }

    public async Task<Result<List<BidHistoryResponse>>> GetBidHistoryByAuctionIdAsync(int auctionId)
    {
        try
        {
            var bidHistories = await _appUnitOfWork.AuctionRepository.GetBidHistoriesByAuctionIdAsync(auctionId);
            
            var response = new List<BidHistoryResponse>();
            foreach (var bid in bidHistories)
            {
                var bidder = await _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(bid.BidderId);
                response.Add(new BidHistoryResponse
                {
                    BidId = bid.Id,
                    AuctionId = bid.AuctionId,
                    UserId = bid.BidderId,
                    BidAmount = bid.BidAmount,
                    BidTime = bid.BidDate,
                    UserName = bidder?.Name ?? ""
                });
            }

            return response;
        }
        catch (Exception ex)
        {
            return Error.InternalServerError(ex.Message);
        }
    }

    public async Task<Result<List<BidHistoryResponse>>> GetBidHistoryByUserIdAsync(int userId)
    {
        try
        {
            var bidHistories = await _appUnitOfWork.AuctionRepository.GetBidHistoriesByUserIdAsync(userId);
            
            var response = new List<BidHistoryResponse>();
            foreach (var bid in bidHistories)
            {
                var bidder = await _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(bid.BidderId);
                response.Add(new BidHistoryResponse
                {
                    BidId = bid.Id,
                    AuctionId = bid.AuctionId,
                    UserId = bid.BidderId,
                    BidAmount = bid.BidAmount,
                    BidTime = bid.BidDate,
                    UserName = bidder?.Name ?? ""
                });
            }

            return response;
        }
        catch (Exception ex)
        {
            return Error.InternalServerError(ex.Message);
        }
    }

    public async Task<Result<bool>> CheckAndUnblockPreviousBidsAsync(int auctionId, int newHighestBidderId)
    {
        try
        {
            var auction = await _appUnitOfWork.AuctionRepository.GetByIdAsync(auctionId);
            if (auction == null)
                return Error.NotFound("Auction not found");            // Get all bid histories for this auction
            var bidHistories = await _appUnitOfWork.AuctionRepository.GetBidHistoriesByAuctionIdAsync(auctionId);
            if (bidHistories == null)
            {
                return true; // No bids to unblock
            }
            
            // Unblock amounts for all bidders except the new highest bidder
            foreach (var bid in bidHistories)
            {
                if (bid.BidderId != newHighestBidderId)
                {
                    var bidder = await _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(bid.BidderId);
                    if (bidder != null)
                    {
                        bidder.WalletBalanceBlocked -= bid.BidAmount;
                        await _appUnitOfWork.PortalUserRepository.UpdateAsync(bidder);
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
    }

    private Result<bool> ValidateBidAmount(Auction auction, int bidAmount)
    {
        // Bid amount must be at least the reserve price if no bids exist
        if (auction.CurrentHighestBid == 0)
        {
            if (bidAmount < auction.ReservedPrice)
                return Error.BadRequest($"Bid amount must be at least the reserve price of ${auction.ReservedPrice}");
        }
        else
        {
            // Bid amount must be at least current highest bid + minimum increment
            var minimumBid = auction.CurrentHighestBid + auction.MinimumBidIncrement;
            if (bidAmount < minimumBid)
                return Error.BadRequest($"Bid amount must be at least ${minimumBid} (current highest + minimum increment)");
        }

        // Bid amount validation (positive and reasonable limit)
        if (bidAmount <= 0)
            return Error.BadRequest("Bid amount must be positive");

        if (bidAmount > 999999) // Max bid limit as per wallet restrictions
            return Error.BadRequest("Bid amount cannot exceed $999,999");

        return true;
    }
}
