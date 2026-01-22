<template>
  <div class="page-container">
    <header class="page-header">
      <div class="header-content">
        <h1>Moje zgłoszenia</h1>
        <p>Lista problemów zgłoszonych przez Ciebie lub przypisanych do Ciebie.</p>
      </div>
    </header>

    <!-- Stan ładowania -->
    <div v-if="loading" class="info-msg">
      <div class="spinner"></div>
      <p>Pobieranie Twoich zgłoszeń...</p>
    </div>
    
    <!-- Brak wyników -->
    <div v-else-if="reports.length === 0" class="info-msg">
      <p>Nie znaleziono żadnych zgłoszeń powiązanych z Twoim kontem.</p>
    </div>

    <!-- Lista zgłoszeń -->
    <div v-else class="reports-grid">
      <div v-for="report in reports" :key="report.id" class="report-card">
        
        <!-- Nagłówek Karty: Status i ID -->
        <div class="card-header">
          <span class="status-badge" :class="getStatusClass(report.status)">
            {{ report.status }}
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
          <div class="person-info assignee" title="Przypisany serwisant">
            <span class="icon">🛠</span>
            <span class="person-name">
              {{ getAssigneeDisplay(report) }}
            </span>
          </div>
        </div>

        <!-- Stopka: Priorytet -->
        <div class="report-footer">
          <div class="priority-info">
            <span class="dot" :class="'prio-dot-' + report.priority"></span>
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
import { ref, onMounted } from 'vue'
import { reportService } from '../services/reportService'

const reports = ref([])
const loading = ref(true)
const currentUserLogin = localStorage.getItem('userLogin') || ''

onMounted(async () => {
  try {
    const res = await reportService.getMyReports()
    reports.value = res.data
  } catch (err) {
    console.error('Błąd pobierania Twoich zgłoszeń:', err)
  } finally {
    loading.value = false
  }
})

// === LOGIKA POBIERANIA ===

// Przekazujemy ID zgłoszenia i OczekiwanaNazwaPliku do serwisu
const downloadAttachment = async (id, fileName) => {
  try {
    document.body.style.cursor = 'wait'
    await reportService.downloadFile(id, fileName) // Użycie nowej sygnatury
  } catch (err) {
    alert(`Błąd: ${err.message}.`)
  } finally {
    document.body.style.cursor = 'default'
  }
}

// === LOGIKA WYŚWIETLANIA ===

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
  if (!nick || nick === 'Nieprzypisany') {
    return 'Oczekiwanie na przydział'
  }
  if (currentUserLogin && nick.toLowerCase() === currentUserLogin.toLowerCase()) {
    return `Ty (Przypisany)`
  }
  return nick
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
/* Style (identyczne jak wcześniej) */
.page-container { max-width: 1100px; margin: 0 auto; }
.page-header { margin-bottom: 2.5rem; }
.page-header h1 { font-size: 2rem; color: #111827; margin-bottom: 0.5rem; }
.page-header p { color: #6b7280; }
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
.dot { width: 10px; height: 10px; border-radius: 50%; }
.prio-dot-1 { background: #10b981; }
.prio-dot-2 { background: #f59e0b; }
.prio-dot-3 { background: #ef4444; }
.prio-text { font-size: 0.85rem; font-weight: 600; color: #4b5563; }
.attachment-section { margin-top: 12px; border-top: 1px dashed #e5e7eb; padding-top: 8px; }
.download-btn { background: none; border: none; color: #2563eb; font-weight: 600; cursor: pointer; padding: 0; font-size: 0.85rem; display: inline-flex; align-items: center; gap: 5px; }
.download-btn:hover { text-decoration: underline; color: #1d4ed8; }
.info-msg { text-align: center; padding: 5rem 0; color: #6b7280; }
.spinner { width: 40px; height: 40px; border: 4px solid #f3f4f6; border-top: 4px solid #2563eb; border-radius: 50%; margin: 0 auto 1rem; animation: spin 1s linear infinite; }
@keyframes spin { 100% { transform: rotate(360deg); } }
</style>