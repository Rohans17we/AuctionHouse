import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { auctionAPI, bidAPI } from '../services/api';
import './AuctionDetails.css';

const AuctionDetails = () => {
  const { id } = useParams();
  const { user } = useAuth();
  const navigate = useNavigate();
  
  const [auction, setAuction] = useState(null);
  const [bidHistory, setBidHistory] = useState([]);
  const [bidAmount, setBidAmount] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => {
    fetchAuctionDetails();
    fetchBidHistory();
  }, [id]);
  const fetchAuctionDetails = async () => {
    try {
      setLoading(true);
      const response = await auctionAPI.getById(id);
      setAuction(response.data);
    } catch (error) {
      setError('Error fetching auction details');
      console.error('Error:', error);
    } finally {
      setLoading(false);
    }
  };
  const fetchBidHistory = async () => {
    try {
      const response = await bidAPI.getBidHistory(id);
      setBidHistory(response.data);
    } catch (error) {
      console.error('Error fetching bid history:', error);
    }
  };

  const handlePlaceBid = async (e) => {
    e.preventDefault();
    
    if (!bidAmount || parseFloat(bidAmount) <= 0) {
      setError('Please enter a valid bid amount');
      return;
    }

    const currentHighestBid = bidHistory.length > 0 
      ? Math.max(...bidHistory.map(bid => bid.bidAmount))
      : auction.startingBid;

    if (parseFloat(bidAmount) <= currentHighestBid) {
      setError(`Bid must be higher than current highest bid of $${currentHighestBid.toFixed(2)}`);
      return;
    }

    try {
      setLoading(true);
      setError('');
      setSuccess('');

      const bidData = {
        auctionId: parseInt(id),
        bidderId: user.id,
        bidAmount: parseFloat(bidAmount)
      };      const response = await bidAPI.placeBid(bidData);

      setSuccess('Bid placed successfully!');
      setBidAmount('');
      await fetchBidHistory();
      await fetchAuctionDetails();
    } catch (error) {
      setError('Error placing bid');
      console.error('Error:', error);
    } finally {
      setLoading(false);
    }
  };

  const formatDateTime = (dateString) => {
    return new Date(dateString).toLocaleString();
  };

  const getAuctionStatus = () => {
    if (!auction) return 'Unknown';
    
    const now = new Date();
    const startTime = new Date(auction.startTime);
    const endTime = new Date(auction.endTime);
    
    if (now < startTime) return 'Upcoming';
    if (now > endTime) return 'Ended';
    return 'Active';
  };

  const getTimeRemaining = () => {
    if (!auction) return '';
    
    const now = new Date();
    const endTime = new Date(auction.endTime);
    const timeDiff = endTime - now;
    
    if (timeDiff <= 0) return 'Auction ended';
    
    const days = Math.floor(timeDiff / (1000 * 60 * 60 * 24));
    const hours = Math.floor((timeDiff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    const minutes = Math.floor((timeDiff % (1000 * 60 * 60)) / (1000 * 60));
    
    if (days > 0) return `${days}d ${hours}h ${minutes}m remaining`;
    if (hours > 0) return `${hours}h ${minutes}m remaining`;
    return `${minutes}m remaining`;
  };

  const getCurrentHighestBid = () => {
    if (bidHistory.length === 0) return auction?.startingBid || 0;
    return Math.max(...bidHistory.map(bid => bid.bidAmount));
  };

  const canPlaceBid = () => {
    const status = getAuctionStatus();
    return status === 'Active' && auction?.sellerId !== user.id;
  };

  if (loading && !auction) {
    return <div className="loading">Loading auction details...</div>;
  }

  if (error && !auction) {
    return (
      <div className="error-container">
        <div className="error-message">{error}</div>
        <button onClick={() => navigate('/dashboard')} className="back-btn">
          Back to Dashboard
        </button>
      </div>
    );
  }

  if (!auction) {
    return (
      <div className="error-container">
        <div className="error-message">Auction not found</div>
        <button onClick={() => navigate('/dashboard')} className="back-btn">
          Back to Dashboard
        </button>
      </div>
    );
  }

  return (
    <div className="auction-details">
      <div className="auction-header">
        <button onClick={() => navigate('/dashboard')} className="back-btn">
          ← Back to Dashboard
        </button>
        <div className="auction-status">
          <span className={`status-badge ${getAuctionStatus().toLowerCase()}`}>
            {getAuctionStatus()}
          </span>
        </div>
      </div>

      <div className="auction-content">
        <div className="auction-main">
          <div className="auction-info">
            <h1>{auction.title}</h1>
            <p className="auction-description">{auction.description}</p>
            
            <div className="auction-timing">
              <div className="time-info">
                <strong>Start:</strong> {formatDateTime(auction.startTime)}
              </div>
              <div className="time-info">
                <strong>End:</strong> {formatDateTime(auction.endTime)}
              </div>
              <div className="time-remaining">
                {getTimeRemaining()}
              </div>
            </div>

            <div className="asset-info">
              <h3>Asset Details</h3>
              <div className="asset-details">
                <p><strong>Name:</strong> {auction.asset?.name}</p>
                <p><strong>Category:</strong> {auction.asset?.category}</p>
                <p><strong>Condition:</strong> {auction.asset?.condition}</p>
                <p><strong>Description:</strong> {auction.asset?.description}</p>
              </div>
            </div>
          </div>

          {auction.asset?.imageUrl && (
            <div className="auction-image">
              <img src={auction.asset.imageUrl} alt={auction.asset.name} />
            </div>
          )}
        </div>

        <div className="auction-sidebar">
          <div className="bid-section">
            <div className="current-bid">
              <h3>Current Highest Bid</h3>
              <div className="bid-amount">
                ${getCurrentHighestBid().toFixed(2)}
              </div>
              <div className="starting-bid">
                Starting bid: ${auction.startingBid.toFixed(2)}
              </div>
              <div className="reserve-price">
                Reserve price: ${auction.reservePrice.toFixed(2)}
              </div>
            </div>

            {error && <div className="error-message">{error}</div>}
            {success && <div className="success-message">{success}</div>}

            {canPlaceBid() && (
              <form onSubmit={handlePlaceBid} className="bid-form">
                <div className="form-group">
                  <label htmlFor="bidAmount">Your Bid ($)</label>
                  <input
                    type="number"
                    id="bidAmount"
                    value={bidAmount}
                    onChange={(e) => setBidAmount(e.target.value)}
                    min={getCurrentHighestBid() + 0.01}
                    step="0.01"
                    placeholder={`Minimum: $${(getCurrentHighestBid() + 0.01).toFixed(2)}`}
                    required
                  />
                </div>
                <button type="submit" disabled={loading} className="bid-btn">
                  {loading ? 'Placing Bid...' : 'Place Bid'}
                </button>
              </form>
            )}

            {auction.sellerId === user.id && (
              <div className="seller-notice">
                <p>You are the seller of this item</p>
              </div>
            )}

            {getAuctionStatus() === 'Ended' && (
              <div className="auction-ended">
                <p>This auction has ended</p>
              </div>
            )}
          </div>

          <div className="bid-history">
            <h3>Bid History</h3>
            {bidHistory.length === 0 ? (
              <p className="no-bids">No bids yet</p>
            ) : (
              <div className="bid-list">
                {bidHistory
                  .sort((a, b) => new Date(b.bidTime) - new Date(a.bidTime))
                  .map((bid, index) => (
                    <div key={index} className="bid-item">
                      <div className="bid-info">
                        <span className="bid-amount">${bid.bidAmount.toFixed(2)}</span>
                        <span className="bid-time">
                          {new Date(bid.bidTime).toLocaleString()}
                        </span>
                      </div>                      <div className="bidder-info">
                        {bid.userId === user.id ? 'You' : `User ${bid.userId}`}
                      </div>
                    </div>
                  ))}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default AuctionDetails;
