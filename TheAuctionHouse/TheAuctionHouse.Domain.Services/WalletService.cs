using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Common.Validation;
using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Domain.ServiceContracts;

namespace TheAuctionHouse.Domain.Services;

public class WalletService : IWalletService
{
    private readonly IAppUnitOfWork _appUnitOfWork;

    public WalletService(IAppUnitOfWork appUnitOfWork)
    {
        _appUnitOfWork = appUnitOfWork;
    }

    public async Task<Result<bool>> DepositAsync(WalletTransactionRequest walletTransactionRequest)
    {
        try
        {
            // Validate the request
            Error validationError = Error.ValidationFailures();
            if (!ValidationHelper.Validate(walletTransactionRequest, validationError))
            {
                return validationError;
            }

            // Additional business rule validations
            var amountValidation = ValidateAmount(walletTransactionRequest.Amount);
            if (!amountValidation.IsSuccess)
                return amountValidation.Error;

            // Check if user exists
            var user = await _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(walletTransactionRequest.UserId);
            if (user == null)
                return Error.NotFound("User not found");

            // Perform deposit operation
            _appUnitOfWork.PortalUserRepository.DepositWalletBalance(walletTransactionRequest.UserId, walletTransactionRequest.Amount);
            await _appUnitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            return Error.InternalServerError(ex.Message);
        }
    }

    public async Task<Result<bool>> WithDrawalAsync(WalletTransactionRequest walletTransactionRequest)
    {
        try
        {
            // Validate the request
            Error validationError = Error.ValidationFailures();
            if (!ValidationHelper.Validate(walletTransactionRequest, validationError))
            {
                return validationError;
            }

            // Additional business rule validations
            var amountValidation = ValidateAmount(walletTransactionRequest.Amount);
            if (!amountValidation.IsSuccess)
                return amountValidation.Error;

            // Check if user exists
            var user = await _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(walletTransactionRequest.UserId);
            if (user == null)
                return Error.NotFound("User not found");

            // Check if user has sufficient available balance (total balance - blocked amount)
            int availableBalance = user.WalletBalance - user.WalletBalanceBlocked;
            if (availableBalance < walletTransactionRequest.Amount)
                return Error.BadRequest("Insufficient available balance for withdrawal");

            // Perform withdrawal operation
            _appUnitOfWork.PortalUserRepository.WithdrawWalletBalance(walletTransactionRequest.UserId, walletTransactionRequest.Amount);
            await _appUnitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            return Error.InternalServerError(ex.Message);
        }
    }

    public async Task<Result<WalletBalanceResponse>> GetWalletBalanceAsync(int userId)
    {
        try
        {
            // Check if user exists
            var user = await _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(userId);
            if (user == null)
                return Error.NotFound("User not found");

            // Get user's bid history for open auctions where they are the highest bidder
            var userBidHistory = await _appUnitOfWork.AuctionRepository.GetBidHistoriesByUserIdAsync(userId);
            var openAuctions = (await _appUnitOfWork.AuctionRepository.GetAllAsync())
                .Where(a => a.Status == Domain.Entities.AuctionStatus.Live && !a.IsExpired() && a.CurrentHighestBidderId == userId)
                .ToList();

            var activeBids = new List<BidHistoryResponse>();
            foreach (var auction in openAuctions)
            {
                var asset = await _appUnitOfWork.AssetRepository.GetByIdAsync(auction.AssetId);
                activeBids.Add(new BidHistoryResponse
                {
                    BidId = 0, // TODO: Get actual bid ID from bid history
                    AuctionId = auction.Id,
                    UserId = userId,
                    BidAmount = auction.CurrentHighestBid,
                    BidTime = DateTime.UtcNow, // TODO: Get actual bid date from bid history
                    UserName = user.Name
                });
            }            // Ensure blocked amount never exceeds total balance
            int correctedBlockedAmount = Math.Min(user.WalletBalanceBlocked, user.WalletBalance);
            
            // Verify blocked amount against actual highest bids
            int expectedBlockedAmount = openAuctions.Sum(a => a.CurrentHighestBid);
            
            // If there's a discrepancy, fix the user data
            if (correctedBlockedAmount != user.WalletBalanceBlocked || expectedBlockedAmount != correctedBlockedAmount)
            {
                // Use the expected blocked amount from active highest bids, but never exceed total balance
                int finalBlockedAmount = Math.Min(expectedBlockedAmount, user.WalletBalance);
                user.WalletBalanceBlocked = finalBlockedAmount;
                await _appUnitOfWork.PortalUserRepository.UpdateAsync(user);
                await _appUnitOfWork.SaveChangesAsync();
                
                // Update the correctedBlockedAmount for the response
                correctedBlockedAmount = finalBlockedAmount;
            }
            
            var response = new WalletBalanceResponse
            {
                UserId = user.Id,
                Amount = user.WalletBalance,
                BlockedAmount = correctedBlockedAmount,
                BidHistory = activeBids
            };

            return response;
        }
        catch (Exception ex)
        {
            return Error.InternalServerError(ex.Message);
        }
    }

    private Result<bool> ValidateAmount(int amount)
    {
        if (amount <= 0)
            return Error.BadRequest("Amount must be a positive integer");

        if (amount > 999999)
            return Error.BadRequest("Amount cannot exceed $999,999");

        return true;
    }
}
