import React, { useState, useEffect } from 'react';
import { bidAPI } from '../services/api';
import { useAuth } from '../contexts/AuthContext';
import './AuctionCard.css';

const AuctionCard = ({ auction, isWinning = false }) => {
  const [bidAmount, setBidAmount] = useState('');
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [auctionStatus, setAuctionStatus] = useState('');
  const [isOwnAsset, setIsOwnAsset] = useState(false);

  const { user } = useAuth();
    useEffect(() => {
    // Determine auction status for styling
    const timeRemaining = calculateTimeRemaining();
    if (timeRemaining === 'Auction ended') {
      setAuctionStatus('ended');
    } else if (auction.status?.toLowerCase() === 'active') {
      setAuctionStatus('active');
    } else {
      setAuctionStatus('pending');
    }
    
    // Check if the current user is the owner of this auction
    setIsOwnAsset(user && auction.userId === user.id);
  }, [auction, user]);

  const calculateTimeRemaining = () => {
    if (!auction.startDate || !auction.totalMinutesToExpiry) {
      return 'Time expired';
    }

    const startTime = new Date(auction.startDate + 'Z'); // Treat as UTC
    const expiryTime = new Date(startTime.getTime() + (auction.totalMinutesToExpiry * 60000));
    const now = new Date();
    const timeLeft = expiryTime.getTime() - now.getTime();

    if (timeLeft <= 0) {
      return 'Auction ended';
    }

    const hours = Math.floor(timeLeft / (1000 * 60 * 60));
    const minutes = Math.floor((timeLeft % (1000 * 60 * 60)) / (1000 * 60));
    
    if (hours > 0) {
      return `${hours}h ${minutes}m remaining`;
    }
    return `${minutes}m remaining`;
  };
  const handlePlaceBid = async (e) => {
    e.preventDefault();
    
    // Prevent bidding on own asset (redundant check for security)
    if (isOwnAsset) {
      setMessage('You cannot bid on your own asset');
      return;
    }
    
    if (!bidAmount || parseFloat(bidAmount) <= 0) {
      setMessage('Please enter a valid bid amount');
      return;
    }

    if (parseFloat(bidAmount) <= auction.currentHighestBid) {
      setMessage(`Bid must be higher than current bid of $${auction.currentHighestBid}`);
      return;
    }

    setLoading(true);
    setMessage('');

    try {
      const bidData = {
        auctionId: auction.auctionId,
        bidderId: user.id,
        bidAmount: parseFloat(bidAmount)
      };

      await bidAPI.placeBid(bidData);
      setMessage('Bid placed successfully!');
      setBidAmount('');
      
      // Refresh page after successful bid
      setTimeout(() => {
        window.location.reload();
      }, 1500);
    } catch (error) {
      setMessage(error.response?.data?.error || 'Failed to place bid');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className={`auction-card ${isWinning ? 'winning' : ''}`}>
      <div className="auction-header">
        <h3>{auction.assetTitle}</h3>
        {isWinning && <span className="winning-badge">Winning!</span>}
        <div className="status-badge" data-status={auctionStatus}>{auction.status}</div>
      </div>
        <div className="auction-details">
        <p className="asset-description">{auction.assetDescription}</p>
        
        <div className="auction-info">
          <div className="info-row">
            <span>Retail Value:</span>
            <span className="retail-value">${auction.retailValue?.toFixed(2) || '0.00'}</span>
          </div>
          
          <div className="info-row">
            <span>Current Bid:</span>
            <span className="current-bid">${auction.currentHighestBid?.toFixed(2) || '0.00'}</span>
          </div>
          
          <div className="info-row">
            <span>Next Call:</span>
            <span className="next-call">${(auction.currentHighestBid + auction.minimumBidIncrement).toFixed(2)}</span>
          </div>

          {auction.highestBidderName && (
            <div className="info-row">
              <span>Highest Bidder:</span>
              <span className="highest-bidder">{auction.highestBidderName}</span>
            </div>
          )}
          
          <div className="info-row">
            <span>Time Remaining:</span>
            <span className="time-remaining">{calculateTimeRemaining()}</span>
          </div>
        </div>
      </div>      {!isWinning && calculateTimeRemaining() !== 'Auction ended' && !isOwnAsset && (
        <div className="bid-section">
          <form onSubmit={handlePlaceBid}>
            <div className="bid-input-group">
              <input
                type="number"
                value={bidAmount}
                onChange={(e) => setBidAmount(e.target.value)}
                placeholder={`Min bid: $${(auction.currentHighestBid + auction.minimumBidIncrement).toFixed(2)}`}
                min={auction.currentHighestBid + auction.minimumBidIncrement}
                step="0.01"
                required
              />
              <button type="submit" disabled={loading} className="bid-btn">
                {loading ? 'Placing Bid...' : 'Place Bid'}
              </button>
            </div>
          </form>
          {message && <div className={`message ${message.includes('success') ? 'success' : 'error'}`}>{message}</div>}
        </div>
      )}
      
      {!isWinning && calculateTimeRemaining() !== 'Auction ended' && isOwnAsset && (
        <div className="bid-section own-asset">
          <div className="message info">
            You cannot bid on your own asset
          </div>
        </div>
      )}
    </div>
  );
};

export default AuctionCard;


