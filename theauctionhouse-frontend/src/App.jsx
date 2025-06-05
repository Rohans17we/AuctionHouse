import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import PrivateRoute from './components/PrivateRoute';
import AdminRoute from './components/AdminRoute';
import Login from './components/Login';
import AdminLogin from './components/AdminLogin';
import Dashboard from './components/Dashboard';
import AdminDashboard from './components/AdminDashboard/AdminDashboard';
import Navigation from './components/Navigation';
import WalletManagement from './components/WalletManagement';
import AssetManagement from './components/AssetManagement';
import AuctionCreation from './components/AuctionCreation';
import AuctionDetails from './components/AuctionDetails';
import SignUp from './pages/SignUp';
import Footer from './components/Footer';
import './App.css';

function AppContent() {
  const { isAuthenticated, user } = useAuth();

  return (
    <div className="app">
      {isAuthenticated && <Navigation />}
      <main className="main-content">
        <Routes>
          <Route
            path="/signup"
            element={isAuthenticated ? <Navigate to="/dashboard" /> : <SignUp />}
          />
          <Route 
            path="/login" 
            element={isAuthenticated ? <Navigate to="/dashboard" /> : <Login />} 
          />
          <Route 
            path="/admin/login" 
            element={isAuthenticated ? <Navigate to="/admin/dashboard" /> : <AdminLogin />} 
          />
          <Route 
            path="/dashboard" 
            element={
              <PrivateRoute>
                <Dashboard />
              </PrivateRoute>
            } 
          />
          <Route 
            path="/admin/dashboard" 
            element={
              <AdminRoute>
                <AdminDashboard />
              </AdminRoute>
            } 
          />
          <Route 
            path="/wallet" 
            element={
              <PrivateRoute>
                <WalletManagement />
              </PrivateRoute>
            } 
          />
          <Route 
            path="/assets" 
            element={
              <PrivateRoute>
                <AssetManagement />
              </PrivateRoute>
            } 
          />
          <Route 
            path="/create-auction" 
            element={
              <PrivateRoute>
                <AuctionCreation />
              </PrivateRoute>
            } 
          />
          <Route 
            path="/auction/:id" 
            element={
              <PrivateRoute>
                <AuctionDetails />
              </PrivateRoute>
            } 
          />
          {/* Root and catch-all routes */}
          <Route 
            path="*" 
            element={
              isAuthenticated ? (
                <Navigate to={user?.isAdmin ? "/admin/dashboard" : "/dashboard"} />
              ) : (
                <Navigate to="/login" />
              )
            } 
          />
        </Routes>
      </main>
      {isAuthenticated && <Footer />}
    </div>
  );
}

function App() {
  return (
    <Router>
      <AuthProvider>
        <AppContent />
      </AuthProvider>
    </Router>
  );
}

export default App;
