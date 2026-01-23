<template>
  <nav class="sidebar">
    <div class="sidebar-top">
      <div class="logo-container">
        <h1 class="logo-text">REPORTMEPLZ</h1>
      </div>

      <div class="nav-links">
        <router-link to="/moje-zgloszenia" class="nav-item">
          <span class="icon">📋</span> Moje zgłoszenia
        </router-link>
        
        <!-- LINK WIDOCZNY TYLKO DLA ADMIN/MANAGERA -->
        <router-link 
          v-if="canViewAllReports" 
          to="/wszystkie-zgloszenia" 
          class="nav-item"
        >
          <span class="icon">🌍</span> Wszystkie zgłoszenia
        </router-link>
        
        <button type="button" @click="$emit('open-report')" class="nav-item btn-report">
          <span class="icon">➕</span> Zgłoś
        </button>
      </div>
    </div>

    <div class="sidebar-bottom">
      <!-- Używamy click.prevent, by zatrzymać propagację, jeśli jest to button/div -->
      <div v-if="isLoggedIn" class="user-control clickable" @click="$emit('open-profile', currentUserId)">
        <div class="user-badge">👤 {{ login }} ({{ userRole }})</div>
        <button @click.stop="handleLogout" class="nav-item logout-link">
          Wyloguj
        </button>
      </div>
      
      <button v-else @click="$emit('open-login')" class="nav-item login-link">
        <span class="icon">👤</span> Zaloguj
      </button>
    </div>
  </nav>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useAuthStore } from '../stores/authStore' // Importujemy store

const authStore = useAuthStore()

// Dodajemy nowy emit 'open-profile'
defineEmits(['open-report', 'open-login', 'open-profile'])

const isLoggedIn = ref(false)
const login = ref('')
const userRole = ref('')

onMounted(() => {
  const token = localStorage.getItem('token')
  if (token) {
    isLoggedIn.value = true
    // Login i rola mogą być pobrane z LocalStorage lub Pinia
    login.value = localStorage.getItem('userLogin') || 'Użytkownik'
    userRole.value = (localStorage.getItem('role') || 'user').toLowerCase()
  }
})

// KLUCZOWA ZMIANA: Pobieramy ID z Pinia, gdzie jest ono ładowane przez fetchUserProfile
const currentUserId = computed(() => {
    // Zakładamy, że ID jest zapisane w LocalStorage pod kluczem 'userId'
    // A Pinia je odświeża.
    return localStorage.getItem('userId'); 
    
    // UWAGA: Jeśli API w /users/me nie zwraca 'userId', musisz je zapisać 
    // z tokena JWT przy logowaniu LUB zmusić API do zwrócenia go.
});

const canViewAllReports = computed(() => {
  if (!isLoggedIn.value) return false
  const role = userRole.value.toLowerCase()
  return role === 'admin' || role === 'manager'
})

const handleLogout = () => {
  authStore.logout() // Używamy metody wylogowania z Pinia
}
</script>

<style scoped>
.sidebar { width: 240px; min-width: 240px; height: 100vh; background-color: #ffffff; border-right: 1px solid #e5e7eb; display: flex; flex-direction: column; justify-content: space-between; padding: 24px 16px; position: sticky; top: 0; left: 0; }
.logo-text { color: #2563eb; font-size: 1.25rem; font-weight: 800; margin-bottom: 32px; padding-left: 12px; }
.nav-links { display: flex; flex-direction: column; gap: 4px; }
.nav-item { width: 100%; border: none; background: none; font-family: inherit; display: flex; align-items: center; padding: 10px 12px; color: #4b5563; text-decoration: none; border-radius: 8px; font-weight: 500; cursor: pointer; transition: 0.2s; font-size: 1rem; }
.nav-item:hover { background-color: #f3f4f6; color: #2563eb; }
.router-link-active { background-color: #eff6ff; color: #2563eb; }
.btn-report { background-color: #2563eb !important; color: white !important; margin-top: 8px; font-weight: 600; }
.sidebar-bottom { border-top: 1px solid #e5e7eb; padding-top: 16px; }
.user-control { display: flex; flex-direction: column; gap: 8px; padding: 0 12px; } 
.user-control.clickable { 
    cursor: pointer;
    background-color: #f9fafb;
    border-radius: 6px;
    padding: 10px;
    transition: background-color 0.1s;
}
.user-control.clickable:hover {
    background-color: #f3f4f6;
}
.user-badge { font-size: 0.95rem; font-weight: 700; color: #1f2937; margin-bottom: 5px; }
.logout-link { color: #ef4444; padding: 0; font-size: 0.9rem; justify-content: flex-start; }
.logout-link:hover { background: none; text-decoration: underline; }
.login-link { color: #6b7280; }
.icon { margin-right: 12px; }
</style>