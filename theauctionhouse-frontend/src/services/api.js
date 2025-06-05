import axios from 'axios';
import authService from './authService';

const API_BASE_URL = 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to requests if available
api.interceptors.request.use(
  (config) => {
    const token = authService.getAuthToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Handle 401 responses
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      authService.clearAuth();
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export const userService = {
  async login(credentials) {
    try {
      console.log('Sending login request:', { emailId: credentials.emailId });
      const response = await api.post('/User/login', {
        emailId: credentials.emailId,
        password: credentials.password
      });
      console.log('Login response:', response.data);
      
      // The API returns the data directly
      return response.data;
    } catch (error) {
      console.error('Login error details:', {
        status: error.response?.status,
        data: error.response?.data,
        message: error.message
      });
      throw error;
    }
  },  
  async signup(userData) {
    try {
      console.log('Sending signup request:', { ...userData, password: '[REDACTED]' });
      const response = await api.post('/User/signup', userData);
      console.log('Signup response:', response.data);
      // Backend is sending { success: true, message: "User registered successfully" }
      // Just return it directly as it's already in the correct format
      return response.data;
    } catch (error) {
      console.error('Signup error details:', {
        status: error.response?.status,
        data: error.response?.data,
        message: error.message
      });      // Try to extract the most specific error message from the backend
      let errorMsg = error.response?.data?.error || error.response?.data?.message || error.message || 'Failed to create account';
      
      // If there are validation errors, append them
      if (error.response?.data?.validationErrors && Array.isArray(error.response.data.validationErrors)) {
        errorMsg += '\n' + error.response.data.validationErrors.map(v => v.ErrorMessage || v).join('\n');
      }
      
      console.log('Formatted error response:', { success: false, error: errorMsg });
      
      return {
        success: false,
        error: errorMsg
      };
    }
  },
  async logout(userId) {
    if (!userId) return { success: true }; // Already logged out
    try {
      const token = authService.getAuthToken();
      if (!token) return { success: true }; // No active session

      const response = await api.post(`/User/logout/${userId}`);
      return response.data;
    } catch (error) {
      console.error('Logout error:', error.response?.data || error.message);
      // Even if the API call fails, we want to clear local auth
      return { success: true };
    } finally {
      // Always clear local auth state
      authService.clearAuth();
    }
  }
};

export const assetAPI = {
  getAll: async (userId) => {
    try {
      if (!userId) {
        console.error('getAll: No userId provided');
        return {
          isSuccess: false,
          error: 'User ID is required',
          data: []
        };
      }
      const response = await api.get(`/Asset?userId=${userId}`);
      return {
        isSuccess: true,
        data: response.data
      };
    } catch (error) {
      console.error('getAll error:', {
        status: error.response?.status,
        data: error.response?.data,
        message: error.message
      });
      return {
        isSuccess: false,
        error: error.response?.data?.message || error.message,
        data: []
      };
    }
  },  getById: (id) => api.get(`/Asset/${id}`),  create: async (assetData) => {
    try {
      console.log('Creating asset:', assetData);
      
      // Convert camelCase to PascalCase for C# DTO
      const formattedData = {
        OwnerId: assetData.ownerId,
        Title: assetData.title,
        Description: assetData.description,
        RetailPrice: assetData.retailPrice
      };
      
      console.log('Formatted asset data:', formattedData);
      const response = await api.post('/Asset', formattedData);
      console.log('Asset creation response:', response.data);
      return {
        isSuccess: true,
        data: response.data
      };
    } catch (error) {
      console.error('Asset creation error:', {
        status: error.response?.status,
        data: error.response?.data,
        message: error.message
      });
      return {
        isSuccess: false,
        error: error.response?.data?.message || error.message || 'Failed to create asset'
      };
    }  },
  update: (id, assetData) => api.put(`/Asset/${id}`, assetData),
  delete: (id, userId) => api.delete(`/Asset/${id}?userId=${userId}`),
  openToAuction: async (assetId) => {
    try {
      if (!assetId) {
        console.error('openToAuction: No assetId provided');
        return {
          isSuccess: false,
          error: 'Asset ID is required'
        };
      }
      const response = await api.post(`/Asset/${assetId}/open-to-auction`);
      return {
        isSuccess: true,
        data: response.data
      };
    } catch (error) {
      console.error('openToAuction error:', {
        status: error.response?.status,
        data: error.response?.data,
        message: error.message
      });
      return {
        isSuccess: false,
        error: error.response?.data?.message || error.message
      };
    }
  },
};

export const auctionAPI = {
  getById: (id) => api.get(`/Auction/${id}`),
  create: async (auctionData) => {
    try {
      console.log('Sending auction creation request:', auctionData);
      const response = await api.post('/Auction', auctionData);
      console.log('Auction creation response:', response.data);
      return {
        isSuccess: true,
        data: response.data
      };
    } catch (error) {
      const errorDetails = {
        message: error.message,
        response: {
          status: error.response?.status,
          statusText: error.response?.statusText,
          data: error.response?.data
        }
      };
      console.error('Auction creation error details:', errorDetails);

      // Try to extract a meaningful error message
      let errorMessage = error.response?.data?.message;
      if (!errorMessage && error.response?.data) {
        // Handle possible validation errors array
        if (Array.isArray(error.response.data)) {
          errorMessage = error.response.data[0]?.message;
        } else if (typeof error.response.data === 'object') {
          // Try to find error message in object
          const possibleMessages = ['error', 'message', 'Message', 'Error'];
          for (let key of possibleMessages) {
            if (error.response.data[key]) {
              errorMessage = error.response.data[key];
              break;
            }
          }
        }
      }

      return {
        isSuccess: false,
        error: errorMessage || error.message || 'An error occurred while creating the auction',
        errorDetails
      };
    }
  },
  checkExpiries: () => api.post('/Auction/check-expiries'),
};

export const bidAPI = {
  placeBid: (bidData) => api.post('/Bid', bidData),
  getBidHistory: (auctionId) => api.get(`/Bid/auction/${auctionId}`),
  getUserBidHistory: (userId) => api.get(`/Bid/user/${userId}`),
  unblockBids: (auctionId) => api.post(`/Bid/unblock/${auctionId}`),
};

export const walletService = {
  async getBalance(userId) {
    if (!userId) {
      console.error('getBalance: No userId provided');
      return {
        success: false,
        balance: 0,
        blockedAmount: 0,
        bidHistory: [],
        error: 'User ID is required'
      };
    }

    try {
      console.log('Getting balance for user:', userId);
      const response = await api.get(`/Wallet/balance/${userId}`);
      console.log('Raw balance response:', response);
      
      if (!response.data) {
        console.error('No data in balance response');
        throw new Error('No data received from server');
      }

      // Log the response structure to help debug
      console.log('Balance response structure:', {
        hasAmount: 'amount' in response.data,
        amount: response.data.amount,
        blockedAmount: response.data.blockedAmount,
        hasBidHistory: Array.isArray(response.data.bidHistory)
      });
      
      return {
        success: true,
        balance: Number(response.data.amount ?? 0),
        blockedAmount: Number(response.data.blockedAmount ?? 0),
        bidHistory: response.data.bidHistory || [],
        error: null
      };
    } catch (error) {
      console.error('Get balance error:', {
        error,
        response: error.response,
        status: error.response?.status,
        data: error.response?.data
      });
      
      const errorMessage = 
        error.response?.data?.message ||
        error.response?.data?.error ||
        error.response?.data ||
        error.message ||
        'Failed to fetch balance';

      return {
        success: false,
        balance: 0,
        blockedAmount: 0,
        bidHistory: [],
        error: errorMessage
      };
    }
  },
  async deposit(data) {
    try {
      if (!data.userId) {
        throw new Error('User ID is required');
      }
      const response = await api.post('/Wallet/deposit', {
        userId: data.userId,
        amount: Math.round(Number(data.amount))
      });
      console.log('Deposit response:', response.data);
      return response.data;
    } catch (error) {
      console.error('Deposit error:', error.response?.data || error.message);
      return {
        success: false,
        error: error.response?.data?.message || error.response?.data?.error || error.message || 'Failed to deposit funds'
      };
    }
  },

  async withdraw(data) {
    try {
      const response = await api.post('/Wallet/withdraw', data);
      console.log('Withdraw response:', response.data);
      return response.data;
    } catch (error) {
      console.error('Withdraw error:', error.response?.data || error);
      return {
        success: false,
        error: error.response?.data?.message || error.response?.data || 'Failed to withdraw funds'
      };
    }
  },
};

export const dashboardAPI = {
  getDashboard: (userId) => api.get(`/Dashboard/${userId}`),
  getLiveAuctions: () => api.get('/Dashboard/live-auctions'),
  getUserHighestBids: (userId) => api.get(`/Dashboard/user-highest-bids/${userId}`),
};

export const adminAPI = {
  getUsers: () => api.get('/Admin/users'),
  resetPassword: (resetData) => api.post('/Admin/reset-password', resetData),
  getAuditLogs: () => api.get('/Admin/audit-logs'),
  getUserAuditLog: (userId) => api.get(`/Admin/audit-log/${userId}`),
  manageUserAccess: (request) => api.post('/Admin/manage-user-access', request),
  deleteUser: (userId) => api.delete(`/Admin/user/${userId}`),
  getLiveAuctions: () => api.get('/Admin/live-auctions'),
};

export const assetService = assetAPI;
export const auctionService = auctionAPI;
export const bidService = bidAPI;
export default api;
