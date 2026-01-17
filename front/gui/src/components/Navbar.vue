<template>
  <nav class="sidebar">
    <div class="sidebar-content">
      <div class="logo-section">
        <h1 class="logo-text">REPORTMEPLZ</h1>
      </div>

      <div class="nav-links">
        <router-link to="/moje-zgloszenia" class="nav-item">
          <span class="icon">📋</span> Moje zgłoszenia
        </router-link>
        <router-link to="/wszystkie-zgloszenia" class="nav-item">
          <span class="icon">🌍</span> Wszystkie zgłoszenia
        </router-link>
        <div @click="$emit('open-report')" class="nav-item btn-report" style="cursor: pointer;">
          <span class="icon">➕</span> Zgłoś
        </div>
      </div>
    </div>

    <div class="sidebar-footer">
      <router-link to="/login" class="nav-item login-link">
        <span class="icon">👤</span> Zaloguj
      </router-link>
    </div>
  </nav>
</template>

<style scoped>
.sidebar {
  width: 240px; /* Zmniejszona szerokość */
  min-width: 240px;
  height: 100vh;
  background-color: #ffffff;
  border-right: 1px solid #e5e7eb;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  padding: 24px 16px;
  position: sticky;
  top: 0;
}

.logo-text {
  color: #2563eb;
  font-size: 1.25rem;
  font-weight: 800;
  margin-bottom: 32px;
  padding-left: 12px;
}

.nav-links {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.nav-item {
  display: flex;
  align-items: center;
  padding: 10px 12px;
  color: #4b5563;
  text-decoration: none;
  border-radius: 8px;
  font-weight: 500;
  transition: 0.2s;
}

.nav-item:hover {
  background-color: #f3f4f6;
  color: #2563eb;
}

.router-link-active {
  background-color: #eff6ff;
  color: #2563eb;
}

.btn-report {
  margin-top: 12px;
  background-color: #2563eb; /* Podstawowy niebieski */
  color: white !important;
  font-weight: 600;
  box-shadow: 0 2px 4px rgba(37, 99, 235, 0.1); /* Delikatny niebieski cień */
  transition: all 0.2s ease-in-out;
}

/* Styl po najechaniu myszką (HOVER) */
.btn-report:hover {
  background-color: #1d4ed8; /* Ciemniejszy, głęboki niebieski */
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3); /* Mocniejszy cień przy najechaniu */
  transform: translateY(-1px); /* Efekt lekkiego uniesienia */
}

/* Styl przy kliknięciu (ACTIVE) */
.btn-report:active {
  transform: translateY(0px); /* Efekt wciśnięcia */
  box-shadow: 0 2px 6px rgba(37, 99, 235, 0.2);
}

/* Upewnienie się, że ikona też jest dobrze widoczna */
.btn-report .icon {
  filter: brightness(0) invert(1); /* Wymusza biały kolor emotki/ikony jeśli trzeba */
  margin-right: 10px;
}

.sidebar-footer {
  border-top: 1px solid #e5e7eb;
  padding-top: 16px;
}

.icon {
  margin-right: 12px;
}
</style>