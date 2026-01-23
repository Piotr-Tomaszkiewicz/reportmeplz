import { defineStore } from 'pinia';
import { userService } from '../services/userService';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    userProfile: null,
    loadingProfile: false,
    isLoggedIn: !!localStorage.getItem('token'),
  }),
  
  actions: {
    async fetchUserProfile() {
      if (!this.isLoggedIn) {
        this.userProfile = null;
        return;
      }

      this.loadingProfile = true;
      try {
        const response = await userService.getMe();
        
        const data = response.data;

        this.userProfile = {
          // KLUCZOWY ZAPIS ID (Zakładam, że API /users/me zwraca 'id' lub 'Id')
          id: data.userId || data.ID || data.id, 
          login: data.login || data.Login,
          email: data.email || data.Email || 'Brak Emaila',
          location: data.locationShortName || data.LocationShortName || 'Brak Lokalizacji',
        };
        
        localStorage.setItem('userLogin', this.userProfile.login);
        localStorage.setItem('role', data.role || data.Role || 'user');
        
        // KLUCZOWY ZAPIS: ID użytkownika musi być w LocalStorage
        if (this.userProfile.id) {
            localStorage.setItem('userId', this.userProfile.id);
        }

      } catch (error) {
        console.error("Błąd ładowania profilu użytkownika:", error);
        this.logout(); 
      } finally {
        this.loadingProfile = false;
      }
    },

    logout() {
      localStorage.removeItem('token');
      localStorage.removeItem('userLogin');
      localStorage.removeItem('role');
      localStorage.removeItem('userId'); // Usuwamy ID
      this.userProfile = null;
      this.isLoggedIn = false;
      window.location.reload(); 
    }
  },
  
  getters: {
    role: (state) => localStorage.getItem('role') || 'guest'
  }
});