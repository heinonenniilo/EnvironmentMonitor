import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import checker from "vite-plugin-checker";

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), checker({ typescript: true })],
  server: {
    proxy: {
      "/api": {
        target: "https://localhost:7135", // Your backend server
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
