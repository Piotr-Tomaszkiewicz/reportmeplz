<template>
  <div class="page-container">
    <header class="page-header">
      <div class="header-content">
        <h1>Wszystkie zgłoszenia</h1>
        <p>Przeglądaj zgłoszenia zarejestrowane w systemie.</p>
      </div>
      
      <!-- Wyszukiwarka -->
      <div class="search-box">
        <span class="search-icon">🔍</span>
        <input 
          v-model="searchQuery" 
          type="text" 
          placeholder="Szukaj po tytule, zgłaszającym lub serwisancie..." 
        />
      </div>
    </header>

    <!-- KOMUNIKAT O BRAKU PEŁNYCH UPRAWNIEŃ (TYLKO DLA ROLI 'user') -->
    <div v-if="isBasicUser" class="access-warning">
      <p>⚠️ Pamiętaj: Jako **standardowy użytkownik**, widzisz tylko zgłoszenia, które sam utworzyłeś lub do których zostałeś przypisany. Pełny wgląd mają Administratorzy i Managerowie.</p>
    </div>

    <!-- Stan ładowania -->
    <div v-if="loading" class="info-msg">
      <div class="spinner"></div>
      <p>Pobieranie danych...</p>
    </div>
    
    <!-- Brak wyników -->
    <div v-else-if="filteredReports.length === 0" class="info-msg">
      <p>Brak zgłoszeń spełniających kryteria.</p>
    </div>

    <!-- Lista zgłoszeń -->
    <div v-else class="reports-grid">
      <div v-for="report in filteredReports" :key="report.id" class="report-card">
        
        <!-- Nagłówek Karty -->
        <div class="card-header">
          <span class="status-badge" :class="getStatusClass(report.status)">
            {{ report.status || 'Nowe' }}
          </span>
          <span class="report-id">#{{ report.id }}</span>
        </div>

        <!-- Treść -->
        <h3 class="report-title">{{ report.title }}</h3>
        <p class="report-desc">{{ truncateText(report.description, 100) }}</p>
        
        <!-- Sekcja Ludzie -->
        <div class="people-section">
          <!-- Zgłaszający -->
          <div class="person-info" title="Zgłaszający">
            <span class="icon">👤</span>
            <span class="person-name" :class="{ 'is-me': isReporterMe(report) }">
              {{ getReporterDisplayName(report) }}
            </span>
          </div>

          <!-- Serwisant -->
          <div v-if="report.assigneeNick" class="person-info assignee" title="Przypisany serwisant">
            <span class="icon">🛠</span>
            <span class="person-name">
              {{ getAssigneeDisplay(report) }}
            </span>
          </div>
        </div>

        <!-- Stopka: Priorytet -->
        <div class="report-footer">
          <div class="priority-info">
            <span class="icon-prio" :class="'prio-dot-' + report.priority">
              {{ getPriorityIcon(report.priority) }}
            </span>
            <span class="prio-text">{{ getPriorityName(report.priority) }}</span>
          </div>
        </div>

        <!-- Przycisk Pobierania (używa fileName) -->
        <div v-if="report.fileName" class="attachment-section">
          <button @click="downloadAttachment(report.id, report.fileName)" class="download-btn">
            📎 Pobierz załącznik ({{ report.fileName }})
          </button>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { reportService } from '../services/reportService'

const reports = ref([])
const loading = ref(true)
const searchQuery = ref('')

const currentUserLogin = localStorage.getItem('userLogin') || ''
const currentUserRole = localStorage.getItem('role') || 'user'

const isBasicUser = computed(() => {
    return currentUserRole.toLowerCase() === 'user';
});


onMounted(async () => {
  try {
    const res = await reportService.getAllReports()
    reports.value = res.data
  } catch (err) {
    console.error('Błąd pobierania zgłoszeń:', err)
  } finally {
    loading.value = false
  }
})

// === LOGIKA POBIERANIA ===

const downloadAttachment = async (id, fileName) => {
  try {
    document.body.style.cursor = 'wait'
    await reportService.downloadFile(id, fileName)
  } catch (err) {
    alert(`Błąd: ${err.message}.`)
  } finally {
    document.body.style.cursor = 'default'
  }
}

// === LOGIKA WYŚWIETLANIA UŻYTKOWNIKÓW ===

const isReporterMe = (report) => {
  const nick = report.reporterNick || report.ReporterNick || ''
  if (!currentUserLogin) return false
  return nick.toLowerCase() === currentUserLogin.toLowerCase()
}

const getReporterDisplayName = (report) => {
  const nick = report.reporterNick || report.ReporterNick || 'Anonim'
  
  if (currentUserLogin && nick.toLowerCase() === currentUserLogin.toLowerCase()) {
    return `Ty (${nick})`
  }
  return nick
}

const getAssigneeDisplay = (report) => {
  const nick = report.assigneeNick || report.AssigneeNick
  if (!nick || nick === 'Nieprzypisany') return 'Oczekiwanie na przydział'
  
  if (currentUserLogin && nick.toLowerCase() === currentUserLogin.toLowerCase()) {
    return `Ty (Przypisany)`
  }
  return nick
}

// === POMOCNICZE FUNKCJE ===

const filteredReports = computed(() => {
  const query = searchQuery.value.toLowerCase()
  return reports.value.filter(report => {
    const titleMatch = report.title.toLowerCase().includes(query)
    const rNick = report.reporterNick || report.ReporterNick || ''
    const reporterMatch = rNick.toLowerCase().includes(query)
    const aNick = report.assigneeNick || report.AssigneeNick || ''
    const assigneeMatch = aNick.toLowerCase().includes(query)
    
    return titleMatch || reporterMatch || assigneeMatch
  })
})

const getPriorityIcon = (p) => {
    switch(p) {
        case 3: return '🔥'; // Krytyczny
        case 2: return '⚠️'; // Wysoki
        case 1: return '🟢'; // Zwykły
        default: return '⚪';
    }
}

const getPriorityName = (p) => {
  switch(p) {
    case 1: return 'Zwykły'
    case 2: return 'Wysoki'
    case 3: return 'Krytyczny'
    default: return 'Nieznany'
  }
}

const getStatusClass = (status) => {
  if (!status) return 'default'
  const s = status.toLowerCase()
  if (s.includes('zarejestrowany')) return 'zarejestrowany'
  if (s.includes('realizacji') || s.includes('toku')) return 'w_realizacji'
  if (s.includes('zamknięty') || s.includes('zakończony')) return 'zamkniety'
  return 'default'
}

const truncateText = (text, limit) => {
  if (!text) return ''
  if (text.length <= limit) return text
  return text.substring(0, limit) + '...'
}
</script>

<style scoped>
/* Style dla AllReportsView */
.page-container { max-width: 1100px; margin: 0 auto; }
.page-header { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 2.5rem; gap: 2rem; }
.page-header h1 { font-size: 2rem; color: #111827; margin-bottom: 0.5rem; }
.page-header p { color: #6b7280; }
.search-box { position: relative; width: 300px; }
.search-icon { position: absolute; left: 12px; top: 50%; transform: translateY(-50%); color: #9ca3af; }
.search-box input { width: 100%; padding: 0.75rem 0.75rem 0.75rem 2.5rem; border: 1px solid #e5e7eb; border-radius: 0.75rem; outline: none; transition: 0.2s; }
.access-warning { margin-bottom: 20px; padding: 15px 20px; background-color: #fef3c7; border: 1px solid #fde68a; border-radius: 8px; color: #92400e; font-size: 0.9rem; font-weight: 500; text-align: center; }
.reports-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 1.5rem; }
.report-card { background: white; border: 1px solid #e5e7eb; border-radius: 1rem; padding: 1.5rem; transition: 0.2s; display: flex; flex-direction: column; }
.report-card:hover { transform: translateY(-4px); box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1); }
.card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; }
.status-badge { padding: 4px 12px; border-radius: 20px; font-size: 0.75rem; font-weight: 700; text-transform: uppercase; }
.status-badge.zarejestrowany { background: #eff6ff; color: #2563eb; }
.status-badge.w_realizacji { background: #fff7ed; color: #ea580c; }
.status-badge.zamkniety { background: #ecfdf5; color: #059669; }
.status-badge.default { background: #f3f4f6; color: #4b5563; }
.report-id { font-size: 0.85rem; color: #9ca3af; font-weight: 500; }
.report-title { font-size: 1.15rem; color: #1f2937; margin-bottom: 0.75rem; font-weight: 700; line-height: 1.4; }
.report-desc { color: #6b7280; font-size: 0.9rem; margin-bottom: 1.5rem; flex-grow: 1; }
.people-section { display: flex; flex-direction: column; gap: 8px; background: #f9fafb; padding: 10px; border-radius: 8px; margin-bottom: 1rem; }
.person-info { display: flex; align-items: center; gap: 8px; font-size: 0.85rem; color: #4b5563; }
.person-info.assignee { color: #ea580c; font-weight: 500; }
.person-name { font-weight: 600; }
.person-name.is-me { color: #2563eb; font-weight: 800; }
.report-footer { display: flex; justify-content: space-between; align-items: center; padding-top: 0.5rem; border-top: 1px solid #f3f4f6; }
.priority-info { display: flex; align-items: center; gap: 8px; margin-top: 5px; }
.icon-prio { font-size: 1.2em; line-height: 1; margin-bottom: -2px; }
.prio-text { font-size: 0.85rem; font-weight: 600; color: #4b5563; }
.attachment-section { margin-top: 12px; border-top: 1px dashed #e5e7eb; padding-top: 8px; }
.download-btn { background: none; border: none; color: #2563eb; font-weight: 600; cursor: pointer; padding: 0; font-size: 0.85rem; display: inline-flex; align-items: center; gap: 5px; }
.download-btn:hover { text-decoration: underline; color: #1d4ed8; }
.info-msg { text-align: center; padding: 5rem 0; color: #6b7280; }
.spinner { width: 40px; height: 40px; border: 4px solid #f3f4f6; border-top: 4px solid #2563eb; border-radius: 50%; margin: 0 auto 1rem; animation: spin 1s linear infinite; }
@keyframes spin { 100% { transform: rotate(360deg); } }
@media (max-width: 640px) { .page-header { flex-direction: column; align-items: flex-start; } .search-box { width: 100%; } }
</style>