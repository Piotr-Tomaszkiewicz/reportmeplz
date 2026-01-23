// src/main.js

import { createApp } from 'vue'
import { createPinia } from 'pinia' // Importujemy Pinia
import App from './App.vue'
import router from './router'

// Usunięta linia z importem './assets/main.css' jeśli była

const app = createApp(App)

// Utworzenie instancji Pinia
const pinia = createPinia()

app.use(pinia) // Użycie Pinia
app.use(router)

app.mount('#app')