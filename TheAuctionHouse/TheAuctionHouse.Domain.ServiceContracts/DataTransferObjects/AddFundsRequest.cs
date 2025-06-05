namespace TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects
{
    public class AddFundsRequest
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
    }
}
