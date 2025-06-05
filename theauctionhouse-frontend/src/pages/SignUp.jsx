import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { userService } from '../services/api';

const SignUp = () => {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
    const navigate = useNavigate();

  const validateForm = () => {
    if (!name.trim()) {
      setError('Name is required');
      return false;
    }
    if (!email.trim()) {
      setError('Email is required');
      return false;
    }
    if (!email.includes('@')) {
      setError('Please enter a valid email address');
      return false;
    }
    if (password.length < 6) {
      setError('Password must be at least 6 characters');
      return false;
    }
    if (password !== confirmPassword) {
      setError('Passwords do not match');
      return false;
    }
    return true;
  };  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    try {
      setError('');
      setLoading(true);

      // Log the request data
      console.log('Submitting signup form with:', {
        name: name.trim(),
        emailId: email.trim().toLowerCase(),
        passwordLength: password.length
      });

      // Make the signup request
      const result = await userService.signup({
        name: name.trim(),
        emailId: email.trim().toLowerCase(),
        password: password
      });      // Log the full result for debugging
      console.log('Signup result:', result);
      
      if (result && result.success) {
        console.log('Signup successful, navigating to login');
        // Clear form and navigate to login
        setName('');
        setEmail('');
        setPassword('');
        setConfirmPassword('');
        navigate('/login', { 
          replace: true,
          state: { message: result.message || 'Account created successfully! Please log in.' }
        });
      } else {
        // If success is false, there should be an error message
        console.error('Signup failed:', result);
        // Make sure we're extracting the error message correctly
        setError(result?.error || result?.message || 'Sign up failed. Please try again.');
      }
    } catch (err) {
      console.error('Unexpected signup error:', err);
      setError(err?.message || 'An unexpected error occurred.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container">
      <div className="card" style={{ maxWidth: '400px', margin: '2rem auto' }}>
        <div className="card-header">
          <h2>Sign Up</h2>
        </div>
        <div className="card-body">
          {error && (
            <div className="alert alert-error">
              {error}
            </div>
          )}
          <form onSubmit={handleSubmit}>
            <div className="form-group mb-3">
              <label htmlFor="name">Name</label>
              <input
                type="text"
                id="name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
                className="form-control"
              />
            </div>
            <div className="form-group mb-3">
              <label htmlFor="email">Email</label>
              <input
                type="email"
                id="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                className="form-control"
              />
            </div>
            <div className="form-group mb-3">
              <label htmlFor="password">Password</label>
              <input
                type="password"
                id="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                className="form-control"
              />
            </div>
            <div className="form-group mb-3">
              <label htmlFor="confirmPassword">Confirm Password</label>
              <input
                type="password"
                id="confirmPassword"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                required
                className="form-control"
              />
            </div>            <button type="submit" disabled={loading} className="btn btn-primary w-100">
              {loading ? 'Creating Account...' : 'Sign Up'}
            </button>
          </form>
          <div className="text-center mt-3">
            <p>Already have an account? <Link to="/login" className="text-primary">Login here</Link></p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default SignUp;
