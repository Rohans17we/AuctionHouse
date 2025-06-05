import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { walletService } from '../services/api';
import './WalletManagement.css';

const WalletManagement = () => {
  const { user } = useAuth();  const [walletData, setWalletData] = useState({
    totalBalance: 0,
    blockedBalance: 0,
    availableBalance: 0,
    activeHighestBids: []
  });
  const [addFundsAmount, setAddFundsAmount] = useState('');
  const [withdrawAmount, setWithdrawAmount] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Effect to fetch wallet balance whenever the user changes
  useEffect(() => {
    console.log('WalletManagement: User state changed:', user);
    if (user?.id) {
      fetchWalletBalance();
    } else {
      console.warn('WalletManagement: No user ID available');
      setError('Please log in to access your wallet.');
    }
  }, [user]);
  const fetchWalletBalance = async () => {
    if (!user?.id) {
      console.warn('fetchWalletBalance: No user ID available');
      setError('Please log in to view your wallet balance.');
      return;
    }

    try {
      setLoading(true);
      setError('');
      const response = await walletService.getBalance(user.id);
      console.log('Wallet balance response:', response);
      
      if (response.success) {
        const totalBalance = response.balance || 0;
        const blockedAmount = response.blockedAmount || 0;
        const availableBalance = Math.max(0, totalBalance - blockedAmount);
        
        console.log('Processing wallet data:', {
          totalBalance,
          blockedAmount,
          availableBalance,
          bidHistory: response.bidHistory
        });
        
        setWalletData({
          totalBalance: totalBalance,
          blockedBalance: blockedAmount,
          availableBalance: availableBalance,
          activeHighestBids: response.bidHistory || []
        });
        
        // Check for potential issues with blocked funds
        if (blockedAmount > totalBalance) {
          setError('Warning: Your blocked amount exceeds your total balance. ' +
                  'This is being corrected. Your available balance is set to zero until resolved.');
        } else {
          setError('');
        }
      } else {
        console.error('Failed to fetch balance:', response);
        setError(response.error || 'Failed to fetch wallet balance');
      }
    } catch (error) {
      console.error('Error fetching wallet balance:', error);
      setError(error.message || 'Error fetching wallet balance');
    } finally {
      setLoading(false);
    }
  };

  const handleAddFunds = async (e) => {
    e.preventDefault();
    
    if (!user?.id) {
      setError('Please log in to add funds.');
      return;
    }

    // Validate amount
    const amount = Math.round(parseFloat(addFundsAmount));
    if (!addFundsAmount || amount <= 0) {
      setError('Please enter a valid amount (must be a positive whole number)');
      return;
    }

    if (amount > 999999) {
      setError('Amount cannot exceed $999,999');
      return;
    }

    try {
      setLoading(true);
      setError('');
      setSuccess('');
      
      const response = await walletService.deposit({
        userId: user.id,
        amount: amount
      });

      if (response.success) {
        setSuccess('Funds added successfully!');
        setAddFundsAmount('');
        await fetchWalletBalance();
      } else {
        setError(response.error || 'Failed to add funds');
      }
    } catch (error) {
      console.error('Error adding funds:', error);
      setError(error.response?.data?.message || error.response?.data?.error || 'Error adding funds');
    } finally {
      setLoading(false);
    }
  };

  const handleWithdrawFunds = async (e) => {
    e.preventDefault();

    if (!user?.id) {
      setError('Please log in to withdraw funds.');
      return;
    }    const amount = Math.round(parseFloat(withdrawAmount));
    if (!withdrawAmount || amount <= 0) {
      setError('Please enter a valid amount (must be a positive whole number)');
      return;
    }    // Calculate the available balance, ensuring it's not negative
    const availableBalance = Math.max(0, walletData.totalBalance - walletData.blockedBalance);
    
    // Check if blocked amount exceeds total balance
    if (walletData.blockedBalance > walletData.totalBalance) {
      setError('Withdrawals are temporarily unavailable because your blocked amount exceeds your total balance.' +
               ' This issue is being resolved. Please try again later.');
      return;
    }    if (amount > availableBalance) {
      setError(`Insufficient available balance. You have $${availableBalance ? availableBalance.toFixed(2) : '0.00'} available ` +
               `(Total: $${walletData.totalBalance ? walletData.totalBalance.toFixed(2) : '0.00'}, ` + 
               `Blocked: $${walletData.blockedBalance ? walletData.blockedBalance.toFixed(2) : '0.00'})`);
      return;
    }

    if (amount > 999999) {
      setError('Amount cannot exceed $999,999');
      return;
    }

    try {
      setLoading(true);
      setError('');
      setSuccess('');
      
      const response = await walletService.withdraw({
        userId: user.id,
        amount: amount
      });

      if (response.success) {
        setSuccess('Funds withdrawn successfully!');
        setWithdrawAmount('');
        await fetchWalletBalance();
      } else {
        setError(response.error || 'Failed to withdraw funds');
      }
    } catch (error) {
      console.error('Error withdrawing funds:', error);
      setError(error.response?.data?.message || error.response?.data?.error || 'Error withdrawing funds');
    } finally {
      setLoading(false);
    }
  };

  // Don't render the wallet UI if there's no user
  if (!user?.id) {
    return (
      <div className="wallet-management">
        <div className="error-message">
          Please log in to access your wallet.
        </div>
      </div>
    );
  }

  return (
    <div className="wallet-management">
      <div className="wallet-header">
        <h1>Wallet Management</h1>
        <div className="wallet-balances">          <div className="balance-card">
            <h2>Total Balance</h2>
            <div className="balance-amount primary">
              ${walletData.totalBalance ? walletData.totalBalance.toFixed(2) : '0.00'}
            </div>
          </div><div className={`balance-card blocked ${walletData.blockedBalance > walletData.totalBalance ? 'error' : ''}`}>
            <h2>Blocked Amount</h2>
            <div className="balance-amount warning">
              ${walletData.blockedBalance ? walletData.blockedBalance.toFixed(2) : '0.00'}
            </div>
            <div className="blocked-info">
              Reserved for your highest bids
            </div>
            {walletData.blockedBalance > walletData.totalBalance && (
              <div className="warning-message">
                Warning: Blocked amount exceeds total balance. This will be corrected automatically.
              </div>
            )}
            {walletData.blockedBalance > 0 && walletData.activeHighestBids.length > 0 && (
              <div className="blocked-details">
                <h3>Active Bids</h3>
                {walletData.activeHighestBids.map(bid => (
                  <div key={bid.auctionId} className="bid-entry">                    <span className="bid-amount">${bid.bidAmount ? bid.bidAmount.toFixed(2) : '0.00'}</span>
                    <span className="bid-info">Auction #{bid.auctionId}</span>
                  </div>
                ))}
              </div>
            )}
          </div>
            <div className="balance-card available">
            <h2>Available Balance</h2>
            <div className="balance-amount success">
              ${walletData.availableBalance ? walletData.availableBalance.toFixed(2) : '0.00'}
            </div>
            <div className="balance-info">
              Available for new bids
            </div>
          </div>
        </div>
      </div>

      {error && <div className="error-message">{error}</div>}
      {success && <div className="success-message">{success}</div>}

      <div className="wallet-actions">
        <div className="action-card">
          <h3>Add Funds</h3>
          <form onSubmit={handleAddFunds}>
            <div className="form-group">
              <label htmlFor="addAmount">Amount ($)</label>
              <input
                type="number"
                id="addAmount"
                value={addFundsAmount}
                onChange={(e) => setAddFundsAmount(e.target.value)}
                placeholder="Enter amount to add"
                min="1"
                step="1"
                max="999999"
                required
              />
            </div>
            <button type="submit" disabled={loading} className="add-funds-btn">
              {loading ? 'Processing...' : 'Add Funds'}
            </button>
          </form>
        </div>

        <div className="action-card">
          <h3>Withdraw Funds</h3>
          <form onSubmit={handleWithdrawFunds}>
            <div className="form-group">
              <label htmlFor="withdrawAmount">Amount ($)</label>
              <input
                type="number"
                id="withdrawAmount"
                value={withdrawAmount}
                onChange={(e) => setWithdrawAmount(e.target.value)}
                placeholder="Enter amount to withdraw"
                min="1"
                step="1"
                max={walletData.availableBalance > 999999 ? 999999 : walletData.availableBalance}
                required
              />
            </div>
            <button type="submit" disabled={loading} className="withdraw-funds-btn">
              {loading ? 'Processing...' : 'Withdraw Funds'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
};

export default WalletManagement;
