<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-container">
      <button class="close-x" @click="$emit('close')">&times;</button>

      <header class="modal-header">
        <h2>Nowe zgłoszenie</h2>
        <p>Wypełnij formularz i załącz zdjęcie usterki.</p>
      </header>

      <form @submit.prevent="handleSubmit" class="report-form">
        <!-- Tytuł -->
        <div class="form-group">
          <label>Tytuł</label>
          <input v-model="form.title" type="text" placeholder="Np. Uszkodzona klamka..." required />
        </div>

        <!-- Priorytet -->
        <div class="form-group">
          <label>Priorytet</label>
          <select v-model.number="form.priority" required>
            <option :value="1">Zwykły (1)</option>
            <option :value="2">Wysoki (2)</option>
            <option :value="3">Krytyczny (3)</option>
          </select>
        </div>

        <!-- Opis -->
        <div class="form-group">
          <label>Opis</label>
          <textarea v-model="form.description" rows="4" placeholder="Opisz szczegóły usterki..." required></textarea>
        </div>

        <!-- PLIK -->
        <div class="form-group">
          <label>Załącznik (Zdjęcie/Dokument)</label>
          <div class="file-upload-wrapper">
            <input 
              type="file" 
              id="file-input" 
              @change="handleFileChange" 
              accept="image/*,.pdf,.doc,.docx"
              class="hidden-input"
            />
            <label for="file-input" class="file-label">
              <span class="file-icon">📁</span>
              {{ selectedFile ? selectedFile.name : 'Wybierz plik z komputera' }}
            </label>
            <p v-if="selectedFile" class="file-size">
              Rozmiar: {{ (selectedFile.size / 1024).toFixed(2) }} KB
            </p>
          </div>
        </div>

        <div class="modal-footer">
          <button type="button" class="btn-cancel" @click="$emit('close')">Anuluj</button>
          <button type="submit" class="btn-submit" :disabled="isSubmitting">
            {{ isSubmitting ? 'Wysyłanie pliku...' : 'Wyślij zgłoszenie' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { reportService } from '../services/reportService'

const emit = defineEmits(['close'])
const isSubmitting = ref(false)
const selectedFile = ref(null)

const form = reactive({
  title: '',
  description: '',
  priority: 1
})

const handleFileChange = (event) => {
  const file = event.target.files[0]
  if (file) {
    selectedFile.value = file
  }
}

const handleSubmit = async () => {
  try {
    isSubmitting.value = true
    
    await reportService.createReport({
      title: form.title,
      description: form.description,
      priority: form.priority,
      file: selectedFile.value 
    })

    alert('Zgłoszenie wraz z plikiem zostało wysłane!')
    emit('close')
    window.location.reload()
  } catch (err) {
    console.error('Błąd wysyłki:', err)
    alert(err.response?.data?.message || 'Błąd podczas przesyłania danych i pliku')
  } finally {
    isSubmitting.value = false
  }
}
</script>

<style scoped>
.modal-overlay { position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; background: rgba(0,0,0,0.5); backdrop-filter: blur(4px); display: flex; justify-content: center; align-items: center; z-index: 10000; }
.modal-container { background: white; width: 95%; max-width: 500px; padding: 2rem; border-radius: 1rem; position: relative; box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1); }
.close-x { position: absolute; top: 1rem; right: 1rem; background: none; border: none; font-size: 1.5rem; cursor: pointer; color: #9ca3af; }
.modal-header h2 { color: #111827; margin-bottom: 0.5rem; }
.modal-header p { color: #6b7280; font-size: 0.9rem; margin-bottom: 1.5rem; }
.form-group { display: flex; flex-direction: column; gap: 0.4rem; margin-bottom: 1.2rem; }
.form-group label { font-size: 0.85rem; font-weight: 600; color: #374151; }
input, select, textarea { padding: 0.75rem; border: 1px solid #d1d5db; border-radius: 0.5rem; font-size: 1rem; outline: none; }
input:focus, select:focus, textarea:focus { border-color: #2563eb; }
.hidden-input { display: none; }
.file-label {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 0.75rem;
  background: #f3f4f6;
  border: 2px dashed #d1d5db;
  border-radius: 0.5rem;
  cursor: pointer;
  color: #4b5563;
  transition: 0.2s;
}
.file-label:hover { background: #e5e7eb; border-color: #2563eb; color: #2563eb; }
.file-size { font-size: 0.75rem; color: #9ca3af; margin-top: 4px; }
.modal-footer { display: flex; justify-content: flex-end; gap: 1rem; margin-top: 1.5rem; }
.btn-cancel { padding: 0.75rem 1.5rem; background: #f3f4f6; border: none; border-radius: 0.5rem; cursor: pointer; font-weight: 500; }
.btn-submit { padding: 0.75rem 1.5rem; background: #2563eb; color: white; border: none; border-radius: 0.5rem; cursor: pointer; font-weight: 600; }
.btn-submit:disabled { opacity: 0.6; cursor: not-allowed; }
</style>