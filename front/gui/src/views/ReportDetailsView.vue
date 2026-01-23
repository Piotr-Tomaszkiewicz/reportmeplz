<template>
  <div class="report-details-container">
    
    <div v-if="loading" class="info-msg">
      <div class="spinner"></div>
      <p>Ładowanie szczegółów zgłoszenia...</p>
    </div>

    <div v-else-if="!report" class="info-msg error">
      <p>Nie znaleziono zgłoszenia o ID: {{ $route.params.id }}</p>
      <button @click="router.back()" class="btn-back">← Powrót</button>
    </div>

    <div v-else>
      
      <!-- Nagłówek i Akcje -->
      <header class="report-header">
        
        <!-- Guzik Powrotu -->
        <button @click="router.back()" class="btn-back">
          ← Powrót do listy
        </button>

        <div class="title-section">
          <span class="report-id">#{{ report.id }}</span>
          <h1>{{ report.title }}</h1>
          <span class="status-tag" :class="getStatusClass(report.status)">
            {{ report.status }}
          </span>
        </div>

        <div class="action-buttons">
          
          <!-- PRZYCISK ROZWIĄŻ -->
          <button 
            v-if="canResolve"
            @click="handleResolve"
            class="btn-action btn-resolve"
            :disabled="isResolving"
          >
            {{ isResolving ? 'Zamykanie...' : '✅ Rozwiąż zgłoszenie' }}
          </button>
          
          <!-- PRZYCISK PRZYPISZ / ZMIEŃ PRZYPISANIE -->
          <button 
            v-if="canAssign"
            @click="showAssignModal = true"
            class="btn-action btn-assign"
          >
            {{ report.assigneeNick && report.assigneeNick !== 'Oczekuje na przydział' ? 'Zmień serwisanta' : 'Przypisz serwisanta' }}
          </button>

          <!-- GUZIK ANULUJ -->
          <button 
            v-if="canCancelReport"
            @click="handleCancel"
            class="btn-action btn-cancel"
            :disabled="isCancelling"
          >
            {{ isCancelling ? 'Anulowanie...' : 'Anuluj zgłoszenie' }}
          </button>
        </div>
      </header>

      <!-- SEKCJA UŻYTKOWNIKÓW I PRIORYTETU -->
      <section class="user-priority-section">
        
        <!-- KARTA ZGŁASZAJĄCEGO -->
        <div class="user-info-card">
          <h2>Zgłaszający</h2>
          <p>👤 Login: <strong>{{ report.reporterNick || 'Brak' }}</strong></p>
          
          <p v-if="reporterDetailsLoading">Ładowanie danych kontaktowych...</p>
          <template v-else>
            <p>📧 Email: {{ getReporterDetail('Email') }}</p>
            <p>📍 Lokalizacja: {{ getReporterDetail('Location') }}</p>
          </template>
        </div>

        <!-- KARTA PRZYPISANEGO -->
        <div class="user-info-card">
          <h2>Przypisany Serwisant</h2>
          <p class="assignee-status" :class="{ 'assigned': report.assigneeNick }">
            🛠 {{ report.assigneeNick || 'Oczekuje na przydział' }}
          </p>
        </div>

        <!-- KARTA PRIORYTETU -->
        <div class="priority-card" :class="'prio-' + report.priority">
          <span class="icon-prio">{{ getPriorityIcon(report.priority) }}</span>
          <div>
            <h3>Priorytet:</h3>
            <p>{{ getPriorityName(report.priority) }}</p>
          </div>
        </div>
      </section>
      
      <!-- SEKCJA OPISU I DATY -->
      <section class="details-section">
        <h2>Opis problemu</h2>
        <p class="description">{{ report.description || 'Brak szczegółowego opisu.' }}</p>
        
        <div class="report-dates">
          <p>📅 Zgłoszono: <strong>{{ formatDateTime(report.dateReported) }}</strong></p>
          <p v-if="report.dateResolved">✅ Rozwiązano: <strong>{{ formatDateTime(report.dateResolved) }}</strong></p>
        </div>
      </section>

      <!-- ZAŁĄCZNIK -->
      <section v-if="report.fileName" class="attachment-section">
        <h2>Załączony plik</h2>
        <button @click="downloadAttachment(report.id, report.fileName)" class="btn-download">
          ⬇️ {{ report.fileName }} (Pobierz)
        </button>
      </section>
      <section v-else class="attachment-section">
        <h2>Załączniki</h2>
        <p class="no-attachment">Brak załączników.</p>
      </section>
    </div>
    
    <!-- MODAL PRZYPISYWANIA -->
    <AssigneeModal
        v-if="showAssignModal"
        :report-id="report.id"
        :current-assignee-login="report.assigneeNick"
        @close="showAssignModal = false"
        @assigned="refreshReportData"
    />
  </div>
</template>

<script setup>
import { ref, onMounted, computed, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { reportService } from '../services/reportService'
import { userService } from '../services/userService' 
import { useAuthStore } from '../stores/authStore' 
import AssigneeModal from '../components/AssigneeModal.vue'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore() 

const report = ref(null)
const loading = ref(true)
const isCancelling = ref(false)
const isResolving = ref(false)
const reporterDetailsLoading = ref(true)
const reporterDetails = ref({ email: 'Brak', location: 'Brak' }) 

const showAssignModal = ref(false)

const currentUserLogin = localStorage.getItem('userLogin') || ''
const currentUserRole = authStore.role.toLowerCase() 


const refreshReportData = async () => {
    loading.value = true;
    await nextTick(); 
    await loadReportData();
};

const loadReportData = async () => {
    const reportId = route.params.id;
    try {
        if (authStore.isLoggedIn && !authStore.userProfile) {
            await authStore.fetchUserProfile();
        }

        const response = await reportService.getReportById(reportId);
        
        report.value = {
            ...response.data,
            // Normalizacja pól z API
            reporterNick: response.data.reporterNick || response.data.ReporterNick,
            assigneeNick: response.data.assigneeNick || response.data.AssigneeNick,
            status: response.data.status || response.data.Status || 'Nieznany',
            fileName: response.data.fileName || response.data.FileName,
            reporterId: response.data.reporterId,
            dateReported: response.data.dateReported, // NOWA DATA
            dateResolved: response.data.dateResolved, // NOWA DATA
        };
        
        if (report.value.reporterId) {
            await fetchReporterDetails(report.value.reporterId);
        }
        
    } catch (err) {
        console.error('Błąd pobierania zgłoszenia:', err);
        if (err.response && err.response.status === 404) {
            report.value = null; 
        }
    } finally {
        loading.value = false;
    }
};

onMounted(() => {
    loadReportData();
});


// FUNKCJA POBIERAJĄCA DANE KONTAKTOWE ZGŁASZAJĄCEGO (POPRAWIONA LOGIKA ROLEK)
async function fetchReporterDetails(reporterId) {
    reporterDetailsLoading.value = true;
    try {
        const isMyReportReporter = isReporterMe.value;
        let response = null;

        // Logika 1: Ja jestem zgłaszającym, używam Pinia (dane już załadowane z /users/me)
        if (isMyReportReporter && authStore.userProfile) {
            reporterDetails.value.email = authStore.userProfile.email;
            reporterDetails.value.location = authStore.userProfile.location;
            return;
        }

        // Logika 2: Mam uprawnienia do podglądu danych innych użytkowników (Worker, Admin, Manager)
        if (currentUserRole === 'admin' || currentUserRole === 'manager' || currentUserRole === 'worker') {
            response = await userService.getUserById(reporterId);
        } else {
            // Logika 3: Zwykły user (rola 'user') nie ma uprawnień
            return; 
        }

        if (response?.data) {
             const data = response.data;
             reporterDetails.value.email = data.email || data.Email || 'Brak Emaila';
             reporterDetails.value.location = data.locationShortName || data.LocationShortName || 'Brak Lokalizacji';
        }
        
    } catch (err) {
        console.error("Błąd pobierania detali zgłaszającego:", err);
        reporterDetails.value.email = 'Brak (Błąd API)';
        reporterDetails.value.location = 'Brak (Błąd API)';
    } finally {
        reporterDetailsLoading.value = false;
    }
}


// === LOGIKA AKCJI I WARUNKÓW (Bez zmian) ===

const isAdminOrManager = computed(() => {
    return currentUserRole === 'admin' || currentUserRole === 'manager'
})

const isReporterMe = computed(() => {
    if (!report.value) return false
    const reporter = report.value.reporterNick || report.value.ReporterNick
    return currentUserLogin.toLowerCase() === (reporter || '').toLowerCase()
})

const isActiveStatus = computed(() => {
    const status = (report.value?.status || '').toLowerCase()
    return status === 'zarejestrowany' || status.includes('realizacji') || status.includes('toku')
})

const canAssign = computed(() => {
    if (!isAdminOrManager.value || !report.value) return false;
    const status = (report.value.status || '').toLowerCase();
    return status === 'zarejestrowany' || status.includes('realizacji') || status.includes('toku');
});

const isAssignedToMe = computed(() => {
    if (!report.value) return false;
    const assignee = report.value.assigneeNick || report.value.AssigneeNick;
    return currentUserLogin.toLowerCase() === (assignee || '').toLowerCase();
});

const canResolve = computed(() => {
    if (!report.value || !isActiveStatus.value) return false;
    
    if (isAdminOrManager.value) return true;
    
    if (currentUserRole === 'worker' && isAssignedToMe.value) {
        return true;
    }
    
    return false;
});


const canCancelReport = computed(() => {
    return isReporterMe.value && isActiveStatus.value;
})

const handleResolve = async () => {
    if (!confirm(`Czy na pewno chcesz oznaczyć zgłoszenie #${report.value.id} jako ROZWIĄZANE?`)) {
        return
    }
    try {
        isResolving.value = true;
        await reportService.resolveReport(report.value.id);
        
        alert('Status zmieniony na: Rozwiązane!');
        await refreshReportData();
        
    } catch (err) {
        console.error('Błąd rozwiązywania:', err);
        let msg = 'Błąd serwera.';
        if (err.response && err.response.status === 404) {
            msg = 'Błąd 404: Endpoint PUT /reports/{id}/resolve nie istnieje w API!';
        } else if (err.response && err.response.status === 403) {
             msg = 'Brak uprawnień do rozwiązania tego zgłoszenia.';
        }
        alert('Nie udało się rozwiązać zgłoszenia: ' + msg);
    } finally {
        isResolving.value = false;
    }
}


const handleCancel = async () => {
    if (!confirm(`Czy na pewno chcesz ANULOWAĆ zgłoszenie #${report.value.id}? Operacji nie można cofnąć.`)) {
        return
    }
    try {
        isCancelling.value = true
        await reportService.cancelReport(report.value.id)
        
        alert('Zgłoszenie zostało anulowane pomyślnie.')
        await refreshReportData()
    } catch (err) {
        console.error('Błąd anulowania:', err)
        alert('Nie udało się anulować zgłoszenia: ' + (err.message || 'Błąd serwera.'));
    } finally {
        isCancelling.value = false
    }
}

const downloadAttachment = async (id, fileName) => {
  try {
    document.body.style.cursor = 'wait'
    await reportService.downloadFile(id, fileName)
  } catch (err) {
    alert(`Błąd: ${err.message}.`)
    console.error(err)
  } finally {
    document.body.style.cursor = 'default'
  }
}

// FUNKCJA POMOCNICZA DO PÓL EMAIL/LOKALIZACJA
const getReporterDetail = (field) => {
    if (reporterDetailsLoading.value) return 'Ładowanie...'
    if (field === 'Email') {
        return reporterDetails.value.email || 'Brak';
    }
    if (field === 'Location') {
        return reporterDetails.value.location || 'Brak';
    }
    return 'Brak';
}


// === POMOCNICZE FUNKCJE WIZUALNE ===

const formatDateTime = (dateString) => {
  if (!dateString) return '---';
  try {
    const options = { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' };
    return new Date(dateString).toLocaleDateString('pl-PL', options);
  } catch {
    return 'Błąd daty';
  }
}

const getPriorityIcon = (p) => {
    switch(p) { case 3: return '🔥'; case 2: return '⚠️'; case 1: return '🟢'; default: return '⚪'; }
}

const getPriorityName = (p) => {
    switch(p) { case 1: return 'Zwykły'; case 2: return 'Wysoki'; case 3: return 'Krytyczny'; default: return 'Nieznany'; }
}

const getStatusClass = (status) => {
    if (!status) return 'default'
    const s = status.toLowerCase()
    if (s.includes('zarejestrowany')) return 'zarejestrowany'
    if (s.includes('realizacji') || s.includes('toku')) return 'w_realizacji'
    if (s.includes('zamknięty') || s.includes('zakończony') || s.includes('rozwiązane')) return 'zamkniety'
    if (s.includes('anulowane')) return 'anulowane'
    return 'default'
}

const truncateText = (text, limit) => {
    return text;
}
</script>

<style scoped>
.report-details-container { max-width: 900px; margin: 0 auto; }
.btn-back { background: #f3f4f6; color: #4b5563; padding: 8px 15px; border: none; border-radius: 6px; cursor: pointer; font-weight: 500; transition: background 0.2s; margin-bottom: 1.5rem; display: inline-block; text-decoration: none; }
.btn-back:hover { background: #e5e7eb; }
.report-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; padding-bottom: 1rem; border-bottom: 1px solid #e5e7eb; }
.title-section { display: flex; align-items: center; gap: 15px; }
.report-id { font-size: 1.5rem; font-weight: 700; color: #9ca3af; }
.report-header h1 { font-size: 2rem; color: #111827; margin: 0; }
.status-tag { padding: 6px 12px; border-radius: 20px; font-size: 0.9rem; font-weight: 700; text-transform: uppercase; }
.status-tag.zarejestrowany { background: #eff6ff; color: #2563eb; }
.status-tag.w_realizacji { background: #fff7ed; color: #ea580c; }
.status-tag.zamkniety { background: #ecfdf5; color: #059669; }
.status-tag.anulowane { background: #fee2e2; color: #dc2626; }
.status-tag.default { background: #f3f4f6; color: #4b5563; }
.action-buttons { display: flex; gap: 10px; }
.btn-action { padding: 10px 15px; border-radius: 8px; font-weight: 600; cursor: pointer; transition: background 0.2s; border: 1px solid transparent; }
.btn-cancel { background: #fef2f2; color: #dc2626; border-color: #fca5a5; }
.btn-cancel:hover { background: #fee2e2; }
.btn-assign { background-color: #e0f2f1; color: #0f766e; border-color: #99f6e4; }
.btn-assign:hover { background-color: #b9f7ed; }
.btn-resolve { background-color: #d1fae5; color: #059669; border-color: #a7f3d0; }
.btn-resolve:hover { background-color: #a7f3d0; }
.user-priority-section { display: grid; grid-template-columns: repeat(3, 1fr); gap: 1.5rem; margin-bottom: 2rem; }
.user-info-card, .priority-card { background: white; padding: 1.5rem; border-radius: 12px; box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05); border: 1px solid #e5e7eb; }
.user-info-card h2, .priority-card h3 { font-size: 1.1rem; color: #1f2937; margin-bottom: 1rem; }
.user-info-card p { font-size: 0.95rem; color: #4b5563; line-height: 1.6; }
.assignee-status { font-weight: 600; color: #ea580c; }
.priority-card { display: flex; align-items: center; justify-content: center; text-align: center; background-color: #f9fafb; border-left: 5px solid; }
.priority-card h3 { margin: 0; }
.priority-card p { font-size: 1.2rem; font-weight: 700; }
.icon-prio { font-size: 2.5rem; line-height: 1; margin-right: 15px; }
.priority-card.prio-3 { border-left-color: #dc2626; }
.priority-card.prio-2 { border-left-color: #f59e0b; }
.priority-card.prio-1 { border-left-color: #10b981; }
.details-section, .attachment-section { background: white; padding: 2rem; border-radius: 12px; box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05); border: 1px solid #e5e7eb; margin-bottom: 2rem; }
.details-section h2, .attachment-section h2 { font-size: 1.2rem; color: #1f2937; margin-bottom: 1rem; padding-bottom: 0.5rem; border-bottom: 1px dashed #f3f4f6; }
.description { white-space: pre-wrap; line-height: 1.8; }
.report-dates { margin-top: 1.5rem; padding-top: 1rem; border-top: 1px dashed #f3f4f6; font-size: 0.9rem; color: #4b5563; }
.report-dates strong { font-weight: 700; color: #1f2937; }
.no-attachment { color: #9ca3af; font-style: italic; }
.btn-download { background: #2563eb; color: white; padding: 10px 20px; border: none; border-radius: 8px; cursor: pointer; font-weight: 600; transition: background 0.2s; }
.btn-download:hover { background: #1d4ed8; }
.info-msg { text-align: center; padding: 5rem 0; color: #6b7280; }
.spinner { width: 40px; height: 40px; border: 4px solid #f3f4f6; border-top: 4px solid #2563eb; border-radius: 50%; margin: 0 auto 1rem; animation: spin 1s linear infinite; }
@keyframes spin { 100% { transform: rotate(360deg); } }
@media (max-width: 768px) { .report-header { flex-direction: column; align-items: flex-start; gap: 15px; } .user-priority-section { grid-template-columns: 1fr; } .action-buttons { width: 100%; justify-content: space-between; } }
</style>