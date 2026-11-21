import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// el cliente corre en el puerto 5173 (el mismo que permiten las APIs en CORS)
export default defineConfig({
  plugins: [react()],
  server: { port: 5173, strictPort: true }
})
