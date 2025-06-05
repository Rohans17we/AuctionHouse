import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { assetService, auctionService } from '../services/api';
import { useNavigate } from 'react-router-dom';
import './AuctionCreation.css';

const AuctionCreation = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [userAssets, setUserAssets] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [formData, setFormData] = useState({
    assetId: '',
    reservePrice: '',
    minimumBidIncrement: '',
    totalMinutesToExpiry: ''
  });

  useEffect(() => {
    fetchUserAssets();
  }, []);

  const fetchUserAssets = async () => {
    try {
      setLoading(true);
      console.log('Fetching assets for user:', user?.id);
      const response = await assetService.getAll(user?.id);
      console.log('Asset response:', response);
      
      if (response.isSuccess) {
        // Debug log to see what statuses we're getting
        console.log('All assets returned from API:', response.data.map(asset => ({
          id: asset.assetId || asset.id,
          userId: asset.userId,
          title: asset.title,
          status: asset.status
        })));
        
        // Filter assets that are in Open status (case-insensitive)
        const openAssets = response.data.filter(asset => {
          const normalizedStatus = (asset.status || '').replace(/\s+/g, '').toLowerCase();
          return normalizedStatus === 'open';
        });
        
        // Debug log to see filtered assets
        console.log('Filtered open assets:', openAssets.map(asset => ({
          id: asset.assetId || asset.id,
          userId: asset.userId,
          title: asset.title,
          status: asset.status
        })));
        
        setUserAssets(openAssets);

        // Display appropriate message if no available assets
        if (openAssets.length === 0) {
          if (response.data.length > 0) {
            setError('You have assets but none are open for auction. Go to Asset Management to set assets to "Open" status first.');
          } else {
            setError('You don\'t have any assets yet. First create some assets in Asset Management.');
          }
        }
      } else {
        setError('Failed to fetch your assets');
      }
    } catch (error) {
      console.error('Asset fetch error details:', {
        message: error.message,
        status: error.response?.status,
        statusText: error.response?.statusText,
        data: error.response?.data,
        error: error
      });
      setError(`Error fetching assets: ${error.response?.data?.message || error.message || 'Unknown error'}`);
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleAssetChange = (e) => {    
    const assetId = e.target.value;    
    setFormData(prev => ({
      ...prev,
      assetId: assetId
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    const reservePrice = parseInt(formData.reservePrice);
    const minIncrement = parseInt(formData.minimumBidIncrement);
    const duration = parseInt(formData.totalMinutesToExpiry);

    // Validation checks
    if (!formData.assetId) {
      setError('Please select an asset');
      return;
    }

    // Reserve price validation ($1-$9999)
    if (reservePrice < 1 || reservePrice > 9999) {
      setError('Reserve price must be between $1 and $9,999');
      return;
    }

    // Minimum bid increment validation ($1-$999)
    if (minIncrement < 1 || minIncrement > 999) {
      setError('Minimum bid increment must be between $1 and $999');
      return;
    }

    // Duration validation (1-10080 minutes, 7 days)
    if (duration < 1 || duration > 10080) {
      setError('Duration must be between 1 minute and 10080 minutes (7 days)');
      return;
    }

    // Minimum bid increment must be less than reserve price
    if (minIncrement >= reservePrice) {
      setError('Minimum bid increment must be less than the reserve price');
      return;
    }

    // Minimum bid increment must be at least 1% of reserve price
    if (minIncrement < Math.max(1, Math.floor(reservePrice / 100))) {
      setError('Minimum bid increment must be at least 1% of the reserve price');
      return;
    }

    try {
      setLoading(true);
      setError('');
      setSuccess('');
      
      // Log the selected asset for debugging
      const selectedAsset = userAssets.find(asset => (asset.assetId || asset.id).toString() === formData.assetId);
      console.log('Selected asset:', selectedAsset);

      // Format the request according to PostAuctionRequest DTO
      const auctionData = {
        assetId: parseInt(formData.assetId),
        ownerId: user.id,
        reservedPrice: parseInt(formData.reservePrice),
        minimumBidIncrement: parseInt(formData.minimumBidIncrement),
        totalMinutesToExpiry: parseInt(formData.totalMinutesToExpiry),
        currentHighestBid: 0,
        currentHighestBidderId: 0
      };

      console.log('Creating auction with data:', auctionData);
      const response = await auctionService.create(auctionData);
      console.log('Auction creation response:', response);

      if (response.isSuccess) {
        setSuccess('Auction created successfully!');
        setTimeout(() => {
          navigate('/auctions');
        }, 1500);
      } else {
        setError(response.error || 'Failed to create auction');
      }
    } catch (error) {
      console.error('Auction creation error:', error);
      setError(error.response?.data?.message || error.message || 'Error creating auction');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auction-creation">
      <div className="auction-header">
        <h1>Create New Auction</h1>
        <p>List one of your assets for auction</p>
      </div>

      {error && <div className="error-message">{error}</div>}
      {success && <div className="success-message">{success}</div>}

      {loading ? (
        <div className="loading-message">Loading your assets...</div>
      ) : userAssets.length === 0 ? (
        <div className="no-assets-message">
          <p>{error || "You don't have any assets open for auction."}</p>
          <button 
            onClick={() => navigate('/assets')}
            className="create-asset-btn"
          >
            Go to Asset Management
          </button>
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="auction-form">
          <div className="form-section">
            <h2>Asset Selection</h2>
            <div className="form-group">
              <label htmlFor="assetId">Select Asset:</label>
              <select
                id="assetId"
                name="assetId"
                value={formData.assetId}
                onChange={handleAssetChange}
                required
              >
                <option value="">Select an asset...</option>
                {userAssets.map((asset) => (
                  <option key={asset.assetId || asset.id} value={asset.assetId || asset.id}>
                    {asset.title}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="form-section">
            <h2>Auction Settings</h2>
            <div className="form-group">
              <label htmlFor="reservePrice">Reserve Price ($):</label>
              <input
                type="number"
                id="reservePrice"
                name="reservePrice"
                value={formData.reservePrice}
                onChange={handleInputChange}
                min="1"
                max="9999"
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="minimumBidIncrement">Minimum Bid Increment ($):</label>
              <input
                type="number"
                id="minimumBidIncrement"
                name="minimumBidIncrement"
                value={formData.minimumBidIncrement}
                onChange={handleInputChange}
                min="1"
                max="999"
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="totalMinutesToExpiry">Duration (minutes):</label>
              <input
                type="number"
                id="totalMinutesToExpiry"
                name="totalMinutesToExpiry"
                value={formData.totalMinutesToExpiry}
                onChange={handleInputChange}
                min="1"
                max="10080"
                required
              />
            </div>
          </div>

          <div className="form-group">
            <button type="submit" disabled={loading}>
              {loading ? 'Creating...' : 'Create Auction'}
            </button>
          </div>
        </form>
      )}
    </div>
  );
};

export default AuctionCreation;
