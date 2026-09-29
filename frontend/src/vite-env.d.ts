/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Base da API. Vazio em dev = http://localhost:5000 (orcazap.local.host). */
  readonly VITE_API_URL?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
