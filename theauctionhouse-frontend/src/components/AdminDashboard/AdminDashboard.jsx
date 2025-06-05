import React, { useState, useEffect } from 'react';
import { adminAPI } from '../../services/api';
import './AdminDashboard.css';

const AdminDashboard = () => {
  const [users, setUsers] = useState([]);
  const [liveAuctions, setLiveAuctions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedUser, setSelectedUser] = useState(null);
  const [auditLogs, setAuditLogs] = useState(null);
  const [stats, setStats] = useState({
    totalUsers: 0,
    activeUsers: 0,
    blockedUsers: 0,
    totalAuctions: 0
  });

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      setLoading(true);      const [usersResponse, auctionsResponse] = await Promise.all([
        adminAPI.getUsers(),
        adminAPI.getLiveAuctions()
      ]);
      const userData = usersResponse.data?.data || [];
      const auctionsData = auctionsResponse.data?.data || [];
      
      setUsers(userData);
      setLiveAuctions(auctionsData);
      
      // Update stats
      setStats({
        totalUsers: userData.length,
        activeUsers: userData.filter(user => !user.isBlocked).length,
        blockedUsers: userData.filter(user => user.isBlocked).length,
        totalAuctions: auctionsData.length
      });
      
      setError('');
    } catch (error) {
      console.error('Error loading admin dashboard:', error);
      setError('Failed to load dashboard data. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleBlockUser = async (userId, isBlocked) => {
    try {
      const action = isBlocked ? 'Allow' : 'Block';
      await adminAPI.manageUserAccess({ userId, action });
      await loadDashboardData(); // Refresh data
      alert(`User has been ${action.toLowerCase()}ed successfully`);
    } catch (error) {
      console.error('Error managing user access:', error);
      alert('Failed to update user access. Please try again.');
    }
  };
  const handleDeleteUser = async (userId) => {
    if (!window.confirm('Are you sure you want to delete this user? This action cannot be undone.')) {
      return;
    }

    try {
      const response = await adminAPI.deleteUser(userId);
      if (response && response.success) {
        await loadDashboardData(); // Refresh data after deletion
        alert('User has been deleted successfully');
      } else {
        throw new Error(response.message || 'Failed to delete user');
      }
    } catch (error) {
      console.error('Error deleting user:', error);
      const errorMessage = error.response?.data?.message || error.message || 'Failed to delete user. Please try again.';
      
      // Show a more user-friendly error message
      let displayMessage = errorMessage;
      if (errorMessage.includes('blocked funds')) {
        displayMessage = 'Cannot delete user because they have blocked funds in active bids. Please wait for their auctions to complete.';
      } else if (errorMessage.includes('active bids')) {
        displayMessage = 'Cannot delete user because they have active bids in auctions. Please wait for the auctions to complete.';
      } else if (errorMessage.includes('funds in wallet')) {
        displayMessage = 'Cannot delete user because they have funds in their wallet. Please ensure they withdraw all funds first.';
      }
      
      alert(displayMessage);
    }
  };
  const handleViewAudit = async (userId) => {
    try {
      setSelectedUser(users.find(u => u.id === userId));
      const response = await adminAPI.getUserAuditLog(userId);
      console.log('Audit log response:', response); // Debug log
      setAuditLogs(response.data);
    } catch (error) {
      console.error('Error fetching audit log:', error);
      alert(error.response?.data?.message || 'Failed to fetch audit log. Please try again.');
      setSelectedUser(null);
      setAuditLogs(null);
    }
  };

  // Function to generate a secure random password
  const generateSecurePassword = () => {
    const length = 12;
    const charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
    let password = "";
    for (let i = 0; i < length; i++) {
      const randomIndex = Math.floor(Math.random() * charset.length);
      password += charset[randomIndex];
    }
    return password;
  };

  const handleResetPassword = async (userId) => {
    if (!window.confirm('Are you sure you want to reset this user\'s password?')) {
      return;
    }

    try {
      const newPassword = generateSecurePassword();
      await adminAPI.resetPassword({ 
        userId, 
        newPassword 
      });
      alert(`Password has been reset successfully. \nNew password: ${newPassword}`);
    } catch (error) {
      console.error('Error resetting password:', error);
      alert(error.response?.data?.message || 'Failed to reset password. Please try again.');
    }
  };

  if (loading) {
    return <div className="admin-dashboard-loading">Loading dashboard data...</div>;
  }

  return (
    <div className="admin-dashboard">
      <h1>Admin Dashboard</h1>
      
      {error && <div className="error-message">{error}</div>}

      <div className="admin-stats">
        <div className="stat-card">
          <h3>{stats.totalUsers}</h3>
          <p>Total Users</p>
        </div>
        <div className="stat-card">
          <h3>{stats.activeUsers}</h3>
          <p>Active Users</p>
        </div>
        <div className="stat-card">
          <h3>{stats.blockedUsers}</h3>
          <p>Blocked Users</p>
        </div>
        <div className="stat-card">
          <h3>{stats.totalAuctions}</h3>
          <p>Active Auctions</p>
        </div>
      </div>

      <section className="users-section">
        <h2>Platform Users (Regular Users Only)</h2>
        <div className="users-table-container">
          <table className="users-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Wallet Balance</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {users.map(user => (
                <tr key={user.id}>
                  <td>{user.name}</td>
                  <td>{user.emailId}</td>
                  <td>${(user.walletBalance || 0).toFixed(2)}</td>
                  <td>
                    <span className={`status-badge ${user.isBlocked ? 'blocked' : 'active'}`}>
                      {user.isBlocked ? 'Blocked' : 'Active'}
                    </span>
                  </td>
                  <td className="action-buttons">
                    <button 
                      onClick={() => handleBlockUser(user.id, user.isBlocked)}
                      className={`action-btn ${user.isBlocked ? 'unblock' : 'block'}`}
                    >
                      {user.isBlocked ? 'Unblock' : 'Block'} User
                    </button>
                    <button 
                      onClick={() => handleDeleteUser(user.id)}
                      className="action-btn delete"
                    >
                      Delete User
                    </button>
                    <button 
                      onClick={() => handleViewAudit(user.id)}
                      className="action-btn audit"
                    >
                      View Audit Log
                    </button>
                    <button 
                      onClick={() => handleResetPassword(user.id)}
                      className="action-btn reset-password"
                    >
                      Reset Password
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      {selectedUser && auditLogs && (
        <section className="user-audit-section">
          <h2>Audit Log - {selectedUser.name}</h2>
          <div className="audit-log-container">
            <div className="user-info">
              <div className="user-details">
                <p><strong>Email:</strong> {selectedUser.emailId}</p>
                <p><strong>Last Login:</strong> {new Date(auditLogs.lastLoginDate).toLocaleString()}</p>
              </div>
              <div className="wallet-details">
                <p><strong>Wallet Balance:</strong> ${(selectedUser.walletBalance || 0).toFixed(2)}</p>
                <p><strong>Blocked Amount:</strong> ${(selectedUser.walletBalanceBlocked || 0).toFixed(2)}</p>
              </div>
              <div className="status-info">
                <p>
                  <strong>Status:</strong> 
                  <span className={`status-badge ${selectedUser.isBlocked ? 'blocked' : 'active'}`}>
                    {selectedUser.isBlocked ? 'Blocked' : 'Active'}
                  </span>
                </p>
              </div>
            </div>
            <div className="activity-log">
              <h3>Activity History</h3>
              {auditLogs.activityLog?.length > 0 ? (
                <ul>
                  {auditLogs.activityLog.map((activity, index) => (
                    <li key={index} className="activity-item">
                      {activity}
                    </li>
                  ))}
                </ul>
              ) : (
                <p className="no-activities">No activities recorded</p>
              )}
            </div>
          </div>
          <button 
            className="close-audit-btn" 
            onClick={() => {
              setSelectedUser(null);
              setAuditLogs(null);
            }}
          >
            Close Audit Log
          </button>
        </section>
      )}

      <section className="live-auctions-section">
        <h2>Live Auctions</h2>
        <div className="auctions-grid">
          {liveAuctions.map(auction => (
            <div key={auction.id} className="auction-card">
              <div className="auction-header">
                <h3>{auction.title}</h3>
                <span className="bid-count">{auction.bidCount} bids</span>
              </div>              <div className="auction-details">
                <p>Seller: {auction.userId}</p>
                <p>Current Bid: ${auction.currentHighestBid}</p>
                <p>Reserved Price: ${auction.reservedPrice}</p>
                <p>Start Date: {new Date(auction.startDate).toLocaleString()}</p>
              </div>
              <div className="auction-status">
                Time Remaining: {auction.totalMinutesToExpiry} minutes
              </div>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
};

export default AdminDashboard;
