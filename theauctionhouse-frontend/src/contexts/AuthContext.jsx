import React, { createContext, useContext, useState, useEffect } from 'react';
import { userService } from '../services/api';
import authService from '../services/authService';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Check for existing auth on mount
    const storedUser = authService.getUserInfo();
    if (storedUser) {
      setUser(storedUser);
    }
    setLoading(false);
  }, []);  const login = async (email, password) => {
    try {
      console.log('Login attempt with:', { email, passwordLength: password?.length });
      const response = await userService.login({ 
        emailId: email,
        password: password
      });
      console.log('Login response in AuthContext:', response);
      
      if (response && response.token) {
        // Build standard user object
        const user = {
          id: response.user?.id,
          emailId: response.user?.email,
          name: response.user?.name,
          isAdmin: response.user?.isAdmin || false,
        };
        authService.setAuthToken(response.token);
        authService.setUserInfo(user);
        setUser(user);
        return { success: true };
      }
      return { 
        success: false, 
        error: response.message || response.error || 'Login failed' 
      };
    } catch (error) {
      console.error('Login error in AuthContext:', error);
      return { 
        success: false,
        error: error.response?.data?.message || error.response?.data?.error || 'An error occurred during login'
      };
    }
  };

  const signup = async (name, email, password) => {
    try {
      const response = await userService.signup({ name, emailId: email, password });
      return { success: response.success, error: response.error };
    } catch (error) {
      return { 
        success: false, 
        error: error.response?.data?.error || 'An error occurred during signup' 
      };
    }
  };

  const logout = async () => {
    try {
      if (user) {
        await userService.logout(user.id);
      }
      setUser(null);
      authService.clearAuth();
      return { success: true };
    } catch (error) {
      return { 
        success: false, 
        error: error.response?.data?.error || 'An error occurred during logout' 
      };
    }
  };

  const value = {
    user,
    login,
    signup,
    logout,
    isAuthenticated: !!user
  };

  if (loading) {
    return <div>Loading...</div>;
  }

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
