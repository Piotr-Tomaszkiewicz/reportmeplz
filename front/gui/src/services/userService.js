import api from '../api';

export const userService = {
  getMe() {
    return api.get('/users/me'); 
  },
  
  getUserById(id) {
    return api.get(`/users/${id}`); 
  },
  
  getAllUsers() {
    return api.get('/users');
  },

  // NOWA METODA: Aktualizacja danych użytkownika przez Admina
  updateUser(id, userData) {
    // userData: { login, email, locationId, roleId, password }
    return api.put(`/users/${id}`, userData);
  },

  // Opcjonalnie: Usuwanie użytkownika
  deleteUser(id) {
    return api.delete(`/users/${id}`);
  }
};