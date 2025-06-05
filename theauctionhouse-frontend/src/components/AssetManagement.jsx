import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { assetService, auctionService } from '../services/api';
import './AssetManagement.css';

const AssetManagement = () => {
  const { user } = useAuth();
  const [assets, setAssets] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingAsset, setEditingAsset] = useState(null);
  
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    retailPrice: ''
  });

  useEffect(() => {
    fetchUserAssets();
  }, []);

  const fetchUserAssets = async () => {
    try {
      setLoading(true);
      // Ensure auction expiries are checked before fetching assets
      await auctionService.checkExpiries();
      const response = await assetService.getAll(user.id);
      if (response.isSuccess) {
        setAssets(response.data);
      } else {
        setError('Failed to fetch assets');
      }
    } catch (error) {
      setError('Error fetching assets');
      console.error('Error:', error);
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

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setLoading(true);
      setError('');
      setSuccess('');

      // Validate title length and characters
      const normalizedTitle = formData.title.trim().replace(/\s+/g, ' ');
      if (!normalizedTitle || normalizedTitle.length < 10 || normalizedTitle.length > 150) {
        setError('Title must be between 10 and 150 characters');
        setLoading(false);
        return;
      }

      // Validate title characters (alphanumeric only)
      if (!/^[a-zA-Z0-9\s]+$/.test(normalizedTitle)) {
        setError('Title can only contain letters, numbers, and spaces');
        setLoading(false);
        return;
      }

      // Validate description
      const normalizedDescription = formData.description.trim();
      if (!normalizedDescription || normalizedDescription.length < 10 || normalizedDescription.length > 1000) {
        setError('Description must be between 10 and 1000 characters');
        setLoading(false);
        return;
      }      // Validate retail price
      const retailPrice = parseInt(formData.retailPrice);
      if (isNaN(retailPrice) || retailPrice <= 0 || !Number.isInteger(retailPrice)) {
        setError('Retail price must be a positive whole number');
        setLoading(false);
        return;
      }
      if (retailPrice > 2147483647) {
        setError('Retail price is too large');
        setLoading(false);
        return;
      }

      const assetData = {
        title: normalizedTitle,
        description: normalizedDescription,
        retailPrice: retailPrice,
        ownerId: user.id
      };
      
      let response;
      if (editingAsset) {
        response = await assetService.update(editingAsset.assetId || editingAsset.id, assetData);
      } else {
        response = await assetService.create(assetData);
      }      if (response.isSuccess) {
        setSuccess(editingAsset ? `Asset "${normalizedTitle}" updated successfully!` : `New asset "${normalizedTitle}" created successfully!`);
        resetForm();
        await fetchUserAssets();
      } else {
        setError(response.error || 'Failed to save asset');
      }
    } catch (error) {
      console.error('Error:', error);
      setError(error.response?.data?.message || error.response?.data?.error || 'Error saving asset');
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (asset) => {
    // Only allow editing if status is Draft
    if (asset.status !== 'Draft') {
      setError('Only assets in Draft status can be edited');
      return;
    }
    
    setEditingAsset(asset);
    setFormData({
      title: asset.title,
      description: asset.description,
      retailPrice: asset.retailPrice || asset.retailValue || ''
    });
    setShowCreateForm(true);
  };
  const handleChangeStatus = async (asset) => {
    try {
      setLoading(true);
      setError('');
      
      if (asset.status === 'Draft') {        const response = await assetService.openToAuction(asset.assetId || asset.id);
        if (response.isSuccess) {
          setSuccess(`Asset "${asset.title}" is now open for auction. Go to Create Auction to start an auction with this asset.`);
          await fetchUserAssets();
        } else {
          setError(response.error || 'Failed to open asset for auction');
        }
      }
    } catch (error) {
      setError('Error changing asset status');
      console.error('Error:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (assetId) => {
    if (!assetId) {
      setError('Cannot delete asset: Invalid asset ID');
      return;
    }

    const asset = assets.find(a => (a.assetId || a.id) === assetId);
    if (!asset) {
      setError('Asset not found');      return;
    }
    
    // Only allow deletion for Draft or Open status
    if (asset.status !== 'Draft' && asset.status !== 'Open') {
      setError('Only assets in Draft or Open status can be deleted');
      return;
    }

    if (!window.confirm(`Are you sure you want to delete the asset "${asset.title}"? This action cannot be undone.`)) {
      return;
    }    try {
      setLoading(true);
      setError('');
        const response = await assetService.delete(assetId, user.id);
      if (response.isSuccess) {
        setSuccess(`Asset "${asset.title}" was deleted successfully!`);
        await fetchUserAssets();
      } else {
        setError(response.error || 'Failed to delete asset');
      }
    } catch (error) {
      console.error('Error deleting asset:', error);
      setError(error.response?.data?.message || error.response?.data?.error || 'Error deleting asset');
    } finally {
      setLoading(false);
    }
  };

  const resetForm = () => {
    setFormData({
      title: '',
      description: '',
      retailPrice: ''
    });
    setEditingAsset(null);
    setShowCreateForm(false);
  };  const canEditAsset = (asset) => asset.status === 'Draft';
  const canDeleteAsset = (asset) => asset.status === 'Draft' || asset.status === 'Open';
  const canOpenForAuction = (asset) => asset.status === 'Draft';

  return (
    <div className="asset-management">
      <div className="asset-header">
        <h1>My Assets</h1>
        <button 
          className="create-asset-btn"
          onClick={() => setShowCreateForm(!showCreateForm)}
        >
          {showCreateForm ? 'Cancel' : 'Add New Asset'}
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}
      {success && <div className="success-message">{success}</div>}

      {showCreateForm && (
        <div className="asset-form-container">
          <h2>{editingAsset ? 'Edit Asset' : 'Create New Asset'}</h2>
          <form onSubmit={handleSubmit} className="asset-form">
            <div className="form-group">
              <label htmlFor="title">Title</label>
              <input
                type="text"
                id="title"
                name="title"
                value={formData.title}
                onChange={handleInputChange}
                required
                minLength={10}
                maxLength={150}
                pattern="^[a-zA-Z0-9\s]+$"
                title="Title must be between 10-150 characters and can only contain letters, numbers, and spaces"
              />
            </div>

            <div className="form-group">
              <label htmlFor="description">Description</label>
              <textarea
                id="description"
                name="description"
                value={formData.description}
                onChange={handleInputChange}
                required
                minLength={10}
                maxLength={1000}
                rows={4}
              />
            </div>

            <div className="form-group">              <label htmlFor="retailPrice">Retail Value</label>
              <input
                type="number"
                id="retailPrice"
                name="retailPrice"
                value={formData.retailPrice}
                onChange={handleInputChange}
                required
                min={1}
                max={2147483647}
                step={1}
                title="Please enter a positive whole number"
                pattern="\d+"
              />
            </div>

            <div className="form-actions">
              <button type="submit" disabled={loading} className="submit-btn">
                {loading ? 'Saving...' : (editingAsset ? 'Update Asset' : 'Create Asset')}
              </button>
              <button type="button" onClick={resetForm} className="cancel-btn">
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      <div className="assets-grid">
        {loading && <div className="loading">Loading assets...</div>}        {!loading && assets.length === 0 && (
          <div className="no-assets">
            <p>You don't have any assets yet. Click the <strong>"Add New Asset"</strong> button above to get started!</p>
            <button 
              className="create-asset-btn no-assets-btn"
              onClick={() => setShowCreateForm(true)}
            >
              Add New Asset
            </button>
          </div>
        )}
        {assets.map(asset => (
          <div key={asset.id || asset.assetId} className="asset-card">
            <div className="asset-content">
              <div className={`asset-status ${asset.status}`}>{asset.status}</div>
              <h3>{asset.title}</h3>
              <p className="asset-retail-value">Retail Value: ${asset.retailPrice || asset.retailValue}</p>
              <p className="asset-description">{asset.description}</p>
              
              <div className="asset-actions">
                {canEditAsset(asset) && (
                  <button 
                    onClick={() => handleEdit(asset)}
                    className="edit-btn"
                  >
                    Edit
                  </button>
                )}
                
                {canOpenForAuction(asset) && (
                  <button 
                    onClick={() => handleChangeStatus(asset)}
                    className="open-auction-btn"
                  >
                    Open for Auction
                  </button>
                )}
                
                {canDeleteAsset(asset) && (
                  <button 
                    onClick={() => handleDelete(asset.assetId || asset.id)}
                    className="delete-btn"
                  >
                    Delete
                  </button>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default AssetManagement;
