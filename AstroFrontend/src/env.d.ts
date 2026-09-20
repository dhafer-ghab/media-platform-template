/// <reference path="../.astro/types.d.ts" />
/// <reference types="astro/client" />

interface ImportMetaEnv {
  readonly POSTS_API_URL?: string;
  readonly PUBLIC_API_BASE_URL: string;
  readonly PUBLIC_SITE_URL?: string;
  readonly PUBLIC_IMAGE_BASE_URL?: string;
  readonly PUBLIC_VIDEO_BASE_URL?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

