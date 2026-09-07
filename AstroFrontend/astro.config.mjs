import { defineConfig } from "astro/config";
import node from "@astrojs/node";
import { loadEnv } from "vite";

const { PUBLIC_SITE_URL } = loadEnv(
  process.env.NODE_ENV ?? "development",
  process.cwd(),
  "PUBLIC_",
);

export default defineConfig({
  site: PUBLIC_SITE_URL ?? "http://localhost:4321",
  output: "server",
  adapter: node({ mode: "standalone" }),
});
