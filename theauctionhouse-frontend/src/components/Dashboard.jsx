import React, { useState, useEffect } from 'react';
import { dashboardAPI, walletService, auctionService, bidAPI } from '../services/api';
import { useAuth } from '../contexts/AuthContext';
import AuctionCard from './AuctionCard';
import './Dashboard.css';

const Dashboard = () => {
  const [dashboardData, setDashboardData] = useState(null);
  const [liveAuctions, setLiveAuctions] = useState([]);
  const [userHighestBids, setUserHighestBids] = useState([]);
  const [userBidHistory, setUserBidHistory] = useState([]);
  const [walletBalance, setWalletBalance] = useState(0);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('live');

  const { user } = useAuth();

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        // Ensure auction expiries are checked before fetching dashboard data
        await auctionService.checkExpiries();        // Fetch all dashboard data
        const [dashboardRes, liveAuctionsRes, userBidsRes, userBidHistoryRes, walletRes] = await Promise.allSettled([
          dashboardAPI.getDashboard(user.id),
          dashboardAPI.getLiveAuctions(),
          dashboardAPI.getUserHighestBids(user.id),
          bidAPI.getUserBidHistory(user.id),
          walletService.getBalance(user.id)
        ]);

        // Check each response and handle accordingly
        if (dashboardRes.status === 'fulfilled' && dashboardRes.value?.data) {
          setDashboardData(dashboardRes.value.data);
        }

        if (liveAuctionsRes.status === 'fulfilled' && liveAuctionsRes.value?.data) {
          setLiveAuctions(liveAuctionsRes.value.data);
        }

        if (userBidsRes.status === 'fulfilled' && userBidsRes.value?.data) {
          setUserHighestBids(userBidsRes.value.data);
        }

        if (userBidHistoryRes.status === 'fulfilled' && userBidHistoryRes.value?.data) {
          setUserBidHistory(userBidHistoryRes.value.data);
        }if (walletRes.status === 'fulfilled') {
          setWalletBalance(walletRes.value?.balance || 0);
        }
      } catch (error) {
        console.error('Error fetching dashboard data:', error);
      } finally {
        setLoading(false);
      }
    };

    if (user?.id) {
      fetchData();
    } else {
      setLoading(false);
    }
  }, [user]);

  if (loading) {
    return (
      <div className="dashboard-loading">
        <div className="loading-spinner" aria-label="Loading"></div>
        <p>Loading your dashboard data...</p>
      </div>
    );
  }

  return (
    <div className="dashboard">
      <div className="dashboard-header">
        <h1>Welcome back, {user?.name}!</h1>
        <div className="wallet-balance">
          <h3>Wallet Balance: ${walletBalance.toFixed(2)}</h3>
        </div>
      </div>

      <div className="dashboard-stats">
        <div className="stat-card">
          <h3>{liveAuctions.length}</h3>
          <p>Live Auctions</p>
        </div>
        <div className="stat-card">
          <h3>{userHighestBids.length}</h3>
          <p>Your Highest Bids</p>
        </div>        <div className="stat-card">
          <h3>{userBidHistory.length}</h3>
          <p>Total Bids Placed</p>
        </div>
      </div>

      <div className="dashboard-tabs">
        <button 
          className={`tab-btn ${activeTab === 'live' ? 'active' : ''}`}
          onClick={() => setActiveTab('live')}
        >
          Live Auctions
        </button>
        <button 
          className={`tab-btn ${activeTab === 'winning' ? 'active' : ''}`}
          onClick={() => setActiveTab('winning')}
        >
          Your Winning Bids
        </button>
        <button 
          className={`tab-btn ${activeTab === 'bids' ? 'active' : ''}`}
          onClick={() => setActiveTab('bids')}
        >
          Your Bid History
        </button>
      </div>

      <div className="dashboard-content">
        {activeTab === 'live' && (
          <div className="auctions-grid">
            {liveAuctions.length > 0 ? (
              liveAuctions.map(auction => (
                <AuctionCard key={auction.auctionId} auction={auction} />
              ))
            ) : (
              <div className="empty-state">
                <p>No live auctions at the moment.</p>
              </div>
            )}
          </div>
        )}

        {activeTab === 'winning' && (
          <div className="auctions-grid">
            {userHighestBids.length > 0 ? (
              userHighestBids.map(auction => (
                <AuctionCard key={auction.auctionId} auction={auction} isWinning={true} />
              ))
            ) : (
              <div className="empty-state">
                <p>You don't have any winning bids yet.</p>
              </div>
            )}
          </div>
        )}        {activeTab === 'bids' && (
          <div className="bid-history">
            {userBidHistory.length > 0 ? (
              <div className="bid-list">                {userBidHistory.map(bid => (
                  <div key={bid.bidId || `${bid.auctionId}-${bid.bidAmount}-${bid.bidTime}`} className="bid-item">
                    <div className="bid-info">
                      <h4>Auction #{bid.auctionId}</h4>
                      <p>Bid Amount: ${bid.bidAmount}</p>
                      <p className="bid-date">{new Date(bid.bidTime).toLocaleDateString()}</p>
                      {bid.assetTitle && <p className="asset-title">Asset: {bid.assetTitle}</p>}
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="empty-state">
                <p>You haven't placed any bids yet.</p>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default Dashboard;
