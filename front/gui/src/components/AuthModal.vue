<template>
  <!-- Ściemnione tło -->
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-container">
      <!-- Przycisk X -->
      <button class="close-x" @click="$emit('close')">&times;</button>

      <header class="modal-header">
        <h1 class="logo-small">REPORTMEPLZ</h1>
        <h2>{{ isLoginMode ? 'Zaloguj się' : 'Zarejestruj się' }}</h2>
      </header>

      <form @submit.prevent="handleSubmit" class="auth-form">
        <!-- Login -->
        <div class="form-group">
          <label>Login</label>
          <input v-model="form.username" type="text" placeholder="Twój login" required />
        </div>

        <!-- Email (Tylko rejestracja) -->
        <div v-if="!isLoginMode" class="form-group">
          <label>Email</label>
          <input v-model="form.email" type="email" placeholder="email@przyklad.pl" required />
        </div>

        <!-- Hasło -->
        <div class="form-group">
          <label>Hasło</label>
          <input v-model="form.password" type="password" placeholder="••••••••" required />
        </div>

        <!-- Powtórz Hasło (Tylko rejestracja) -->
        <div v-if="!isLoginMode" class="form-group">
          <label>Powtórz hasło</label>
          <input v-model="form.confirmPassword" type="password" placeholder="••••••••" required />
        </div>

        <button type="submit" class="btn-primary">
          {{ isLoginMode ? 'Zaloguj się' : 'Stwórz konto' }}
        </button>
      </form>

      <footer class="modal-footer">
        <p v-if="isLoginMode">
          Nie masz konta? <span @click="isLoginMode = false">Zarejestruj się</span>
        </p>
        <p v-else>
          Masz już konto? <span @click="isLoginMode = true">Zaloguj się</span>
        </p>
      </footer>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'

const emit = defineEmits(['close'])
const isLoginMode = ref(true)

const form = reactive({
  username: '',
  email: '',
  password: '',
  confirmPassword: ''
})

const handleSubmit = () => {
  if (!isLoginMode.value && form.password !== form.confirmPassword) {
    alert("Hasła nie są takie same!")
    return
  }
  console.log('Dane autoryzacji:', form)
  alert(isLoginMode.value ? 'Zalogowano!' : 'Konto stworzone!')
  emit('close')
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(4px);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 10000;
}

.modal-container {
  background: white;
  width: 90%;
  max-width: 400px;
  padding: 2.5rem;
  border-radius: 1.25rem;
  position: relative;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  animation: modalPop 0.3s ease-out;
}

@keyframes modalPop {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}

.close-x {
  position: absolute;
  top: 1.25rem;
  right: 1.25rem;
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #9ca3af;
}

.modal-header {
  text-align: center;
  margin-bottom: 2rem;
}

.logo-small {
  color: #2563eb;
  font-size: 1.1rem;
  font-weight: 800;
  margin-bottom: 0.5rem;
}

.auth-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.form-group label {
  font-size: 0.85rem;
  font-weight: 600;
  color: #374151;
}

input {
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 0.5rem;
  font-size: 1rem;
  outline: none;
  transition: border-color 0.2s;
}

input:focus {
  border-color: #2563eb;
}

.btn-primary {
  background: #2563eb;
  color: white;
  border: none;
  padding: 0.8rem;
  border-radius: 0.5rem;
  font-weight: 600;
  cursor: pointer;
  margin-top: 0.5rem;
}

.btn-primary:hover {
  background: #1d4ed8;
}

.modal-footer {
  margin-top: 1.5rem;
  text-align: center;
  font-size: 0.9rem;
  color: #6b7280;
}

.modal-footer span {
  color: #2563eb;
  font-weight: 700;
  cursor: pointer;
}
</style>