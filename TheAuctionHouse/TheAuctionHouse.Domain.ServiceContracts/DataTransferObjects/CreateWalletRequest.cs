namespace TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects
{
    public class CreateWalletRequest
    {
        public int UserId { get; set; }
        public decimal InitialBalance { get; set; }
    }
}
