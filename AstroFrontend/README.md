# Verso frontend

Astro SSR frontend for the media platform. Catalog content is loaded at runtime
from the PostgreSQL-backed Catalog API; production pages do not contain local
catalog fixtures.

## Requirements

- Node.js 22.12 or newer
- The .NET API and PostgreSQL catalog
- `PUBLIC_API_BASE_URL`, the API root origin (required)
- `PUBLIC_SITE_URL`, the public frontend origin (required for builds)

Copy `.env.example` to `.env` and set values for the local environment. Optional
`PUBLIC_IMAGE_BASE_URL` and `PUBLIC_VIDEO_BASE_URL` values enable real asset
delivery; deterministic visual placeholders are shown when those services are
not configured.

## Commands

```sh
npm install
npm run dev
npm run check
npm run build
npm run audit
```

The production server output is written to `dist/`. Pages are server-rendered,
so the Catalog API must be reachable at request time. The build itself does not
fetch catalog content.

## Catalog routes

- `/` renders paged discovery and featured creators.
- `/explore?q=&tag=&page=` performs server-side Catalog discovery.
- `/actors` is the paged creator directory.
- `/tags` and `/tags/[tag]` use Catalog tags and discovery.
- `/actors/[slug]`, album, and GUID post routes use Catalog detail endpoints.
- `/artists`, `/works`, and `/categories` are legacy routes resolved by the
  Catalog API and returned as permanent redirects.
- `/sitemap.xml` enumerates actors and albums. Post URLs are omitted because the
  Catalog API has no bulk post endpoint; fetching posts per album would make
  sitemap generation scale as an N+1 request pattern.

API calls are centralized in `src/api/client.ts` and `src/api/catalog.ts`.
`createApiClient` and `createCatalogApi` accept injected fetch/client functions
for tests without introducing product fixture data into runtime code.
