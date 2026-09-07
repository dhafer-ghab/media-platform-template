import { defineConfig } from "astro/config";
import node from "@astrojs/node";
import { loadEnv } from "vite";

const isBuild = process.argv.includes('build');
const { PUBLIC_SITE_URL, PUBLIC_API_BASE_URL } = loadEnv(
  process.env.NODE_ENV ?? (isBuild ? 'production' : 'development'),
  process.cwd(),
  "PUBLIC_",
);

if (isBuild && !PUBLIC_SITE_URL) {
  throw new Error('PUBLIC_SITE_URL is required for production builds.');
}

if (!PUBLIC_API_BASE_URL) {
  throw new Error('PUBLIC_API_BASE_URL is required.');
}

for (const [name, value] of Object.entries({ PUBLIC_SITE_URL, PUBLIC_API_BASE_URL })) {
  if (!value) continue;
  const url = new URL(value);
  const isLocalHttp = url.protocol === 'http:' && ['localhost', '127.0.0.1', '[::1]'].includes(url.hostname);
  if (
    (url.protocol !== 'https:' && !isLocalHttp) ||
    url.pathname !== '/' ||
    url.search ||
    url.hash ||
    url.username ||
    url.password
  ) {
    throw new Error(`${name} must be an HTTPS root origin; HTTP is allowed only for local development.`);
  }
}

export default defineConfig({
  site: PUBLIC_SITE_URL,
  output: "server",
  adapter: node({ mode: "standalone" }),
});
