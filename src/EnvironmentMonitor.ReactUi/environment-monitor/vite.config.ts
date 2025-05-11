import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import checker from "vite-plugin-checker";

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    checker({
      typescript: {
        tsconfigPath: "./tsconfig.app.json",
      },
    }),
  ],
  server: {
    proxy: {
      "/api": {
        target: "https://localhost:7135",
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
