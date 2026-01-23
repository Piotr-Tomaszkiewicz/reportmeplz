<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-container">
      <button class="close-x" @click="$emit('close')">&times;</button>

      <header class="modal-header">
        <h2>Dostępne Lokalizacje</h2>
        <p>Lista wszystkich biur i oddziałów zarejestrowanych w systemie.</p>
      </header>

      <div v-if="loading" class="info-msg">Ładowanie danych...</div>
      
      <div v-else class="location-list">
        <div class="location-header">
            <span>Nazwa Skrócona</span>
            <span>Pełny Adres</span>
        </div>
        <div 
          v-for="loc in sortedLocations" 
          :key="loc.id" 
          class="location-item"
        >
          <span class="location-short">{{ loc.shortName }}</span>
          <span class="location-address">{{ loc.fullAddress }}</span>
        </div>
        
        <p v-if="!sortedLocations.length" class="info-msg">Brak lokalizacji do wyświetlenia.</p>
      </div>

      <footer class="modal-footer">
        <button class="btn-cancel" @click="$emit('close')">Zamknij</button>
      </footer>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { locationService } from '../services/locationService' // Wymaga importu locationService
import { useAuthStore } from '../stores/authStore'

const emit = defineEmits(['close'])
const loading = ref(true)
const locations = ref([])

onMounted(async () => {
  try {
    const response = await locationService.getLocations()
    // Zakładamy, że API zwraca pola: id, shortName, fullAddress
    locations.value = response.data
  } catch (error) {
    console.error("Błąd ładowania lokalizacji:", error)
  } finally {
    loading.value = false
  }
})

// Sortowanie alfabetyczne po nazwie skróconej
const sortedLocations = computed(() => {
    return [...locations.value].sort((a, b) => {
        const nameA = a.shortName.toLowerCase()
        const nameB = b.shortName.toLowerCase()
        if (nameA < nameB) return -1
        if (nameA > nameB) return 1
        return 0
    })
})
</script>

<style scoped>
.modal-overlay { position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; background: rgba(0, 0, 0, 0.5); backdrop-filter: blur(4px); display: flex; justify-content: center; align-items: center; z-index: 10001; }
.modal-container { 
    background: white; width: 90%; max-width: 600px; 
    padding: 2rem; border-radius: 12px; box-shadow: 0 10px 20px rgba(0, 0, 0, 0.2); 
    position: relative; max-height: 90vh; display: flex; flex-direction: column; 
}
.close-x { position: absolute; top: 1rem; right: 1rem; background: none; border: none; font-size: 1.5rem; cursor: pointer; color: #9ca3af; }

.modal-header h2 { color: #1f2937; margin-bottom: 0.5rem; }
.modal-header p { color: #6b7280; margin-bottom: 1.5rem; font-size: 0.95rem; }

.location-list {
    flex-grow: 1;
    overflow-y: auto; /* Włączamy przewijanie dla listy */
    border: 1px solid #e5e7eb;
    border-radius: 8px;
    margin-bottom: 1.5rem;
}

.location-header {
    display: grid;
    grid-template-columns: 1fr 2fr;
    background: #f3f4f6;
    font-weight: 700;
    padding: 10px 15px;
    border-bottom: 1px solid #d1d5db;
    position: sticky;
    top: 0;
    z-index: 10;
}

.location-item {
    display: grid;
    grid-template-columns: 1fr 2fr;
    padding: 10px 15px;
    border-bottom: 1px solid #f3f4f6;
    font-size: 0.9rem;
}

.location-item:nth-child(even) {
    background: #fafafa;
}

.location-short {
    font-weight: 600;
    color: #2563eb;
}

.location-address {
    color: #4b5563;
}

.modal-footer { display: flex; justify-content: flex-end; }
.btn-cancel { padding: 0.75rem 1.5rem; background: #f3f4f6; border: none; border-radius: 0.5rem; cursor: pointer; font-weight: 500; }

.info-msg { text-align: center; padding: 2rem 0; color: #6b7280; }
</style>