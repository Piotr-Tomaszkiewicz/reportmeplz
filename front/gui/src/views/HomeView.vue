<template>
  <div class="home-container">
    <!-- Sekcja powitalna -->
    <header class="welcome-section">
      <div class="welcome-text">
        <h1>Witaj w <span class="brand-name">REPORTMEPLZ</span></h1>
        <p>Centralny system zarządzania incydentami i usterkami. Zgłoś problem w kilka sekund, a my zajmiemy się jego śledzeniem i rozwiązaniem.</p>
        
        <div class="welcome-actions">
          <!-- Emitujemy zdarzenie do App.vue, by otworzyć modal ZGŁOSZENIA -->
          <button @click="$emit('open-report')" class="btn-primary-large">
            ➕ Utwórz nowe zgłoszenie
          </button>
          <router-link to="/moje-zgloszenia" class="btn-secondary-large">
            📋 Zobacz Moje zgłoszenia
          </router-link>
        </div>
      </div>
    </header>

    <!-- Sekcja zalet i funkcji systemu (Z trzema kartami) -->
    <section class="features-grid">
      
      <div class="feature-card">
        <div class="feature-icon">🔍</div>
        <h3>Pełna transparentność</h3>
        <p>Śledź status swoich zgłoszeń w czasie rzeczywistym. Widzisz, kto jest przypisany do zadania i na jakim jest ono etapie (Zarejestrowany, W realizacji, Rozwiązany).</p>
      </div>
      
      <div class="feature-card">
        <div class="feature-icon">🔒</div>
        <h3>Bezpieczeństwo danych (JWT)</h3>
        <p>Korzystamy z nowoczesnej autoryzacji JWT, zapewniając, że Twoje dane i zgłoszenia są widoczne tylko dla uprawnionych osób w firmie.</p>
      </div>
      
      <div class="feature-card">
        <div class="feature-icon">🖼️</div>
        <h3>Obsługa załączników</h3>
        <p>Dołącz zdjęcia, zrzuty ekranu lub dokumenty do swojego zgłoszenia, by maksymalnie ułatwić zrozumienie i przyspieszyć naprawę.</p>
      </div>

    </section>

    <!-- Stopka informacyjna (ZMIENIONA) -->
    <footer class="home-footer">
      <p>Aplikacja stworzona przez <strong>Piotra Tomaszkiewicza</strong> oraz <strong>Adama Pietrasa</strong>.</p>
    </footer>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router';
// Emitem otwiera się modal zgłaszania (zdefiniowany w App.vue)
defineEmits(['open-report']) 

const router = useRouter(); 
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

/* Header Section */
.welcome-section {
    display: flex;
    justify-content: center;
    align-items: center;
    text-align: center;
    background: white;
    padding: 4rem 3rem;
    border-radius: 1.5rem;
    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
    margin-bottom: 2.5rem;
    border: 1px solid #e5e7eb;
}

.welcome-text {
    max-width: 700px;
}

.welcome-text h1 {
  font-size: 2.5rem;
  color: #111827;
  margin-bottom: 1rem;
}

.brand-name {
  color: #2563eb;
  font-weight: 800;
}

.welcome-text p {
  color: #6b7280;
  font-size: 1.1rem;
  line-height: 1.6;
  margin-bottom: 2rem;
}

.welcome-actions {
  display: flex;
  justify-content: center;
  gap: 1.25rem;
}

/* Buttons */
.btn-primary-large {
  background: #2563eb;
  color: white;
  border: none;
  padding: 1rem 2rem;
  border-radius: 0.75rem;
  font-weight: 600;
  cursor: pointer;
  transition: 0.2s;
  text-decoration: none;
  display: inline-block;
}

.btn-primary-large:hover { background: #1d4ed8; transform: translateY(-2px); }

.btn-secondary-large {
  background: #f3f4f6;
  color: #374151;
  padding: 1rem 2rem;
  border-radius: 0.75rem;
  text-decoration: none;
  font-weight: 600;
  transition: 0.2s;
  display: inline-block;
}

.btn-secondary-large:hover { background: #e5e7eb; }


/* FEATURES GRID */
.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); 
  gap: 1.5rem;
  margin-bottom: 3rem;
}

.feature-card {
  background: white;
  padding: 2rem;
  border-radius: 1.25rem;
  border: 1px solid #e5e7eb;
  transition: 0.3s;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.feature-card:hover { border-color: #2563eb; }

.feature-icon { 
    font-size: 1.8rem; 
    margin-bottom: 1rem; 
    color: #2563eb;
}

.feature-card h3 { 
    color: #111827; 
    margin-bottom: 0.75rem;
    font-size: 1.25rem;
}

.feature-card p { 
    color: #6b7280; 
    font-size: 0.95rem; 
    line-height: 1.5; 
}

/* Footer */
.home-footer {
  text-align: center;
  padding: 1.5rem;
  color: #9ca3af;
  font-size: 0.85rem;
  border-top: 1px solid #e5e7eb;
}

/* Nowa stylizacja imion */
.home-footer strong {
    color: #2563eb;
    font-weight: 700;
}


/* Responsywność */
@media (max-width: 640px) {
  .welcome-section { padding: 3rem 1rem; }
  .welcome-actions { flex-direction: column; gap: 1rem; }
  .btn-primary-large, .btn-secondary-large { width: 100%; text-align: center; }
}
</style>