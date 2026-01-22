<template>
  <!-- Overlay (tło) -->
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-container">
      <!-- Przycisk zamknięcia -->
      <button class="close-x" @click="$emit('close')">&times;</button>

      <header class="modal-header">
        <h1 class="logo-small">REPORTMEPLZ</h1>
        <h2>{{ isLoginMode ? 'Zaloguj się' : 'Zarejestruj się' }}</h2>
        <p class="subtitle">
          {{ isLoginMode ? 'Witaj z powrotem!' : 'Dołącz do systemu zgłoszeń.' }}
        </p>
      </header>

      <form @submit.prevent="handleSubmit" class="auth-form">
        <!-- Login -->
        <div class="form-group">
          <label>Login</label>
          <input 
            v-model="form.login" 
            type="text" 
            placeholder="Twój login" 
            required 
          />
        </div>

        <!-- Sekcja tylko dla Rejestracji -->
        <template v-if="!isLoginMode">
          <!-- Email -->
          <div class="form-group">
            <label>Adres E-mail</label>
            <input 
              v-model="form.email" 
              type="email" 
              placeholder="email@poczta.pl" 
              required 
            />
          </div>

          <!-- Wybór Lokalizacji z API -->
          <div class="form-group">
            <label>Lokalizacja (Biuro/Oddział)</label>
            <div class="select-wrapper">
              <select v-model="form.locationId" required :disabled="isLoadingLocations">
                <option value="" disabled>
                  {{ isLoadingLocations ? 'Pobieranie listy...' : 'Wybierz lokalizację' }}
                </option>
                <option v-for="loc in locations" :key="loc.id" :value="loc.id">
                  {{ loc.shortName }} — {{ loc.fullAddress }}
                </option>
              </select>
            </div>
          </div>
        </template>

        <!-- Hasło -->
        <div class="form-group">
          <label>Hasło</label>
          <input 
            v-model="form.password" 
            type="password" 
            placeholder="••••••••" 
            required 
          />
        </div>

        <!-- Powtórz Hasło -->
        <div v-if="!isLoginMode" class="form-group">
          <label>Powtórz hasło</label>
          <input 
            v-model="form.confirmPassword" 
            type="password" 
            placeholder="••••••••" 
            required 
          />
        </div>

        <button type="submit" class="btn-submit" :disabled="isSubmitting">
          {{ isSubmitting ? 'Przetwarzanie...' : (isLoginMode ? 'Zaloguj się' : 'Stwórz konto') }}
        </button>
      </form>

      <footer class="modal-footer">
        <p v-if="isLoginMode">
          Nie masz konta? <span @click="toggleMode">Zarejestruj się</span>
        </p>
        <p v-else>
          Masz już konto? <span @click="toggleMode">Zaloguj się</span>
        </p>
      </footer>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { authService } from '../services/authService'
import { locationService } from '../services/locationService'

const emit = defineEmits(['close'])

const isLoginMode = ref(true)
const isSubmitting = ref(false)
const isLoadingLocations = ref(false)
const locations = ref([])

const form = reactive({
  login: '', // KLUCZ ZGODNY Z API
  email: '',
  locationId: '',
  password: '',
  confirmPassword: ''
})

// Pobieranie lokalizacji przy otwarciu modala
onMounted(async () => {
  try {
    isLoadingLocations.value = true
    const response = await locationService.getLocations()
    locations.value = response.data 
  } catch (error) {
    console.error('Błąd pobierania lokalizacji:', error)
  } finally {
    isLoadingLocations.value = false
  }
})

const toggleMode = () => {
  isLoginMode.value = !isLoginMode.value
  // Reset pól przy przełączaniu
  form.password = ''
  form.confirmPassword = ''
}

const handleSubmit = async () => {
  if (!isLoginMode.value && form.password !== form.confirmPassword) {
    alert("Hasła nie są identyczne!")
    return
  }

  try {
    isSubmitting.value = true
    
    if (isLoginMode.value) {
      // LOGOWANIE: Wymaga login i password
      const res = await authService.login({ 
        login: form.login, 
        password: form.password 
      })
      
      // Zapisujemy token i login użytkownika do LocalStorage
      localStorage.setItem('token', res.data.token)
      localStorage.setItem('userLogin', form.login)
      
      // Jeżeli API zwraca rolę, zapisz ją:
      if (res.data.role) {
         localStorage.setItem('role', res.data.role);
      }

      alert('Zalogowano pomyślnie!')
      window.location.reload()
    } else {
      // REJESTRACJA: Wymaga login, email, password, locationId
      await authService.register({
        login: form.login,
        email: form.email,
        password: form.password,
        locationId: form.locationId,
        roleId: 1 // Wartość domyślna dla użytkowników (zgodnie z Twoim przykładem API)
      })
      alert('Konto zostało utworzone! Możesz się teraz zalogować.')
      isLoginMode.value = true
    }
    
    emit('close')
  } catch (error) {
    const msg = error.response?.data?.message || 'Błąd połączenia z serwerem'
    alert(msg)
  } finally {
    isSubmitting.value = false
  }
}
</script>

<style scoped>
.modal-overlay { position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; background: rgba(0, 0, 0, 0.5); backdrop-filter: blur(4px); display: flex; justify-content: center; align-items: center; z-index: 10000; }
.modal-container { background: white; width: 90%; max-width: 420px; padding: 2.5rem; border-radius: 1.25rem; position: relative; box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25); }
.close-x { position: absolute; top: 1.25rem; right: 1.25rem; background: none; border: none; font-size: 1.5rem; cursor: pointer; color: #9ca3af; }
.modal-header { text-align: center; margin-bottom: 2rem; }
.logo-small { color: #2563eb; font-size: 1.1rem; font-weight: 800; margin-bottom: 0.5rem; }
.subtitle { color: #6b7280; font-size: 0.9rem; }
.auth-form { display: flex; flex-direction: column; gap: 1.25rem; }
.form-group { display: flex; flex-direction: column; gap: 0.4rem; }
.form-group label { font-size: 0.85rem; font-weight: 600; color: #374151; }
input, select { padding: 0.75rem; border: 1px solid #d1d5db; border-radius: 0.5rem; font-size: 1rem; transition: border-color 0.2s; outline: none; }
input:focus, select:focus { border-color: #2563eb; box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1); }
.btn-submit { background: #2563eb; color: white; border: none; padding: 0.85rem; border-radius: 0.5rem; font-weight: 600; cursor: pointer; margin-top: 0.5rem; transition: background 0.2s; }
.btn-submit:hover:not(:disabled) { background: #1d4ed8; }
.btn-submit:disabled { opacity: 0.6; cursor: not-allowed; }
.modal-footer { margin-top: 1.5rem; text-align: center; font-size: 0.9rem; color: #6b7280; }
.modal-footer span { color: #2563eb; font-weight: 700; cursor: pointer; }
.modal-footer span:hover { text-decoration: underline; }
</style>