import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView
    },
    {
      path: '/moje-zgloszenia',
      name: 'my-reports',
      component: () => import('../views/MyReportsView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/wszystkie-zgloszenia',
      name: 'all-reports',
      component: () => import('../views/AllReportsView.vue'),
      meta: { requiresAuth: true, requiredRoles: ['admin', 'manager'] }
    },
    
    // === PANEL ADMINA ===
    {
      path: '/admin',
      name: 'admin-panel',
      component: () => import('../views/AdminPanelView.vue'),
      meta: { requiresAuth: true, requiredRoles: ['admin'] },
      children: [
        {
          path: 'locations',
          name: 'admin-locations',
          component: () => import('../views/LocationsView.vue'), // Tutaj poprawiona ścieżka
          meta: { requiredRoles: ['admin'] }
        },
        {
          path: 'users',
          name: 'admin-users',
          component: () => import('../views/UsersView.vue'), // Tutaj poprawiona ścieżka
          meta: { requiredRoles: ['admin'] }
        },
        {
          path: '',
          redirect: { name: 'admin-locations' }
        }
      ]
    },
    
    {
      path: '/zgloszenie/:id', 
      name: 'report-details',
      component: () => import('../views/ReportDetailsView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/'
    }
  ]
})

// NAVIGATION GUARD
router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('token')
  const userRole = (localStorage.getItem('role') || 'guest').toLowerCase()

  if (to.meta.requiresAuth && !token) {
    alert('Musisz być zalogowany, aby uzyskać dostęp.')
    next('/')
    return
  }

  if (to.meta.requiredRoles) {
    const isAuthorized = to.meta.requiredRoles.includes(userRole)
    if (!isAuthorized) {
      alert('Brak uprawnień. Nie masz dostępu do tej sekcji.')
      next('/')
      return
    }
  }

  next()
})

export default router