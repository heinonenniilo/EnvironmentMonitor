/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_SHOW_HANGFIRE_LINK: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
