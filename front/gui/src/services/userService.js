// src/services/userService.js (CAŁY PLIK)
import api from '../api';

export const userService = {
  getMe() {
    // GET /api/users/me
    return api.get('/users/me'); 
  },
  
  getUserById(id) {
    // GET /api/users/{id}
    return api.get(`/users/${id}`); 
  },
  
  getAllUsers() {
    return api.get('/users');
  }
};