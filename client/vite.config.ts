import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import path from 'node:path';

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5000',
      '/auctionHub': { target: 'http://localhost:5000', ws: true },
      '/openapi': 'http://localhost:5000',
      '/scalar': 'http://localhost:5000',
      '/hangfire': 'http://localhost:5000',
    },
  },
});
