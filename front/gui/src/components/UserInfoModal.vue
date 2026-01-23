<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-container">
      <button class="close-x" @click="$emit('close')">&times;</button>

      <header class="modal-header">
        <h2>Szczegóły użytkownika</h2>
      </header>

      <div v-if="loading" class="info-msg">Ładowanie danych...</div>
      
      <div v-else-if="user" class="user-details">
        
        <div class="detail-group">
          <span class="detail-label">👤 Login:</span>
          <span class="detail-value primary">
            {{ user.login || 'Brak' }}
          </span>
        </div>
        
        <div class="detail-group">
          <span class="detail-label">📧 E-mail:</span>
          <span class="detail-value">{{ user.email || 'Brak' }}</span>
        </div>
        
        <div class="detail-group">
          <span class="detail-label">📍 Lokalizacja:</span>
          <span class="detail-value location" :title="user.locationFullAddress">
            {{ user.locationShortName || 'Nieokreślona' }}
          </span>
        </div>
        
        <div class="detail-group">
          <span class="detail-label">👮 Rola:</span>
          <span class="detail-value role">{{ user.role || 'user' }}</span>
        </div>

      </div>
      
      <div v-else class="info-msg error">
        Nie udało się pobrać danych użytkownika (ID: {{ userId }})
      </div>
      
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { userService } from '../services/userService'

const props = defineProps({
  userId: {
    type: [Number, String],
    required: true
  }
})
const emit = defineEmits(['close'])

const user = ref(null)
const loading = ref(true)

onMounted(async () => {
  try {
    const response = await userService.getUserById(props.userId)
    
    // MAPOWANIE JSON Z API DO LOKALNEGO OBIEKTU user.value
    user.value = {
      login: response.data.login || response.data.Login,
      email: response.data.email || response.data.Email,
      locationShortName: response.data.locationShortName || response.data.LocationShortName,
      locationFullAddress: response.data.locationFullAddress || response.data.LocationFullAddress,
      role: response.data.role || response.data.Role,
    }

  } catch (err) {
    console.error("Błąd ładowania danych użytkownika:", err)
    user.value = null
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.modal-overlay { position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; background: rgba(0, 0, 0, 0.5); backdrop-filter: blur(4px); display: flex; justify-content: center; align-items: center; z-index: 10001; }
.modal-container { background: white; width: 90%; max-width: 400px; padding: 2rem; border-radius: 12px; box-shadow: 0 10px 20px rgba(0, 0, 0, 0.2); position: relative; }
.close-x { position: absolute; top: 1rem; right: 1rem; background: none; border: none; font-size: 1.5rem; cursor: pointer; color: #9ca3af; }

.modal-header h2 { color: #1f2937; margin-bottom: 1rem; }

.user-details { margin-top: 1rem; }
.detail-group { display: flex; justify-content: space-between; padding: 0.75rem 0; border-bottom: 1px dashed #f3f4f6; }
.detail-label { font-weight: 600; color: #4b5563; font-size: 0.9rem; }
.detail-value { font-weight: 500; color: #111827; }
.detail-value.primary { color: #2563eb; font-weight: 700; }
.detail-value.location { font-style: italic; }
.detail-value.role { text-transform: uppercase; }

.info-msg { text-align: center; padding: 2rem 0; color: #6b7280; }
.info-msg.error { color: #dc2626; font-weight: 600; }
</style>