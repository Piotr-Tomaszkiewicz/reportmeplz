// Otwórz: src/router/index.js

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
      meta: { 
        requiresAuth: true,
        // Tylko te role mają dostęp
        requiredRoles: ['admin', 'manager'] 
      }
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/'
    }
  ]
})

// NAVIGATION GUARD - Strażnik dostępu
router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('token')
  const userRole = (localStorage.getItem('role') || 'guest').toLowerCase() // Domyślna rola to guest

  if (to.meta.requiresAuth && !token) {
    alert('Musisz być zalogowany, aby uzyskać dostęp.')
    next('/')
    return
  }

  // Sprawdzanie Ról
  if (to.meta.requiredRoles) {
    const isAuthorized = to.meta.requiredRoles.includes(userRole)
    
    // Jeśli rola użytkownika NIE jest dozwolona:
    if (!isAuthorized) {
      alert('Brak uprawnień. Tylko administratorzy i managerowie mają dostęp do wszystkich zgłoszeń.')
      next('/')
      return
    }
  }

  next()
})

export default router