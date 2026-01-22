import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5016/api',
});

// Interceptor: Ten kod uruchamia się PRZED każdym zapytaniem
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  
  if (token) {
    // Ważne: Sprawdź czy nie masz literówki w "Bearer " (spacja na końcu!)
    config.headers.Authorization = `Bearer ${token}`;
    console.log('Token wysłany:', token); 
  } else {
    console.log('Brak tokena w localStorage - zapytanie anonimowe');
  }
  
  return config;
}, (error) => {
  return Promise.reject(error);
});

export default api;