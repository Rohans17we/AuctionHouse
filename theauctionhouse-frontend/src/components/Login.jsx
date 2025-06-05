import React, { useState } from 'react';
import { userService } from '../services/api';
import { useAuth } from '../contexts/AuthContext';
import './Login.css';

const Login = () => {
  const [formData, setFormData] = useState({
    emailId: '',
    password: '',
  });
  const [isSignUp, setIsSignUp] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [signupData, setSignupData] = useState({
    name: '',
    emailId: '',
    password: '',
  });

  const { login } = useAuth();

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    if (isSignUp) {
      setSignupData(prev => ({
        ...prev,
        [name]: value
      }));
    } else {
      setFormData(prev => ({
        ...prev,
        [name]: value
      }));
    }
  };
  const handleLogin = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {
      if (!formData.emailId || !formData.password) {
        setError('Email and password are required');
        return;
      }

      const result = await login(formData.emailId, formData.password);
      if (!result.success) {
        setError(result.error || 'Login failed. Please try again.');
        return;
      }

      // Login was successful, the AuthContext will handle redirecting
    } catch (error) {
      console.error('Login error:', error);
      setError(error.response?.data?.error || error.message || 'Login failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleSignUp = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {      const response = await userService.signup(signupData);
      if (response.data.success) {
        // Switch to login after successful signup        setIsSignUp(false);
        setFormData({
          emailId: signupData.emailId,
          password: signupData.password,
          name: signupData.name
        });
        // Automatically try to login with the signup credentials
        const loginResult = await login(signupData.emailId, signupData.password);
        if (!loginResult.success) {
          setError(loginResult.error || 'Login failed. Please try signing in again.');
        }
      }
    } catch (error) {
      setError(error.response?.data?.error || 'Sign up failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-form">
        <h2>{isSignUp ? 'Create Account' : 'The Auction House'}</h2>
        <p>{isSignUp ? 'Join The Auction House' : 'Sign in to your account'}</p>

        {error && <div className="error-message">{error}</div>}

        <form onSubmit={isSignUp ? handleSignUp : handleLogin}>
          {isSignUp && (
            <div className="form-group">
              <label htmlFor="name">Full Name</label>
              <input
                type="text"
                id="name"
                name="name"
                value={signupData.name}
                onChange={handleInputChange}
                required
                placeholder="Enter your full name"
              />
            </div>
          )}

          <div className="form-group">
            <label htmlFor="emailId">Email Address</label>
            <input
              type="email"
              id="emailId"
              name="emailId"
              value={isSignUp ? signupData.emailId : formData.emailId}
              onChange={handleInputChange}
              required
              placeholder="Enter your email"
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              type="password"
              id="password"
              name="password"
              value={isSignUp ? signupData.password : formData.password}
              onChange={handleInputChange}
              required
              placeholder="Enter your password"
            />
          </div>

          <button type="submit" disabled={loading} className="submit-btn">
            {loading ? 'Please wait...' : (isSignUp ? 'Create Account' : 'Sign In')}
          </button>
        </form>

        <div className="form-footer">
          <p>
            {isSignUp ? 'Already have an account?' : "Don't have an account?"}{' '}
            <button
              type="button"
              onClick={() => setIsSignUp(!isSignUp)}
              className="link-btn"
            >
              {isSignUp ? 'Sign In' : 'Sign Up'}
            </button>
          </p>
        </div>
      </div>
    </div>
  );
};

export default Login;
