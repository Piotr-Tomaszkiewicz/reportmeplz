import api from '../api';

export const authService = {
  login(credentials) {
    // credentials: { login, password }
    return api.post('/auth/login', credentials);
  },
  register(userData) {
    // userData: { login, email, password, locationId, roleId }
    return api.post('/auth/register', userData);
  }
};