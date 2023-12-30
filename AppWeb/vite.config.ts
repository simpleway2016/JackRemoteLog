import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import removeConsole from 'vite-plugin-remove-console'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    ...process.env.NODE_ENV == "production"?[removeConsole()]:[],
    vue()],
  server: {
    host: "0.0.0.0" 
  },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  define:{
    'process.env':{
      //development production test
      'ServerUrl': process.env.NODE_ENV == "development" ? "http://127.0.0.1:9000/":""//127.0.0.1:10001 18.183.223.73
    }
  }
})
