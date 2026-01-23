<template>
  <div class="admin-users">
    <div class="header-actions">
      <div class="search-box">
        <span class="search-icon">🔍</span>
        <input 
          v-model="searchQuery" 
          type="text" 
          placeholder="Szukaj użytkownika (login lub email)..." 
        />
      </div>
    </div>

    <!-- Tabela Użytkowników -->
    <div v-if="loading" class="loading">Pobieranie listy użytkowników...</div>
    <div v-else class="table-container">
      <table class="admin-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Login</th>
            <th>Email</th>
            <th>Rola</th>
            <th>Lokalizacja</th>
            <th>Akcje</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in filteredUsers" :key="user.id">
            <td>{{ user.id }}</td>
            <td class="bold">{{ user.login }}</td>
            <td>{{ user.email }}</td>
            <td>
              <span class="role-badge" :class="user.role?.toLowerCase()">
                {{ user.role || 'user' }}
              </span>
            </td>
            <td>{{ user.locationShortName || 'Brak' }}</td>
            <td class="actions">
              <button @click="openEditModal(user)" class="btn-edit">Edytuj</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- MODAL EDYCJI UŻYTKOWNIKA -->
    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-box">
        <h3>Edytuj Użytkownika #{{ form.id }}</h3>
        
        <form @submit.prevent="handleUpdate">
          <div class="form-grid">
            <div class="form-group">
              <label>Login</label>
              <input v-model="form.login" type="text" required />
            </div>

            <div class="form-group">
              <label>Email</label>
              <input v-model="form.email" type="email" required />
            </div>

            <div class="form-group">
              <label>Lokalizacja</label>
              <select v-model="form.locationId" required>
                <option v-for="loc in locations" :key="loc.id" :value="loc.id">
                  {{ loc.shortName }}
                </option>
              </select>
            </div>

            <div class="form-group">
              <label>Rola</label>
              <select v-model="form.roleId" required>
                <option v-for="role in roles" :key="role.id" :value="role.id">
                  {{ role.name.toUpperCase() }}
                </option>
              </select>
            </div>

            <div class="form-group">
              <label>Nowe Hasło (opcjonalnie)</label>
              <input v-model="form.password" type="password" placeholder="Wpisz, aby zmienić" />
            </div>

            <div class="form-group">
              <label>Powtórz Hasło</label>
              <input v-model="form.confirmPassword" type="password" placeholder="Powtórz nowe hasło" />
            </div>
          </div>

          <div class="modal-btns">
            <button type="button" @click="closeModal" class="btn-cancel">Anuluj</button>
            <button type="submit" class="btn-save" :disabled="submitting">
              {{ submitting ? 'Zapisywanie...' : 'Zapisz zmiany' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, reactive, computed } from 'vue'
import { userService } from '../services/userService'
import { locationService } from '../services/locationService'

const users = ref([])
const locations = ref([])
const loading = ref(true)
const showModal = ref(false)
const submitting = ref(false)
const searchQuery = ref('')

// Stała lista ról zgodna z Twoim systemem
const roles = [
  { id: 1, name: 'user' },
  { id: 2, name: 'worker' },
  { id: 3, name: 'manager' },
  { id: 4, name: 'admin' }
]

const form = reactive({
  id: null,
  login: '',
  email: '',
  locationId: null,
  roleId: null,
  password: '',
  confirmPassword: ''
})

const fetchData = async () => {
  loading.value = true
  try {
    const [usersRes, locsRes] = await Promise.all([
      userService.getAllUsers(),
      locationService.getLocations()
    ])
    users.value = usersRes.data
    locations.value = locsRes.data
  } catch (err) {
    alert("Błąd pobierania danych.")
  } finally {
    loading.value = false
  }
}

onMounted(fetchData)

// Logika wyszukiwania
const filteredUsers = computed(() => {
  const q = searchQuery.value.toLowerCase()
  return users.value.filter(u => 
    u.login.toLowerCase().includes(q) || 
    u.email.toLowerCase().includes(q)
  )
})

const openEditModal = (user) => {
  form.id = user.id
  form.login = user.login
  form.email = user.email
  form.locationId = user.locationId
  // Mapowanie nazwy roli na ID dla selecta
  const userRole = roles.find(r => r.name.toLowerCase() === user.role?.toLowerCase())
  form.roleId = userRole ? userRole.id : 1
  form.password = ''
  form.confirmPassword = ''
  showModal.value = true
}

const closeModal = () => {
  showModal.value = false
}

const handleUpdate = async () => {
  // Walidacja haseł
  if (form.password && form.password !== form.confirmPassword) {
    alert("Hasła nie są identyczne!")
    return
  }

  submitting.value = true
  try {
    const payload = {
      login: form.login,
      email: form.email,
      locationId: form.locationId,
      roleId: form.roleId,
      password: form.password || null // Wyślij null jeśli nie zmieniamy hasła
    }

    await userService.updateUser(form.id, payload)
    alert("Dane użytkownika zostały zaktualizowane.")
    showModal.value = false
    await fetchData()
  } catch (err) {
    alert(err.response?.data?.message || "Błąd podczas zapisu.")
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
.admin-users { animation: fadeIn 0.3s; }
.header-actions { margin-bottom: 25px; }

/* Search Box */
.search-box { position: relative; max-width: 400px; }
.search-icon { position: absolute; left: 12px; top: 50%; transform: translateY(-50%); color: #9ca3af; }
.search-box input { width: 100%; padding: 12px 12px 12px 40px; border: 1px solid #e5e7eb; border-radius: 10px; outline: none; }
.search-box input:focus { border-color: #2563eb; }

/* Tabela */
.admin-table { width: 100%; border-collapse: collapse; background: white; border-radius: 8px; overflow: hidden; }
.admin-table th { text-align: left; padding: 15px; background: #f9fafb; border-bottom: 1px solid #e5e7eb; font-size: 0.8rem; color: #6b7280; text-transform: uppercase; }
.admin-table td { padding: 15px; border-bottom: 1px solid #f3f4f6; }
.bold { font-weight: 600; }

/* Badges dla ról */
.role-badge { padding: 4px 8px; border-radius: 6px; font-size: 0.75rem; font-weight: 700; text-transform: uppercase; }
.role-badge.admin { background: #fef2f2; color: #dc2626; }
.role-badge.manager { background: #fff7ed; color: #ea580c; }
.role-badge.worker { background: #eff6ff; color: #2563eb; }
.role-badge.user { background: #f3f4f6; color: #4b5563; }

.btn-edit { color: #2563eb; background: #eff6ff; border: 1px solid #bfdbfe; padding: 6px 12px; border-radius: 6px; cursor: pointer; font-weight: 600; }

/* Modal */
.modal-overlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.4); display: flex; align-items: center; justify-content: center; z-index: 2000; }
.modal-box { background: white; padding: 30px; border-radius: 15px; width: 500px; box-shadow: 0 25px 50px -12px rgba(0,0,0,0.25); }
.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; }
.form-group { display: flex; flex-direction: column; gap: 5px; margin-bottom: 10px; }
.form-group label { font-weight: 600; font-size: 0.85rem; color: #374151; }
.form-group input, .form-group select { padding: 10px; border: 1px solid #d1d5db; border-radius: 8px; font-size: 0.95rem; }

.modal-btns { display: flex; justify-content: flex-end; gap: 12px; margin-top: 25px; }
.btn-cancel { background: #f3f4f6; border: none; padding: 10px 20px; border-radius: 8px; cursor: pointer; font-weight: 600; }
.btn-save { background: #2563eb; color: white; border: none; padding: 10px 20px; border-radius: 8px; cursor: pointer; font-weight: 600; }
.btn-save:disabled { opacity: 0.5; }

@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
</style>