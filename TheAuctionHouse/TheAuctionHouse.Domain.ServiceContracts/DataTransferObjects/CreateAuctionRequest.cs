using System;

namespace TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects
{
    public class CreateAuctionRequest
    {
        public int AssetId { get; set; }
        public decimal StartingBid { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
