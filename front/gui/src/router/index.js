import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'my-reports',
      // Lazy loading (opcjonalne, dla lepszej wydajności)
      component: () => import('../views/MyReportsView.vue') 
    },
    // Dodaj resztę ścieżek tutaj...
  ]
})

export default router