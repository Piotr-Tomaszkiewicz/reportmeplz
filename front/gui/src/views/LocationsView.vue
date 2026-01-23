<template>
  <div class="admin-locations">
    <div class="header-actions">
      <p class="desc">Zarządzaj listą oddziałów i biur w systemie.</p>
      <button @click="openModal()" class="btn-add">+ Dodaj Lokalizację</button>
    </div>

    <!-- Tabela Lokalizacji -->
    <div v-if="loading" class="loading">Pobieranie danych...</div>
    <div v-else class="table-container">
      <table class="admin-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Nazwa skrócona</th>
            <th>Pełny adres</th>
            <th>Akcje</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="loc in locations" :key="loc.id">
            <td>{{ loc.id }}</td>
            <td class="bold">{{ loc.shortName }}</td>
            <td>{{ loc.fullAddress }}</td>
            <td class="actions">
              <button @click="openModal(loc)" class="btn-edit">Edytuj</button>
              <button @click="confirmDelete(loc)" class="btn-delete">Usuń</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- MODAL FORMULARZA (DODAWANIE / EDYCJA) -->
    <div v-if="showModal" class="modal-overlay" @click.self="showModal = false">
      <div class="modal-box">
        <h3>{{ isEditMode ? 'Edytuj Lokalizację' : 'Nowa Lokalizacja' }}</h3>
        <form @submit.prevent="saveLocation">
          <div class="form-group">
            <label>Nazwa skrócona</label>
            <input v-model="form.shortName" type="text" placeholder="np. Biuro A" required />
          </div>
          <div class="form-group">
            <label>Pełny adres</label>
            <input v-model="form.fullAddress" type="text" placeholder="ul. Przykładowa 10, Miasto" required />
          </div>
          <div class="modal-btns">
            <button type="button" @click="showModal = false" class="btn-cancel">Anuluj</button>
            <button type="submit" class="btn-save" :disabled="submitting">
              {{ submitting ? 'Zapisywanie...' : 'Zapisz' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, reactive } from 'vue'
// POPRAWIONA ŚCIEŻKA: Jeden poziom w górę (../)
import { locationService } from '../services/locationService'

const locations = ref([])
const loading = ref(true)
const showModal = ref(false)
const isEditMode = ref(false)
const submitting = ref(false)

const form = reactive({
  id: null,
  shortName: '',
  fullAddress: ''
})

const fetchLocations = async () => {
  loading.value = true
  try {
    const res = await locationService.getLocations()
    // Sortowanie alfabetyczne
    locations.value = res.data.sort((a,b) => a.shortName.localeCompare(b.shortName))
  } catch (err) {
    console.error(err)
    alert("Błąd pobierania danych.")
  } finally {
    loading.value = false
  }
}

onMounted(fetchLocations)

const openModal = (loc = null) => {
  if (loc) {
    isEditMode.value = true
    form.id = loc.id
    form.shortName = loc.shortName
    form.fullAddress = loc.fullAddress
  } else {
    isEditMode.value = false
    form.id = null
    form.shortName = ''
    form.fullAddress = ''
  }
  showModal.value = true
}

const saveLocation = async () => {
  submitting.value = true
  try {
    if (isEditMode.value) {
      await locationService.updateLocation(form.id, { ...form })
    } else {
      await locationService.createLocation({ 
        shortName: form.shortName, 
        fullAddress: form.fullAddress 
      })
    }
    showModal.value = false
    await fetchLocations()
  } catch (err) {
    alert("Błąd podczas zapisywania lokalizacji.")
  } finally {
    submitting.value = false
  }
}

const confirmDelete = async (loc) => {
  if (!confirm(`Czy na pewno usunąć lokalizację: ${loc.shortName}?`)) return
  try {
    await locationService.deleteLocation(loc.id)
    await fetchLocations()
  } catch (err) {
    alert("Nie można usunąć lokalizacji. Prawdopodobnie jest używana przez zgłoszenia.")
  }
}
</script>

<style scoped>
.admin-locations { animation: fadeIn 0.3s; }
.header-actions { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.desc { color: #6b7280; font-size: 0.95rem; }

.btn-add { background: #10b981; color: white; border: none; padding: 10px 20px; border-radius: 8px; cursor: pointer; font-weight: 600; }
.admin-table { width: 100%; border-collapse: collapse; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
.admin-table th { background: #f9fafb; text-align: left; padding: 12px 15px; font-size: 0.85rem; color: #4b5563; text-transform: uppercase; border-bottom: 1px solid #e5e7eb; }
.admin-table td { padding: 12px 15px; border-bottom: 1px solid #f3f4f6; font-size: 0.95rem; }
.bold { font-weight: 600; color: #111827; }

.actions { display: flex; gap: 10px; }
.btn-edit { color: #2563eb; background: #eff6ff; border: 1px solid #bfdbfe; padding: 5px 10px; border-radius: 4px; cursor: pointer; }
.btn-delete { color: #dc2626; background: #fef2f2; border: 1px solid #fecaca; padding: 5px 10px; border-radius: 4px; cursor: pointer; }

/* Modal Styles */
.modal-overlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.4); display: flex; align-items: center; justify-content: center; z-index: 2000; }
.modal-box { background: white; padding: 30px; border-radius: 12px; width: 400px; box-shadow: 0 20px 25px -5px rgba(0,0,0,0.1); }
.form-group { margin-bottom: 15px; }
.form-group label { display: block; margin-bottom: 5px; font-weight: 600; font-size: 0.9rem; }
.form-group input { width: 100%; padding: 10px; border: 1px solid #d1d5db; border-radius: 6px; }
.modal-btns { display: flex; justify-content: flex-end; gap: 10px; margin-top: 20px; }
.btn-cancel { background: #f3f4f6; border: none; padding: 10px 15px; border-radius: 6px; cursor: pointer; }
.btn-save { background: #2563eb; color: white; border: none; padding: 10px 20px; border-radius: 6px; cursor: pointer; font-weight: 600; }

@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
</style>