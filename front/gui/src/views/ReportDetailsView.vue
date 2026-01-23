<template>
  <div class="report-details-container">
    <!-- Loader -->
    <div v-if="loading" class="info-msg">
      <div class="spinner"></div>
      <p>Ładowanie szczegółów zgłoszenia...</p>
    </div>

    <!-- Błąd 404 -->
    <div v-else-if="!report" class="info-msg error">
      <p>Nie znaleziono zgłoszenia o ID: {{ $route.params.id }}</p>
      <button @click="router.back()" class="btn-back">← Powrót</button>
    </div>

    <div v-else>
      <!-- NAGŁÓWEK I PRZYCISKI AKCJI -->
      <header class="report-header">
        <div class="header-left">
          <button @click="router.back()" class="btn-back-minimal">← Powrót</button>
          <div class="title-wrapper">
            <span class="report-id">#{{ report.id }}</span>
            <h1>{{ report.title }}</h1>
            <span class="status-tag" :class="getStatusClass(report.status)">
              {{ report.status }}
            </span>
          </div>
        </div>

        <div class="action-buttons">
          <!-- ROZWIĄŻ (Dla przypisanego workera, admina lub managera) -->
          <button 
            v-if="canResolve" 
            @click="handleResolve" 
            class="btn-action btn-resolve" 
            :disabled="isResolving"
          >
            {{ isResolving ? 'Zamykanie...' : '✅ Rozwiąż' }}
          </button>
          
          <!-- PRZYPISZ (Dla Admina/Managera) -->
          <button 
            v-if="canAssign" 
            @click="showAssignModal = true" 
            class="btn-action btn-assign"
          >
            {{ (report.assigneeNick && report.assigneeNick !== 'Nieprzypisany') ? 'Zmień serwisanta' : 'Przypisz serwisanta' }}
          </button>

          <!-- ANULUJ (Dla zgłaszającego) -->
          <button 
            v-if="canCancelReport" 
            @click="handleCancel" 
            class="btn-action btn-cancel" 
            :disabled="isCancelling"
          >
            {{ isCancelling ? 'Anulowanie...' : 'Anuluj' }}
          </button>
        </div>
      </header>

      <!-- SEKCJA KART INFORMACYJNYCH -->
      <div class="info-cards-grid">
        <!-- Zgłaszający -->
        <div class="info-card user-card">
          <div class="card-icon">👤</div>
          <div class="card-content">
            <h3>Zgłaszający</h3>
            <p class="main-info">{{ report.reporterNick }}</p>
            <div class="sub-info" v-if="!reporterDetailsLoading">
              <p>📧 {{ reporterDetails.email }}</p>
              <p>📍 {{ reporterDetails.location }}</p>
            </div>
            <p v-else class="loading-sub">Ładowanie detali...</p>
          </div>
        </div>

        <!-- Serwisant -->
        <div class="info-card assignee-card" :class="{ 'unassigned': !report.assigneeNick || report.assigneeNick === 'Nieprzypisany' }">
          <div class="card-icon">🛠</div>
          <div class="card-content">
            <h3>Serwisant</h3>
            <p class="main-info">{{ report.assigneeNick || 'Brak przypisania' }}</p>
            <p class="sub-info">{{ (report.assigneeNick && report.assigneeNick !== 'Nieprzypisany') ? 'Osoba odpowiedzialna' : 'Oczekiwanie na wsparcie' }}</p>
          </div>
        </div>

        <!-- Priorytet -->
        <div class="info-card priority-card" :class="'prio-border-' + report.priority">
          <div class="card-icon">{{ getPriorityIcon(report.priority) }}</div>
          <div class="card-content">
            <h3>Priorytet</h3>
            <p class="main-info">{{ getPriorityName(report.priority) }}</p>
            <p class="sub-info">Ważność zgłoszenia</p>
          </div>
        </div>
      </div>

      <!-- TREŚĆ I ZAŁĄCZNIK -->
      <div class="main-details-layout">
        <section class="description-section">
          <h2>Opis problemu</h2>
          <div class="content-box">
            <p class="description-text">{{ report.description || 'Brak szczegółowego opisu.' }}</p>
          </div>
          <div class="meta-dates">
            <span>📅 Zgłoszono: {{ formatDateTime(report.dateReported) }}</span>
            <span v-if="report.dateResolved">✅ Rozwiązano: {{ formatDateTime(report.dateResolved) }}</span>
          </div>
        </section>

        <section v-if="report.fileName" class="file-section">
          <h2>Załącznik do zgłoszenia</h2>
          <div class="file-box">
            <span class="file-icon">📄</span>
            <div class="file-info">
              <p class="file-name">{{ report.fileName }}</p>
              <button @click="downloadAttachment" class="btn-download-link">Pobierz plik</button>
            </div>
          </div>
        </section>
      </div>

      <!-- SEKCJA KOMENTARZY -->
      <section class="comments-section">
        <div class="comments-header">
          <h2>Komunikacja i historia ({{ comments.length }})</h2>
        </div>

        <div class="comments-container">
          <div class="comments-list">
            <div 
              v-for="comment in comments" 
              :key="comment.id" 
              class="comment-wrapper"
              :class="{ 'is-me': isCommentMe(comment) }"
            >
              <div class="comment-bubble">
                <div class="comment-top">
                  <span class="author">{{ isCommentMe(comment) ? 'Ty' : comment.userLogin }}</span>
                  <span class="date">{{ formatDateTime(comment.dateAdded) }}</span>
                </div>
                <div class="comment-body">
                  {{ comment.content }}
                </div>
                <!-- Załącznik w komentarzu -->
                <div v-if="comment.fileName" class="comment-attachment">
                  <button @click="downloadCommentFile(comment.id, comment.fileName)" class="btn-comment-download">
                    📎 {{ comment.fileName }}
                  </button>
                </div>
              </div>
            </div>

            <div v-if="comments.length === 0" class="empty-comments">
              Brak komentarzy. Możesz zadać pytanie lub dodać informację poniżej.
            </div>
          </div>

          <!-- Formularz dodawania komentarza -->
          <div class="comment-form-box">
            <textarea 
              v-model="newComment.content" 
              placeholder="Wpisz treść wiadomości..." 
              rows="3"
            ></textarea>
            
            <div class="form-bottom">
              <div class="file-upload-mini">
                <input type="file" id="comm-file" @change="handleCommentFile" hidden />
                <label for="comm-file" class="file-label-mini">
                  {{ newComment.file ? '✅ ' + newComment.file.name : '📎 Dodaj załącznik' }}
                </label>
              </div>
              <button 
                @click="sendComment" 
                class="btn-send" 
                :disabled="submittingComment || !newComment.content.trim()"
              >
                {{ submittingComment ? 'Wysyłanie...' : 'Wyślij' }}
              </button>
            </div>
          </div>
        </div>
      </section>
    </div>

    <!-- Modal Przypisywania -->
    <AssigneeModal
      v-if="showAssignModal"
      :report-id="report?.id"
      :current-assignee-login="report?.assigneeNick"
      @close="showAssignModal = false"
      @assigned="refreshReportData"
    />
  </div>
</template>

<script setup>
import { ref, onMounted, computed, reactive, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { reportService } from '../services/reportService'
import { commentService } from '../services/commentService'
import { userService } from '../services/userService'
import { useAuthStore } from '../stores/authStore'
import AssigneeModal from '../components/AssigneeModal.vue'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

// State
const report = ref(null)
const comments = ref([])
const loading = ref(true)
const isCancelling = ref(false)
const isResolving = ref(false)
const submittingComment = ref(false)
const reporterDetailsLoading = ref(true)
const reporterDetails = ref({ email: 'Brak', location: 'Brak' })
const showAssignModal = ref(false)

const newComment = reactive({ content: '', file: null })

// Logged User Data
const currentUserLogin = localStorage.getItem('userLogin') || ''
const currentUserRole = authStore.role.toLowerCase()

// --- CYKL ŻYCIA ---
onMounted(async () => {
  await loadReportData()
})

// --- POBIERANIE DANYCH ---
const loadReportData = async () => {
  const reportId = route.params.id
  try {
    if (authStore.isLoggedIn && !authStore.userProfile) await authStore.fetchUserProfile()

    const [reportRes, commentsRes] = await Promise.all([
      reportService.getReportById(reportId),
      commentService.getCommentsByReportId(reportId)
    ])

    report.value = {
      ...reportRes.data,
      reporterNick: reportRes.data.reporterNick || reportRes.data.ReporterNick,
      assigneeNick: reportRes.data.assigneeNick || reportRes.data.AssigneeNick,
      status: reportRes.data.status || reportRes.data.Status,
      fileName: reportRes.data.fileName || reportRes.data.FileName
    }
    comments.value = commentsRes.data

    if (report.value.reporterId) await fetchReporterDetails(report.value.reporterId)
  } catch (err) {
    console.error('Błąd pobierania danych:', err)
  } finally {
    loading.value = false
  }
}

async function fetchReporterDetails(id) {
  reporterDetailsLoading.value = true
  try {
    if (isReporterMe.value && authStore.userProfile) {
      reporterDetails.value = { email: authStore.userProfile.email, location: authStore.userProfile.location }
      return
    }
    if (['admin', 'manager', 'worker'].includes(currentUserRole)) {
      const res = await userService.getUserById(id)
      reporterDetails.value = { 
        email: res.data.email || res.data.Email || 'Brak danych', 
        location: res.data.locationShortName || res.data.LocationShortName || 'Brak danych' 
      }
    }
  } catch (err) {
    console.error("Błąd pobierania danych użytkownika")
  } finally {
    reporterDetailsLoading.value = false
  }
}

// --- LOGIKA KOMENTARZY ---
const isCommentMe = (c) => c.userLogin?.toLowerCase() === currentUserLogin.toLowerCase()
const handleCommentFile = (e) => { newComment.file = e.target.files[0] }

const sendComment = async () => {
  try {
    submittingComment.value = true
    await commentService.addComment({
      content: newComment.content,
      reportId: report.value.id,
      file: newComment.file
    })
    newComment.content = ''
    newComment.file = null
    // Odśwież tylko listę komentarzy
    const res = await commentService.getCommentsByReportId(report.value.id)
    comments.value = res.data
  } catch (err) {
    alert("Nie udało się dodać komentarza.")
  } finally {
    submittingComment.value = false
  }
}

const downloadCommentFile = (id, name) => commentService.downloadCommentFile(id, name)

// --- UPRAWNIENIA I AKCJE ---
const isAdminOrManager = computed(() => ['admin', 'manager'].includes(currentUserRole))
const isReporterMe = computed(() => (report.value?.reporterNick || '').toLowerCase() === currentUserLogin.toLowerCase())
const isAssignedToMe = computed(() => (report.value?.assigneeNick || '').toLowerCase() === currentUserLogin.toLowerCase())
const isActiveStatus = computed(() => {
  const s = (report.value?.status || '').toLowerCase()
  return s === 'zarejestrowany' || s.includes('realizacji') || s.includes('toku')
})

const canAssign = computed(() => isAdminOrManager.value && isActiveStatus.value)
const canResolve = computed(() => isActiveStatus.value && (isAdminOrManager.value || (currentUserRole === 'worker' && isAssignedToMe.value)))
const canCancelReport = computed(() => isReporterMe.value && isActiveStatus.value)

const handleResolve = async () => {
  if (!confirm("Czy na pewno rozwiązałeś ten problem?")) return
  isResolving.value = true
  try {
    await reportService.resolveReport(report.value.id)
    await refreshReportData()
  } catch (err) {
    alert("Błąd podczas zmiany statusu.")
  } finally { isResolving.value = false }
}

const handleCancel = async () => {
  if (!confirm("Czy na pewno chcesz anulować zgłoszenie?")) return
  isCancelling.value = true
  try {
    await reportService.cancelReport(report.value.id)
    await refreshReportData()
  } catch (err) {
    alert("Błąd podczas anulowania.")
  } finally { isCancelling.value = false }
}

const refreshReportData = async () => {
  loading.value = true
  await nextTick()
  await loadReportData()
}

const downloadAttachment = () => reportService.downloadFile(report.value.id, report.value.fileName)

// --- HELPERY ---
const getPriorityIcon = (p) => p === 3 ? '🔥' : (p === 2 ? '⚠️' : '🟢')
const getPriorityName = (p) => p === 3 ? 'Krytyczny' : (p === 2 ? 'Wysoki' : 'Zwykły')
const getStatusClass = (s) => {
  s = (s || '').toLowerCase()
  if (s.includes('zarejestrowany')) return 'zarejestrowany'
  if (s.includes('realizacji') || s.includes('toku')) return 'w_realizacji'
  if (s.includes('rozwiązane') || s.includes('zamknięty')) return 'zamkniety'
  if (s.includes('anulowane')) return 'anulowane'
  return 'default'
}
const formatDateTime = (d) => d ? new Date(d).toLocaleDateString('pl-PL', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }) : '---'
</script>

<style scoped>
.report-details-container { max-width: 1000px; margin: 0 auto; padding-bottom: 100px; }

/* NAGŁÓWEK */
.report-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2.5rem; background: white; padding: 2rem; border-radius: 1rem; border: 1px solid #e5e7eb; box-shadow: 0 1px 3px rgba(0,0,0,0.05); }
.btn-back-minimal { background: none; border: none; color: #6b7280; cursor: pointer; font-size: 0.9rem; margin-bottom: 0.5rem; display: block; padding: 0; }
.title-wrapper { display: flex; align-items: center; gap: 1rem; flex-wrap: wrap; }
.report-id { color: #9ca3af; font-weight: 800; font-size: 1.5rem; }
.report-header h1 { font-size: 1.8rem; color: #111827; margin: 0; }
.status-tag { padding: 6px 14px; border-radius: 20px; font-size: 0.8rem; font-weight: 800; text-transform: uppercase; }
.status-tag.zarejestrowany { background: #eff6ff; color: #2563eb; }
.status-tag.w_realizacji { background: #fff7ed; color: #ea580c; }
.status-tag.zamkniety { background: #ecfdf5; color: #059669; }
.status-tag.anulowane { background: #fef2f2; color: #dc2626; }

.action-buttons { display: flex; gap: 0.75rem; }
.btn-action { padding: 12px 20px; border-radius: 8px; font-weight: 700; cursor: pointer; border: none; transition: 0.2s; }
.btn-resolve { background: #059669; color: white; }
.btn-assign { background: #3b82f6; color: white; }
.btn-cancel { background: #fef2f2; color: #dc2626; }
.btn-action:hover { opacity: 0.9; transform: translateY(-1px); }

/* KARTY INFORMACYJNE */
.info-cards-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 1.5rem; margin-bottom: 2rem; }
.info-card { background: white; padding: 1.5rem; border-radius: 1rem; border: 1px solid #e5e7eb; display: flex; align-items: center; gap: 1.25rem; }
.card-icon { font-size: 2rem; }
.card-content h3 { font-size: 0.8rem; color: #9ca3af; text-transform: uppercase; margin-bottom: 0.25rem; }
.main-info { font-size: 1.1rem; font-weight: 700; color: #1f2937; }
.sub-info { font-size: 0.85rem; color: #6b7280; margin-top: 4px; }
.assignee-card.unassigned { border: 1px dashed #d1d5db; background: #f9fafb; }
.prio-border-3 { border-left: 6px solid #dc2626; }
.prio-border-2 { border-left: 6px solid #f59e0b; }
.prio-border-1 { border-left: 6px solid #10b981; }

/* OPIS I ZAŁĄCZNIK */
.main-details-layout { display: grid; grid-template-columns: 2fr 1fr; gap: 1.5rem; margin-bottom: 2.5rem; }
.description-section, .file-section { background: white; padding: 2rem; border-radius: 1rem; border: 1px solid #e5e7eb; }
.description-section h2, .file-section h2 { font-size: 1.1rem; margin-bottom: 1.5rem; color: #111827; }
.description-text { white-space: pre-wrap; line-height: 1.7; color: #374151; }
.meta-dates { margin-top: 2rem; font-size: 0.8rem; color: #9ca3af; display: flex; gap: 1.5rem; }
.file-box { display: flex; align-items: center; gap: 1rem; background: #f3f4f6; padding: 1rem; border-radius: 0.75rem; }
.file-icon { font-size: 1.5rem; }
.file-name { font-size: 0.9rem; font-weight: 600; color: #374151; margin-bottom: 4px; overflow: hidden; text-overflow: ellipsis; }
.btn-download-link { background: none; border: none; color: #2563eb; padding: 0; font-weight: 700; cursor: pointer; font-size: 0.85rem; }

/* KOMENTARZE */
.comments-section { background: white; padding: 2.5rem; border-radius: 1rem; border: 1px solid #e5e7eb; }
.comments-list { margin: 2rem 0; display: flex; flex-direction: column; gap: 1.5rem; }
.comment-wrapper { display: flex; flex-direction: column; }
.comment-wrapper.is-me { align-items: flex-end; }
.comment-bubble { max-width: 75%; background: #f3f4f6; padding: 1.25rem; border-radius: 1rem; border-bottom-left-radius: 0; }
.comment-wrapper.is-me .comment-bubble { background: #eff6ff; border-radius: 1rem; border-bottom-right-radius: 0; border: 1px solid #bfdbfe; }
.comment-top { display: flex; justify-content: space-between; margin-bottom: 0.5rem; font-size: 0.75rem; gap: 2rem; }
.author { font-weight: 800; color: #111827; }
.date { color: #9ca3af; }
.comment-body { line-height: 1.5; color: #374151; }
.btn-comment-download { margin-top: 10px; background: white; border: 1px solid #d1d5db; padding: 6px 12px; border-radius: 6px; font-size: 0.8rem; cursor: pointer; }

.comment-form-box { margin-top: 3rem; border-top: 2px solid #f3f4f6; padding-top: 2rem; }
.comment-form-box textarea { width: 100%; border: 1px solid #d1d5db; border-radius: 0.75rem; padding: 1rem; font-family: inherit; resize: none; margin-bottom: 1rem; }
.form-bottom { display: flex; justify-content: space-between; align-items: center; }
.file-label-mini { font-size: 0.85rem; color: #2563eb; cursor: pointer; font-weight: 700; }
.btn-send { background: #2563eb; color: white; border: none; padding: 12px 30px; border-radius: 8px; font-weight: 700; cursor: pointer; }
.btn-send:disabled { background: #93c5fd; cursor: not-allowed; }

.empty-comments { text-align: center; color: #9ca3af; padding: 2rem; border: 2px dashed #f3f4f6; border-radius: 1rem; }
.spinner { width: 40px; height: 40px; border: 4px solid #f3f4f6; border-top: 4px solid #2563eb; border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 1rem; }
@keyframes spin { 100% { transform: rotate(360deg); } }

@media (max-width: 900px) {
  .info-cards-grid, .main-details-layout { grid-template-columns: 1fr; }
  .report-header { flex-direction: column; gap: 1.5rem; }
  .action-buttons { width: 100%; display: grid; grid-template-columns: 1fr 1fr; }
}
</style>