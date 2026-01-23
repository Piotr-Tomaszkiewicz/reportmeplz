import api from '../api';

export const locationService = {
  // GET: Pobieranie wszystkich
  getLocations() {
    return api.get('/locations');
  },
  
  // POST: Dodawanie nowej lokalizacji
  createLocation(data) {
    // data: { shortName, fullAddress }
    return api.post('/locations', data);
  },
  
  // PUT: Aktualizacja istniejącej
  updateLocation(id, data) {
    // data: { id, shortName, fullAddress }
    return api.put(`/locations/${id}`, data);
  },

  // DELETE: Usuwanie lokalizacji
  deleteLocation(id) {
    return api.delete(`/locations/${id}`);
  }
};