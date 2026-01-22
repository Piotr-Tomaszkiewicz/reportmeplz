<script setup>
import { ref } from 'vue'
import { RouterView } from 'vue-router'
import Navbar from './components/Navbar.vue'
import ReportModal from './components/ReportModal.vue'
import AuthModal from './components/AuthModal.vue'

// null | 'report' | 'login'
const activeModal = ref(null)

const handleOpenReport = () => {
  const token = localStorage.getItem('token')
  
  if (token) {
    // Jeśli zalogowany - otwórz formularz zgłoszenia
    activeModal.value = 'report'
  } else {
    // Jeśli nie - pokaż komunikat i otwórz modal logowania
    alert("Zaloguj się lub stwórz konto, aby móc wysłać zgłoszenie.")
    activeModal.value = 'login'
  }
}

const closeModal = () => {
  activeModal.value = null
}
</script>

<template>
  <div class="app-layout">
    <Navbar 
      @open-report="handleOpenReport" 
      @open-login="activeModal = 'login'" 
    />
    
    <main class="main-content">
      <router-view @open-report="handleOpenReport" />
    </main>

    <!-- Modale renderowane warunkowo -->
    <ReportModal 
      v-if="activeModal === 'report'" 
      @close="closeModal" 
    />

    <AuthModal 
      v-if="activeModal === 'login'" 
      @close="closeModal" 
    />
  </div>
</template>

<style>
/* Reset i Style Globalne */
*, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

html, body {
  margin: 0; padding: 0;
  width: 100%; height: 100%;
  background-color: #f9fafb;
  font-family: 'Inter', sans-serif;
}

#app {
  width: 100% !important;
  max-width: 100% !important;
}

.app-layout {
  display: flex;
  width: 100vw;
  min-height: 100vh;
}

.main-content {
  flex: 1;
  padding: 40px;
  background-color: #f9fafb;
  min-height: 100vh;
}
</style>