<script setup>
import { ref } from 'vue'
import { RouterView } from 'vue-router'
import Navbar from './components/Navbar.vue'
import ReportModal from './components/ReportModal.vue'

const isModalOpen = ref(false)

</script>

<template>
  <div class="app-layout">
    <Navbar @open-report="isModalOpen = true" />
    <main class="main-content">
      <RouterView />
    </main>
    <ReportModal v-if="isModalOpen" @close="isModalOpen = false" />
  </div>
</template>

<style>
/* TWARDY RESET */
*, *::before, *::after {
  box-sizing: border-box; /* Zapobiega "puchnięciu" elementów przez padding */
  margin: 0;
  padding: 0;
}

html, body {
  margin: 0;
  padding: 0;
  width: 100%;
  height: 100%;
  overflow-x: hidden; /* Blokuje przewijanie na boki */
  background-color: #f9fafb;
}

/* To jest kluczowe - Vue domyślnie tu dodaje padding i max-width */
#app {
  width: 100% !important;
  max-width: 100% !important;
  margin: 0 !important;
  padding: 0 !important;
  display: block !important; /* Wymuszenie braku grida z domyślnych styli */
}

.app-layout {
  display: flex;
  width: 100vw;
  min-height: 100vh;
}

.main-content {
  flex: 1; /* Zajmuje całą resztę szerokości */
  padding: 40px;
  background-color: #f9fafb;
}
</style>