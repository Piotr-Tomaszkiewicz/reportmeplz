<template>
  <!-- Overlay (ściemnienie tła) -->
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-container">
      <!-- Przycisk X -->
      <button class="close-x" @click="$emit('close')">&times;</button>

      <header class="modal-header">
        <h2>Nowe zgłoszenie</h2>
        <p>Opisz napotkany problem najdokładniej jak potrafisz.</p>
      </header>

      <form @submit.prevent="handleSubmit" class="report-form">
        <!-- Tytuł -->
        <div class="form-group">
          <label for="title">Tytuł zgłoszenia</label>
          <input 
            id="title" 
            v-model="form.title" 
            type="text" 
            placeholder="Krótki opis (np. Błąd logowania)" 
            required
          >
        </div>

        <!-- Rodzaj błędu -->
        <div class="form-group">
          <label for="severity">Priorytet błędu</label>
          <select id="severity" v-model="form.severity" required>
            <option value="normalny">Zwykły (Normalny)</option>
            <option value="wysoki">Wysoki</option>
            <option value="krytyczny">Krytyczny 🔥</option>
          </select>
        </div>

        <!-- Opis -->
        <div class="form-group">
          <label for="description">Opis problemu</label>
          <textarea 
            id="description" 
            v-model="form.description" 
            rows="4" 
            placeholder="Co się stało? Jakie kroki podjąłeś?"
            required
          ></textarea>
        </div>

        <!-- Pliki -->
        <div class="form-group">
          <label for="files">Załączniki (zdjęcia/logi)</label>
          <input 
            id="files" 
            type="file" 
            multiple 
            @change="handleFileUpload"
            class="file-input"
          >
        </div>

        <!-- Stopka z przyciskiem -->
        <footer class="modal-footer">
          <button type="button" class="btn-cancel" @click="$emit('close')">Anuluj</button>
          <button type="submit" class="btn-submit">Wyślij zgłoszenie</button>
        </footer>
      </form>
    </div>
  </div>
</template>

<script setup>
import { reactive } from 'vue'

const emit = defineEmits(['close'])

const form = reactive({
  title: '',
  severity: 'normalny',
  description: '',
  files: null
})

const handleFileUpload = (event) => {
  form.files = event.target.files
}

const handleSubmit = () => {
  console.log('Dane do wysłania:', form)
  alert('Zgłoszenie zostało wysłane (sprawdź konsolę)')
  emit('close') // Zamknij okno po wysłaniu
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background-color: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(4px); /* Lekkie rozmycie tła */
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 9999;
}

.modal-container {
  background: white;
  width: 100%;
  max-width: 500px;
  padding: 2rem;
  border-radius: 1rem;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
  position: relative;
  animation: slideIn 0.3s ease-out;
}

@keyframes slideIn {
  from { transform: translateY(-20px); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

.close-x {
  position: absolute;
  top: 1rem;
  right: 1rem;
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #9ca3af;
}

.modal-header h2 {
  margin-bottom: 0.5rem;
  color: #111827;
}

.modal-header p {
  color: #6b7280;
  font-size: 0.9rem;
  margin-bottom: 1.5rem;
}

.form-group {
  margin-bottom: 1.2rem;
  display: flex;
  flex-direction: column;
}

.form-group label {
  font-size: 0.85rem;
  font-weight: 600;
  color: #374151;
  margin-bottom: 0.4rem;
}

input[type="text"],
select,
textarea {
  padding: 0.6rem;
  border: 1px solid #d1d5db;
  border-radius: 0.5rem;
  font-size: 1rem;
  outline: none;
}

input:focus, select:focus, textarea:focus {
  border-color: #2563eb;
  ring: 2px solid #bfdbfe;
}

.file-input {
  font-size: 0.8rem;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 2rem;
}

.btn-cancel {
  padding: 0.6rem 1.2rem;
  background: #f3f4f6;
  border: none;
  border-radius: 0.5rem;
  cursor: pointer;
  font-weight: 500;
}

.btn-submit {
  padding: 0.6rem 1.2rem;
  background: #2563eb;
  color: white;
  border: none;
  border-radius: 0.5rem;
  cursor: pointer;
  font-weight: 600;
}

.btn-submit:hover {
  background: #1d4ed8;
}
</style>