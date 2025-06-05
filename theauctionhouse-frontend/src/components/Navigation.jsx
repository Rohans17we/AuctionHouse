import React from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { userService } from '../services/api';
import './Navigation.css';

const Navigation = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();  const handleLogout = async () => {
    try {
      await userService.logout(user?.id);
    } finally {
      // Always logout locally and redirect
      logout();
      // Replace history state to prevent going back to authenticated routes
      navigate('/login', { replace: true });
    }
  };

  return (
    <nav className="navigation">
      <div className="nav-container">
        <div className="nav-brand">
          <h2>TheAuctionHouse</h2>
        </div>
          <div className="nav-links">          {!user?.isAdmin ? (
            <>
              <NavLink 
                to="/dashboard" 
                className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}
              >
                Dashboard
              </NavLink>
              <NavLink 
                to="/wallet" 
                className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}
              >
                Wallet
              </NavLink>
              <NavLink 
                to="/assets" 
                className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}
              >
                My Assets
              </NavLink>
              <NavLink 
                to="/create-auction" 
                className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}
              >
                Create Auction
              </NavLink>
            </>
          ) : (
            <NavLink 
              to="/admin/dashboard" 
              className={({ isActive }) => isActive ? 'nav-link active admin-link' : 'nav-link admin-link'}
            >
              Admin Dashboard
            </NavLink>
          )}
        </div><div className="nav-user">
          <span className="user-welcome">Hello, {user?.name || user?.firstName || 'User'}</span>
          <button onClick={handleLogout} className="logout-btn">
            Logout
          </button>
        </div>
      </div>
    </nav>
  );
};

export default Navigation;
