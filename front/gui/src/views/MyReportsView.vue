<template>
  <div class="page-container">
    <header class="page-header">
      <div class="header-content">
        <h1>Moje zgłoszenia</h1>
        <p>Lista problemów zgłoszonych przez Ciebie lub przypisanych do Ciebie.</p>
      </div>
      
      <!-- WYSZUKIWARKA I SORTOWANIE -->
      <div class="controls-group">
        <div class="search-box">
          <span class="search-icon">🔍</span>
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="Szukaj po tytule, zgłaszającym lub serwisancie..." 
          />
        </div>
        
        <div class="sort-box">
          <label for="sort-select">Sortuj:</label>
          <select id="sort-select" v-model="sortCriteria">
            <option value="dateReported_desc">📅 Data zgłoszenia (Nowsze)</option>
            <option value="dateReported_asc">📅 Data zgłoszenia (Starsze)</option>
            <option value="dateResolved_desc">✅ Data rozwiązania</option>
            <option value="id_asc"># Numer zgłoszenia</option>
            <option value="title_asc">A-Z Tytuł</option>
            <option value="reporterNick_asc">👤 Zgłaszający</option>
            <option value="assigneeNick_asc">🛠 Serwisant</option>
            <option value="status_asc">Status</option>
          </select>
        </div>
      </div>
    </header>

    <!-- Stan ładowania -->
    <div v-if="loading" class="info-msg">
      <div class="spinner"></div>
      <p>Pobieranie Twoich zgłoszeń...</p>
    </div>
    
    <!-- Brak wyników -->
    <div v-else-if="filteredAndSortedReports.length === 0" class="info-msg">
      <p>Nie znaleziono żadnych zgłoszeń powiązanych z Twoim kontem.</p>
    </div>

    <!-- Lista zgłoszeń -->
    <div v-else class="reports-grid">
      <router-link 
        v-for="report in filteredAndSortedReports" 
        :key="report.id" 
        :to="{ name: 'report-details', params: { id: report.id } }"
        class="report-card-link"
      >
        <div class="report-card">
          
          <!-- Nagłówek Karty: Status i ID -->
          <div class="card-header">
            <span class="status-badge" :class="getStatusClass(report.status)">
              {{ report.status }}
            </span>
            <span class="report-id">#{{ report.id }}</span>
          </div>

          <!-- Treść -->
          <h3 class="report-title">{{ truncateText(report.title, 50) }}</h3>
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

          <!-- Stopka: Priorytet i Data -->
          <div class="report-footer">
            <div class="priority-info">
              <span class="icon-prio" :class="'prio-dot-' + report.priority">
                {{ getPriorityIcon(report.priority) }}
              </span>
              <span class="prio-text">{{ getPriorityName(report.priority) }}</span>
            </div>
            
            <span class="date-reported" title="Data zgłoszenia">
                {{ formatDateTime(report.dateReported) }}
            </span>
          </div>

          <!-- Przycisk Pobierania (tylko podgląd) -->
          <div v-if="report.fileName" class="attachment-info">
            📎 Zawiera załącznik
          </div>
        </div>
      </router-link>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { reportService } from '../services/reportService'

const reports = ref([])
const loading = ref(true)
const currentUserLogin = localStorage.getItem('userLogin') || ''

const searchQuery = ref('')
const sortCriteria = ref('dateReported_desc') // DOMYŚLNE SORTOWANIE

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

// === LOGIKA SORTOWANIA I FILTROWANIA ===

const filteredReports = computed(() => {
  const query = searchQuery.value.toLowerCase()
  if (!query) {
    return reports.value
  }
  
  return reports.value.filter(report => {
    const titleMatch = report.title.toLowerCase().includes(query)
    const rNick = report.reporterNick || report.ReporterNick || ''
    const reporterMatch = rNick.toLowerCase().includes(query)
    const aNick = report.assigneeNick || report.AssigneeNick || ''
    const assigneeMatch = aNick.toLowerCase().includes(query)
    
    return titleMatch || reporterMatch || assigneeMatch
  })
})

const filteredAndSortedReports = computed(() => {
    const [key, direction] = sortCriteria.value.split('_');
    const sorted = [...filteredReports.value]; // Kopiujemy, by nie mutować oryginalnej tablicy

    sorted.sort((a, b) => {
        let aVal = a[key];
        let bVal = b[key];

        // Konwersja dat
        if (key.startsWith('date')) {
            aVal = aVal ? new Date(aVal).getTime() : 0;
            bVal = bVal ? new Date(bVal).getTime() : 0;
        } 
        // Konwersja na stringi dla pól tekstowych
        else if (typeof aVal === 'string' && typeof bVal === 'string') {
             aVal = aVal.toLowerCase();
             bVal = bVal.toLowerCase();
        }

        if (aVal < bVal) return direction === 'asc' ? -1 : 1;
        if (aVal > bVal) return direction === 'asc' ? 1 : -1;
        return 0;
    });

    return sorted;
});


// === LOGIKA WYŚWIETLANIA I POMOCNICZE ===

const formatDateTime = (dateString) => {
  if (!dateString) return '---';
  try {
    const options = { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' };
    return new Date(dateString).toLocaleDateString('pl-PL', options);
  } catch {
    return 'Błąd daty';
  }
}

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

const getPriorityIcon = (p) => {
    switch(p) { case 3: return '🔥'; case 2: return '⚠️'; case 1: return '🟢'; default: return '⚪'; }
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
  if (s.includes('zamknięty') || s.includes('zakończony') || s.includes('rozwiązany')) return 'zamkniety' // Rozwiązany na zielono
  return 'default'
}

const truncateText = (text, limit) => {
  if (!text) return ''
  if (text.length <= limit) return text
  return text.substring(0, limit) + '...'
}
</script>

<style scoped>
/* Style dla Wyszukiwania i Sortowania */
.page-header {
    display: flex; justify-content: space-between; align-items: flex-end; 
    margin-bottom: 2.5rem; gap: 2rem;
}
.controls-group {
    display: flex;
    gap: 15px;
}
.header-content { flex-shrink: 0; }
.search-box { position: relative; width: 250px; }
.search-icon { position: absolute; left: 12px; top: 50%; transform: translateY(-50%); color: #9ca3af; }
.search-box input {
    width: 100%; padding: 0.75rem 0.75rem 0.75rem 2.5rem; border: 1px solid #e5e7eb; 
    border-radius: 0.75rem; font-size: 0.95rem; outline: none;
}
.sort-box label {
    font-size: 0.9rem;
    color: #4b5563;
    margin-right: 5px;
    font-weight: 600;
}
.sort-box select {
    padding: 0.75rem;
    border: 1px solid #e5e7eb;
    border-radius: 0.75rem;
    font-size: 0.95rem;
    cursor: pointer;
}

/* KARTA I GRID */
.page-container { max-width: 1100px; margin: 0 auto; }
.page-header h1 { font-size: 2rem; color: #111827; margin-bottom: 0.5rem; }
.page-header p { color: #6b7280; }
.reports-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 1.5rem; }
.report-card-link { text-decoration: none; color: inherit; } 
.report-card { 
  background: white; border: 1px solid #e5e7eb; border-radius: 1rem; 
  padding: 1.5rem; transition: all 0.2s; cursor: pointer; 
  display: flex; flex-direction: column; height: 100%;
}
.report-card:hover { transform: translateY(-4px); box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1); }

.card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; }
.status-badge { padding: 4px 12px; border-radius: 20px; font-size: 0.75rem; font-weight: 700; text-transform: uppercase; }
.status-badge.zarejestrowany { background: #eff6ff; color: #2563eb; }
.status-badge.w_realizacji { background: #fff7ed; color: #ea580c; }
.status-badge.zamkniety { background: #ecfdf5; color: #059669; } /* Rozwiązany/Zamknięty na zielono */
.status-badge.default { background: #f3f4f6; color: #4b5563; }
.report-id { font-size: 0.85rem; color: #9ca3af; font-weight: 500; }
.report-title { font-size: 1.15rem; color: #1f2937; margin-bottom: 0.75rem; font-weight: 700; line-height: 1.4; }
.report-desc { color: #6b7280; font-size: 0.9rem; margin-bottom: 1.5rem; flex-grow: 1; min-height: 40px; }
.people-section { display: flex; flex-direction: column; gap: 8px; background: #f9fafb; padding: 10px; border-radius: 8px; margin-bottom: 1rem; }
.person-info { display: flex; align-items: center; gap: 8px; font-size: 0.85rem; color: #4b5563; }
.person-info.assignee { color: #ea580c; font-weight: 500; }
.person-name { font-weight: 600; }
.person-name.is-me { color: #2563eb; font-weight: 800; }

/* STOPKA */
.report-footer { 
    display: flex; justify-content: space-between; align-items: center; 
    padding-top: 0.5rem; border-top: 1px solid #f3f4f6; 
    font-size: 0.85rem;
}
.date-reported {
    color: #9ca3af;
}
.priority-info { display: flex; align-items: center; gap: 8px; margin-top: 5px; }
.icon-prio { font-size: 1.2em; line-height: 1; margin-bottom: -2px; }
.attachment-info { margin-top: 10px; font-size: 0.8rem; color: #2563eb; font-weight: 600; }
.info-msg { text-align: center; padding: 5rem 0; color: #6b7280; }
.spinner { width: 40px; height: 40px; border: 4px solid #f3f4f6; border-top: 4px solid #2563eb; border-radius: 50%; margin: 0 auto 1rem; animation: spin 1s linear infinite; }
@keyframes spin { 100% { transform: rotate(360deg); } }

@media (max-width: 640px) {
  .page-header { flex-direction: column; align-items: flex-start; }
  .controls-group { flex-direction: column; width: 100%; }
  .search-box { width: 100%; }
  .sort-box { width: 100%; }
}
</style>