<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-container">
      <button class="close-x" @click="$emit('close')">&times;</button>

      <header class="modal-header">
        <h2>Przypisz Serwisanta (Zgłoszenie #{{ reportId }})</h2>
        <p>Wpisz login pracownika, który zajmie się tym problemem.</p>
      </header>
      
      <form @submit.prevent="handleSubmit">
        
        <div class="form-group">
          <label for="assignee-login">Login pracownika:</label>
          <input 
            id="assignee-login" 
            v-model="selectedLogin" 
            type="text"
            placeholder="Wpisz login, np. marek_serwisant"
            :required="!isUnassignMode"
            :disabled="isUnassignMode"
          />
        </div>

        <div class="unassign-option">
            <input type="checkbox" id="unassign-check" v-model="isUnassignMode">
            <label for="unassign-check">Usuń przydział (pozostaw zgłoszenie bez serwisanta)</label>
        </div>

        <div class="modal-footer">
          <button type="button" class="btn-cancel" @click="$emit('close')">Anuluj</button>
          <button type="submit" class="btn-submit" :disabled="isSubmitting || (selectedLogin === '' && !isUnassignMode)">
            {{ isSubmitting ? 'Zapisywanie...' : 'Zapisz przydział' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { reportService } from '../services/reportService'

const props = defineProps({
  reportId: { type: [Number, String], required: true },
  currentAssigneeLogin: { type: String, default: '' }
})
const emit = defineEmits(['close', 'assigned'])

const isSubmitting = ref(false)
const selectedLogin = ref(props.currentAssigneeLogin || '')
const isUnassignMode = ref(false)

// Zmiana trybu kasowania zeruje login
watch(isUnassignMode, (newValue) => {
    if (newValue) {
        selectedLogin.value = '';
    } else if (props.currentAssigneeLogin) {
        // Przywracamy obecnego, jeśli tryb anulowania jest wyłączony
        selectedLogin.value = props.currentAssigneeLogin;
    }
});

const handleSubmit = async () => {
    // Jeśli wybrano tryb usuwania, loginem do przypisania jest null
    const loginToAssign = isUnassignMode.value ? null : selectedLogin.value;

    if (!loginToAssign && !isUnassignMode.value) {
        alert('Wpisz login lub wybierz opcję usunięcia przydziału.');
        return;
    }
    
    try {
        isSubmitting.value = true;
        
        await reportService.assignReport(props.reportId, loginToAssign);
        
        alert(`Zgłoszenie #${props.reportId} pomyślnie przypisane do: ${loginToAssign || 'NIKOGO'}`);
        
        emit('assigned');
        emit('close');
        
    } catch (err) {
        console.error("Błąd przypisywania:", err);
        alert('Nie udało się przypisać zgłoszenia. Sprawdź, czy login jest poprawny.');
    } finally {
        isSubmitting.value = false;
    }
}
</script>

<style scoped>
.modal-overlay { position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; background: rgba(0, 0, 0, 0.5); backdrop-filter: blur(4px); display: flex; justify-content: center; align-items: center; z-index: 10001; }
.modal-container { background: white; width: 90%; max-width: 450px; padding: 2rem; border-radius: 12px; box-shadow: 0 10px 20px rgba(0, 0, 0, 0.2); position: relative; }
.close-x { position: absolute; top: 1rem; right: 1rem; background: none; border: none; font-size: 1.5rem; cursor: pointer; color: #9ca3af; }
.modal-header h2 { color: #1f2937; margin-bottom: 0.5rem; }
.modal-header p { color: #6b7280; margin-bottom: 1.5rem; }

.form-group label { display: block; font-weight: 600; margin-bottom: 0.5rem; color: #374151; }
input[type="text"] { 
    width: 100%; 
    padding: 0.75rem; 
    border: 1px solid #d1d5db; 
    border-radius: 0.5rem; 
    font-size: 1rem; 
    transition: border-color 0.2s;
}
input[type="text"]:disabled {
    background-color: #f9fafb;
}

.unassign-option {
    display: flex;
    align-items: center;
    gap: 10px;
    margin-top: 15px;
    font-size: 0.9rem;
    color: #4b5563;
    cursor: pointer;
}

.modal-footer { display: flex; justify-content: flex-end; gap: 1rem; margin-top: 2rem; }
.btn-cancel { padding: 0.75rem 1.5rem; background: #f3f4f6; border: none; border-radius: 0.5rem; cursor: pointer; font-weight: 500; }
.btn-submit { padding: 0.75rem 1.5rem; background: #2563eb; color: white; border: none; border-radius: 0.5rem; cursor: pointer; font-weight: 600; }
.btn-submit:disabled { opacity: 0.6; cursor: not-allowed; }
</style>