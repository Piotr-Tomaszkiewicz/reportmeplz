import api from '../api';

export const locationService = {
  getLocations() {
    return api.get('/locations');
  }
};