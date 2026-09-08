import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    // Forward /api/* to the local Azure Functions host (func start on 7071).
    // Functions already serve under /api, so no path rewrite is needed.
    proxy: {
      '/api': { target: 'http://localhost:7071', changeOrigin: true },
    },
  },
});
