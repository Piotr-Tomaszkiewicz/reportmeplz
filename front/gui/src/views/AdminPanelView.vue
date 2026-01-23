<template>
  <div class="admin-dashboard">
    <h1>Panel Administracyjny</h1>
    <p class="subtitle">Zarządzanie zasobami i użytkownikami systemu.</p>

    <nav class="admin-nav">
      <router-link :to="{ name: 'admin-locations' }" class="admin-tab">
        Zarządzanie Lokalizacjami
      </router-link>
      <router-link :to="{ name: 'admin-users' }" class="admin-tab">
        Zarządzanie Użytkownikami
      </router-link>
    </nav>
    
    <!-- Tutaj będą się ładować moduły: LocationsView lub UsersView -->
    <router-view />
  </div>
</template>

<script setup>
// Brak logiki Vue, to jest tylko kontener/layout dla routingu
</script>

<style scoped>
.admin-dashboard {
    max-width: 1200px;
    margin: 0 auto;
}
.admin-dashboard h1 {
    font-size: 2.2rem;
    color: #6d28d9; /* Głęboki fiolet */
    margin-bottom: 0.5rem;
}
.subtitle {
    color: #9ca3af;
    margin-bottom: 2rem;
}
.admin-nav {
    display: flex;
    margin-bottom: 2rem;
    border-bottom: 2px solid #e5e7eb;
}
.admin-tab {
    padding: 10px 20px;
    text-decoration: none;
    color: #4b5563;
    font-weight: 600;
    transition: all 0.2s;
    border-bottom: 3px solid transparent;
}
.admin-tab:hover {
    color: #6d28d9;
    background-color: #f3f4f6;
}
.router-link-active {
    color: #6d28d9;
    border-bottom-color: #6d28d9;
}
</style>