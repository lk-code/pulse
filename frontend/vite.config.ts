import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd())
  const apiTarget = env.VITE_API_BASE_URL || 'http://localhost:6145'

  return {
    base: '/',
    plugins: [vue(), tailwindcss()],
    server: {
      proxy: {
        '/api': {
          target: apiTarget,
          changeOrigin: true
        }
      }
    }
  }
})
