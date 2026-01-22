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
        
        <!-- LINK WIDOCZNY TYLKO DLA ADMIN/MANAGER -->
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
      <div v-if="isLoggedIn" class="user-control">
        <div class="user-badge">👤 {{ login }}</div>
        <button @click="handleLogout" class="nav-item logout-link">
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

defineEmits(['open-report', 'open-login'])

const isLoggedIn = ref(false)
const login = ref('')
const userRole = ref('')

onMounted(() => {
  const token = localStorage.getItem('token')
  if (token) {
    isLoggedIn.value = true
    login.value = localStorage.getItem('userLogin') || 'Użytkownik'
    userRole.value = (localStorage.getItem('role') || 'user').toLowerCase() // Upewniamy się, że rola jest małą literą
  }
})

// Obliczana właściwość: zwraca true, jeśli rola to admin LUB manager
const canViewAllReports = computed(() => {
  if (!isLoggedIn.value) return false
  return userRole.value === 'admin' || userRole.value === 'manager'
})

const handleLogout = () => {
  localStorage.removeItem('token')
  localStorage.removeItem('userLogin')
  localStorage.removeItem('role') // Ważne: usuwamy też rolę
  isLoggedIn.value = false
  window.location.reload()
}
</script>

<style scoped>
.sidebar { width: 240px; min-width: 240px; height: 100vh; background-color: #ffffff; border-right: 1px solid #e5e7eb; display: flex; flex-direction: column; justify-content: space-between; padding: 24px 16px; position: sticky; top: 0; }
.logo-text { color: #2563eb; font-size: 1.25rem; font-weight: 800; margin-bottom: 32px; padding-left: 12px; }
.nav-links { display: flex; flex-direction: column; gap: 4px; }
.nav-item { width: 100%; border: none; background: none; font-family: inherit; display: flex; align-items: center; padding: 10px 12px; color: #4b5563; text-decoration: none; border-radius: 8px; font-weight: 500; cursor: pointer; transition: 0.2s; font-size: 1rem; }
.nav-item:hover { background-color: #f3f4f6; color: #2563eb; }
.router-link-active { background-color: #eff6ff; color: #2563eb; }
.btn-report { background-color: #2563eb !important; color: white !important; margin-top: 8px; font-weight: 600; }
.sidebar-bottom { border-top: 1px solid #e5e7eb; padding-top: 16px; }
.user-control { display: flex; flex-direction: column; gap: 8px; }
.user-badge { padding: 8px 12px; font-size: 0.9rem; font-weight: 600; color: #1f2937; }
.logout-link { color: #ef4444; }
.icon { margin-right: 12px; }
</style>