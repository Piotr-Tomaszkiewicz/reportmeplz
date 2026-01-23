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
        
        <router-link 
          v-if="canViewAllReports" 
          to="/wszystkie-zgloszenia" 
          class="nav-item"
        >
          <span class="icon">🌍</span> Wszystkie zgłoszenia
        </router-link>

        <!-- PANEL ADMINA - Tylko dla Admina -->
        <router-link 
          v-if="isAdmin" 
          to="/admin" 
          class="nav-item admin-link"
        >
          <span class="icon">🛠</span> Panel Admina
        </router-link>
        
        <button type="button" @click="$emit('open-locations')" class="nav-item location-link">
          <span class="icon">📍</span> Lokalizacje
        </button>
        
        <button type="button" @click="$emit('open-report')" class="nav-item btn-report">
          <span class="icon">➕</span> Zgłoś
        </button>
      </div>
    </div>

    <div class="sidebar-bottom">
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
import { useAuthStore } from '../stores/authStore'

const authStore = useAuthStore()
defineEmits(['open-report', 'open-login', 'open-profile', 'open-locations'])

const isLoggedIn = ref(false)
const login = ref('')
const userRole = ref('')

onMounted(() => {
  const token = localStorage.getItem('token')
  if (token) {
    isLoggedIn.value = true
    login.value = localStorage.getItem('userLogin') || 'Użytkownik'
    userRole.value = (localStorage.getItem('role') || 'user').toLowerCase()
  }
})

const currentUserId = computed(() => localStorage.getItem('userId'))

const canViewAllReports = computed(() => {
  if (!isLoggedIn.value) return false
  const role = userRole.value.toLowerCase()
  return role === 'admin' || role === 'manager'
})

const isAdmin = computed(() => {
  return userRole.value.toLowerCase() === 'admin'
})

const handleLogout = () => {
  authStore.logout()
}
</script>

<style scoped>
.sidebar { width: 240px; min-width: 240px; height: 100vh; background-color: #ffffff; border-right: 1px solid #e5e7eb; display: flex; flex-direction: column; justify-content: space-between; padding: 24px 16px; position: sticky; top: 0; left: 0; }
.logo-text { color: #2563eb; font-size: 1.25rem; font-weight: 800; margin-bottom: 32px; padding-left: 12px; }
.nav-links { display: flex; flex-direction: column; gap: 4px; }
.nav-item { width: 100%; border: none; background: none; font-family: inherit; display: flex; align-items: center; padding: 10px 12px; color: #4b5563; text-decoration: none; border-radius: 8px; font-weight: 500; cursor: pointer; transition: 0.2s; font-size: 1rem; }
.nav-item:hover { background-color: #f3f4f6; color: #2563eb; }
.router-link-active { background-color: #eff6ff !important; color: #2563eb !important; font-weight: 700; }
.btn-report { background-color: #2563eb !important; color: white !important; margin-top: 8px; font-weight: 600; }
.admin-link { color: #7c3aed; margin-top: 10px; border-top: 1px solid #f3f4f6; padding-top: 15px; }
.admin-link:hover { color: #6d28d9; background-color: #f5f3ff; }
.sidebar-bottom { border-top: 1px solid #e5e7eb; padding-top: 16px; }
.user-control.clickable { cursor: pointer; background-color: #f9fafb; border-radius: 6px; padding: 10px; transition: 0.1s; }
.user-control.clickable:hover { background-color: #f3f4f6; }
.user-badge { font-size: 0.9rem; font-weight: 700; color: #1f2937; margin-bottom: 5px; }
.logout-link { color: #ef4444; padding: 0; font-size: 0.85rem; }
.logout-link:hover { text-decoration: underline; background: none; }
.icon { margin-right: 12px; }
</style>