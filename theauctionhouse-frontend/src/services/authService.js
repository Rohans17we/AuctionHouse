// Storage keys
const TOKEN_KEY = 'auth_token';
const USER_KEY = 'user_info';

class AuthService {
  setAuthToken(token) {
    localStorage.setItem(TOKEN_KEY, token);
  }

  getAuthToken() {
    return localStorage.getItem(TOKEN_KEY);
  }

  removeAuthToken() {
    localStorage.removeItem(TOKEN_KEY);
  }

  setUserInfo(user) {
    localStorage.setItem(USER_KEY, JSON.stringify(user));
  }

  getUserInfo() {
    const userInfo = localStorage.getItem(USER_KEY);
    return userInfo ? JSON.parse(userInfo) : null;
  }

  removeUserInfo() {
    localStorage.removeItem(USER_KEY);
  }

  isAuthenticated() {
    return !!this.getAuthToken();
  }

  clearAuth() {
    this.removeAuthToken();
    this.removeUserInfo();
  }
}

export default new AuthService();
