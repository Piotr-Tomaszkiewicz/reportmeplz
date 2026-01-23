<script setup>
import { ref, onMounted } from 'vue'
import { RouterView } from 'vue-router'
import Navbar from './components/Navbar.vue'
import ReportModal from './components/ReportModal.vue'
import AuthModal from './components/AuthModal.vue'
import UserInfoModal from './components/UserInfoModal.vue' // NOWY IMPORT
import { useAuthStore } from './stores/authStore' 

const authStore = useAuthStore()

// null | 'report' | 'login' | 'user-profile'
const activeModal = ref(null)
const profileModalUserId = ref(null) // ID użytkownika do wyświetlenia w modal

// Używamy Pinia do załadowania profilu użytkownika na starcie
onMounted(() => {
  authStore.fetchUserProfile() 
})

const handleOpenReport = () => {
  if (authStore.isLoggedIn) {
    activeModal.value = 'report'
  } else {
    alert("Zaloguj się lub stwórz konto, aby móc wysłać zgłoszenie.")
    activeModal.value = 'login'
  }
}

// NOWA FUNKCJA: Otwieranie Modala Profilu
const handleOpenProfile = (userId) => {
    // Ponieważ użytkownik klika na własny profil (jest zalogowany),
    // używamy jego ID, które jest w Pinia, lub fallbacku.
    const id = userId || authStore.userProfile?.id; 
    
    if (id) {
        profileModalUserId.value = id;
        activeModal.value = 'user-profile';
    } else {
        alert("Brak ID użytkownika do wyświetlenia profilu.");
    }
}

const closeModal = () => {
  activeModal.value = null
  profileModalUserId.value = null;
}
</script>

<template>
  <div class="app-container">
    <!-- Navbar nasłuchuje zdarzeń otwarcia modala -->
    <Navbar 
      @open-report="handleOpenReport" 
      @open-login="activeModal = 'login'" 
      @open-profile="handleOpenProfile"
    />
    
    <!-- MIEJSCE NA TREŚĆ STRONY -->
    <main class="main-content">
      <RouterView @open-report="handleOpenReport" />
    </main>

    <!-- Modal Zgłoszenia -->
    <ReportModal 
      v-if="activeModal === 'report'" 
      @close="closeModal" 
    />

    <!-- Modal Logowania/Rejestracji -->
    <AuthModal 
      v-if="activeModal === 'login'" 
      @close="closeModal" 
    />

    <!-- Modal Profilu Użytkownika -->
    <UserInfoModal
        v-if="activeModal === 'user-profile' && profileModalUserId"
        :user-id="profileModalUserId"
        @close="closeModal"
    />
  </div>
</template>

<style>
/* Reset i style ogólne */
* { box-sizing: border-box; margin: 0; padding: 0; }

/* TWARDY RESET */
html, body {
  margin: 0; padding: 0;
  width: 100%; height: 100%;
  overflow-x: hidden;
  background-color: #f9fafb;
}

#app {
  margin: 0; padding: 0;
  max-width: 100% !important;
  width: 100%;
}

.app-container {
  display: flex;
  min-height: 100vh;
}

.sidebar {
  width: 240px;
  min-width: 240px; 
  height: 100vh;
  background-color: #ffffff;
  border-right: 1px solid #e5e7eb;
  display: flex;
  flex-direction: column;
  position: sticky; 
  top: 0;
  left: 0;
}

.main-content {
  flex-grow: 1;
  width: calc(100% - 240px); 
  padding: 2rem;
  min-height: 100vh;
}

.router-link-active {
  background-color: #eff6ff !important;
  color: #2563eb !important;
}

@media (max-width: 768px) {
  .main-content {
    width: 100%;
  }
}
</style>