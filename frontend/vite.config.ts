import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  server: {
    proxy: {
      // Forward API calls to the SearchApi backend so the browser sees a
      // same-origin request (no CORS). Targets the http profile (5189), which
      // both launch profiles serve, avoiding the self-signed https cert.
      '/api': {
        target: 'http://localhost:5189',
        changeOrigin: true,
      },
    },
  },
})
