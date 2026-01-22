<template>
  <div class="home-container">
    <!-- Sekcja powitalna -->
    <header class="welcome-section">
      <div class="welcome-text">
        <h1>Witaj w <span class="brand-name">REPORTMEPLZ</span></h1>
        <p>Twój centralny system zgłaszania i śledzenia problemów technicznych. Szybko, sprawnie i pod kontrolą.</p>
        
        <div class="welcome-actions">
          <!-- Emitujemy zdarzenie do App.vue -->
          <button @click="$emit('open-report')" class="btn-primary-large">
            ➕ Nowe zgłoszenie
          </button>
          <router-link to="/moje-zgloszenia" class="btn-secondary-large">
            📋 Moja historia
          </router-link>
        </div>
      </div>

      <div class="welcome-stats">
        <!-- Karta 1 -->
        <div class="stat-mini-card">
          <span class="stat-icon">🔥</span>
          <div class="stat-info">
            <span class="stat-label">Aktywne błędy</span>
            <span class="stat-value">12</span>
          </div>
        </div>
        <!-- Karta 2 -->
        <div class="stat-mini-card">
          <span class="stat-icon">✅</span>
          <div class="stat-info">
            <span class="stat-label">Rozwiązane</span>
            <span class="stat-value">148</span>
          </div>
        </div>
      </div>
    </header>

    <!-- Sekcja informacyjna -->
    <section class="info-grid">
      <div class="info-card">
        <div class="info-icon">🚀</div>
        <h3>Szybkość reakcji</h3>
        <p>Nasze API na localhost:5016 przetwarza Twoje zgłoszenia w ułamku sekundy, powiadamiając odpowiednie osoby.</p>
      </div>
      
      <div class="info-card">
        <div class="info-icon">🔒</div>
        <h3>Bezpieczeństwo JWT</h3>
        <p>Twoje dane są chronione zaawansowanym systemem tokenów JWT, zapewniając dostęp tylko uprawnionym użytkownikom.</p>
      </div>

      <div class="info-card">
        <div class="info-icon">📍</div>
        <h3>Precyzyjna lokalizacja</h3>
        <p>Dzięki integracji z listą lokalizacji, wiemy dokładnie, gdzie wystąpił problem, co przyspiesza naprawę.</p>
      </div>
    </section>

    <!-- Stopka -->
    <footer class="home-footer">
      <p>Status systemu: <span class="status-online">Online (Port: 5016)</span></p>
    </footer>
  </div>
</template>

<script setup>
defineEmits(['open-report'])
</script>

<style scoped>
.home-container {
  max-width: 1000px;
  margin: 0 auto;
  animation: fadeIn 0.6s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.welcome-section {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: white;
  padding: 3rem;
  border-radius: 1.5rem;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
  margin-bottom: 2rem;
  border: 1px solid #e5e7eb;
}

.welcome-text h1 { font-size: 2.5rem; color: #111827; margin-bottom: 1rem; }
.brand-name { color: #2563eb; font-weight: 800; }
.welcome-text p { color: #6b7280; font-size: 1.1rem; max-width: 500px; line-height: 1.6; margin-bottom: 2rem; }
.welcome-actions { display: flex; gap: 1rem; }

.welcome-stats { display: flex; flex-direction: column; gap: 1rem; }
.stat-mini-card {
  background: #f9fafb;
  padding: 1rem 1.5rem;
  border-radius: 1rem;
  display: flex;
  align-items: center;
  gap: 1rem;
  border: 1px solid #f3f4f6;
}

.stat-icon { font-size: 1.5rem; }
.stat-label { display: block; font-size: 0.8rem; color: #6b7280; font-weight: 600; }
.stat-value { font-size: 1.25rem; font-weight: 700; color: #1f2937; }

.btn-primary-large {
  background: #2563eb; color: white; border: none;
  padding: 1rem 2rem; border-radius: 0.75rem;
  font-weight: 600; cursor: pointer; transition: 0.2s;
}
.btn-primary-large:hover { background: #1d4ed8; transform: translateY(-2px); }

.btn-secondary-large {
  background: #f3f4f6; color: #374151;
  padding: 1rem 2rem; border-radius: 0.75rem;
  text-decoration: none; font-weight: 600; transition: 0.2s;
}
.btn-secondary-large:hover { background: #e5e7eb; }

.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.info-card {
  background: white; padding: 2rem; border-radius: 1.25rem;
  border: 1px solid #e5e7eb; transition: 0.3s;
}
.info-card:hover { transform: translateY(-5px); box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1); }
.info-icon { font-size: 2rem; margin-bottom: 1rem; }
.info-card h3 { color: #111827; margin-bottom: 0.75rem; }
.info-card p { color: #6b7280; font-size: 0.95rem; line-height: 1.5; }

.home-footer { text-align: center; padding: 2rem; color: #9ca3af; font-size: 0.85rem; }
.status-online { color: #10b981; font-weight: 700; }

@media (max-width: 768px) {
  .welcome-section { flex-direction: column; text-align: center; padding: 2rem; }
  .welcome-text p { margin: 0 auto 2rem; }
  .welcome-actions { justify-content: center; flex-direction: column; }
}
</style>