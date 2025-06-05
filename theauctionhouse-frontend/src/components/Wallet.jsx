import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { walletService } from '../services/api';
import './Wallet.css';

const Wallet = () => {
  const { user } = useAuth();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [walletInfo, setWalletInfo] = useState({
    totalBalance: 0,
    blockedBalance: 0,
    availableBalance: 0,
    activeHighestBids: [],
    bidDetails: []
  });
  const [depositAmount, setDepositAmount] = useState('');

  useEffect(() => {
    fetchWalletInfo();
  }, []);
  const fetchWalletInfo = async () => {
    try {
      setLoading(true);
      const response = await walletService.getWalletInfo(user.id);
      console.log('Wallet info response:', response);
      
      if (response.isSuccess) {
        const totalBalance = response.data.balance || 0;
        const blockedAmount = response.data.blockedAmount || 0;
        const availableBalance = Math.max(0, totalBalance - blockedAmount);
        
        console.log('Processing wallet info:', {
          totalBalance,
          blockedAmount,
          availableBalance,
          bidHistory: response.data.bidHistory
        });
        
        setWalletInfo({
          totalBalance: totalBalance,
          blockedBalance: blockedAmount,
          availableBalance: availableBalance,
          activeHighestBids: response.data.bidHistory || [],
          bidDetails: response.data.bidHistory || []
        });
        
        // Add warning if blocked amount exceeds total balance
        if (blockedAmount > totalBalance) {
          setError('Warning: Your blocked amount exceeds your total balance. This will be automatically corrected.');
        } else {
          setError('');
        }
      } else {
        console.error('Failed to fetch wallet info:', response);
        setError(response.error || 'Failed to fetch wallet information');
      }
    } catch (error) {
      console.error('Error loading wallet information:', error);
      setError(error.message || 'Error loading wallet information');
    } finally {
      setLoading(false);
    }
  };

  const handleDeposit = async (e) => {
    e.preventDefault();
    const amount = parseInt(depositAmount);
    
    if (isNaN(amount) || amount <= 0 || amount > 999999) {
      setError('Amount must be between $1 and $999,999');
      return;
    }

    try {
      setLoading(true);
      const response = await walletService.deposit({
        userId: user.id,
        amount: amount
      });

      if (response.isSuccess) {
        setDepositAmount('');
        await fetchWalletInfo();
        setError('');
      } else {
        setError(response.error || 'Failed to deposit amount');
      }
    } catch (error) {
      setError('Error processing deposit');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="wallet-container">
      <h1>My Wallet</h1>

      <div className="wallet-summary">        <div className="balance-card total">
          <h3>Total Balance</h3>
          <p className="amount">${walletInfo.totalBalance ? walletInfo.totalBalance.toFixed(2) : '0.00'}</p>
        </div>
          <div className={`balance-card blocked ${walletInfo.blockedBalance > walletInfo.totalBalance ? 'error' : ''}`}>
          <h3>Blocked Amount</h3>
          <p className="amount">${walletInfo.blockedBalance ? walletInfo.blockedBalance.toFixed(2) : '0.00'}</p>
          <small>Amount reserved for active bids</small>
          {walletInfo.blockedBalance > walletInfo.totalBalance && (
            <div className="warning-message">
              Warning: Blocked amount exceeds total balance. This will be corrected automatically.
            </div>
          )}
          {walletInfo.blockedBalance > 0 && walletInfo.activeHighestBids.length > 0 && (
            <div className="blocked-details">
              <h4>Active Bid Details</h4>
              {walletInfo.activeHighestBids.map(bid => (
                <div key={bid.auctionId} className="bid-entry">                  <span className="bid-amount">${bid.bidAmount ? bid.bidAmount.toFixed(2) : '0.00'}</span>
                  <span className="bid-auction">Auction #{bid.auctionId}</span>
                  <span className="bid-time">{new Date(bid.bidTime).toLocaleDateString()}</span>
                </div>
              ))}
            </div>
          )}
        </div>
          <div className="balance-card available">
          <h3>Available Balance</h3>
          <p className="amount">${walletInfo.availableBalance ? walletInfo.availableBalance.toFixed(2) : '0.00'}</p>
          <small>Amount available for new bids</small>
        </div>
      </div>

      <div className="deposit-section">
        <h2>Add Funds</h2>
        <form onSubmit={handleDeposit}>
          <div className="form-group">
            <label htmlFor="depositAmount">Amount to Add ($)</label>
            <input
              type="number"
              id="depositAmount"
              value={depositAmount}
              onChange={(e) => setDepositAmount(e.target.value)}
              placeholder="Enter amount to deposit"
              min="1"
              max="999999"
              required
            />
            <small>Enter amount between $1 and $999,999</small>
          </div>
          <button type="submit" disabled={loading}>
            {loading ? 'Processing...' : 'Deposit'}
          </button>
        </form>
      </div>

      {error && <div className="error-message">{error}</div>}

      {walletInfo.bidDetails.length > 0 && (
        <div className="active-bids-section">
          <h2>All Active Bids</h2>
          <div className="bid-history">
            {walletInfo.bidDetails.map(bid => (
              <div key={bid.bidId} className="bid-history-entry">
                <div className="bid-info">
                  <span className="bid-label">Auction</span>
                  <span className="bid-value">#{bid.auctionId}</span>
                </div>
                <div className="bid-info">
                  <span className="bid-label">Amount</span>
                  <span className="bid-value">${bid.bidAmount ? bid.bidAmount.toFixed(2) : '0.00'}</span>
                </div>
                <div className="bid-info">
                  <span className="bid-label">Date</span>
                  <span className="bid-value">{new Date(bid.bidTime).toLocaleDateString()}</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

export default Wallet;
