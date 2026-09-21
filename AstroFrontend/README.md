# Verso frontend

Verso is an Astro 7 frontend for the media platform. The Home, Explore, Tag,
Search, and Post detail routes are server-rendered and read published posts from
the .NET Posts API.

## Requirements

- Node.js 22.12 or newer
- npm
- The .NET Posts API for real application data

The frontend does not require a global Astro installation.

## Configuration

`POSTS_API_URL` is the required server-side base URL of the Posts API. Keeping
it private prevents the frontend from exposing an internal service address.

PowerShell:

```powershell
$env:POSTS_API_URL = 'http://127.0.0.1:5188'
```

Bash:

```bash
export POSTS_API_URL=http://127.0.0.1:5188
```

This value is private to the Astro server and is not sent to the browser.

## Install and run

```text
npm ci
npm run dev
```

The development server is available at `http://localhost:4321`.

Build and preview the server-rendered output:

```text
npm run build
npm run preview -- --host 127.0.0.1
```

The production build uses the Astro Node standalone adapter. It can also be
started directly after setting `POSTS_API_URL`, `HOST`, and `PORT`:

```text
node dist/server/entry.mjs
```

## Routes

| Route | Purpose |
| --- | --- |
| `/` | Most-viewed and newest published posts, categories, and tags |
| `/explore` | Category/tag filters, sorting, and pagination |
| `/tags` | Tags returned by the Posts API, ordered by published-post activity |
| `/tags/[tag]` | Published posts for one tag |
| `/search` | Search form and keyword redirect |
| `/search/[keyword]` | Server-rendered keyword results |
| `/posts/[id]` | Published post detail and view recording |
| `/tag/[tag]` | Permanent redirect to `/tags/[tag]` |

The Posts API and upload flow are owned by their respective teams; this frontend
only consumes their published-post and facet contracts. The existing artist,
category, and legacy work pages remain statically
prerendered from `src/data/gallery.ts` while their backend modules are built.

## Rendering and browser JavaScript

Home, Explore, Tag, Search, and Post detail use ordinary links and GET forms.
They do not ship browser-side JavaScript. Filtering, sorting, searching, and
pagination are handled by Astro on the server.

The legacy category page still contains its original client-side tag filter.
Legacy Follow and Save controls are visual placeholders until server-backed
actions replace their removed local-storage behavior.

## Caching

| Page type | Browser | Shared/CDN | Stale fallback |
| --- | ---: | ---: | ---: |
| Home and popularity | 1 minute | 5 minutes | 1 hour |
| Explore, Tag, and Search | 2 minutes | 10 minutes | 1 hour |
| Post detail and errors | `no-store` | `no-store` | none |

These values keep navigation fast and improve Core Web Vitals while limiting
how long users can see stale discovery results. Post details remain `no-store`
because each server render currently records a view. Move view collection to
Cloudflare or another edge pipeline before caching detail HTML.

A CDN such as Cloudflare may respect the shared-cache directives, but it must
never cache write methods, errors, or private responses. Publishing or editing
content should eventually trigger a CDN purge; the TTL remains a safety limit
when purging fails.

## Verification

```text
npm run check
npm run build
npm run test:smoke
npm audit
```

The Playwright smoke suite starts a deterministic local Posts API fixture and
checks desktop and mobile rendering, filters, Search, Tag, cache headers, and
the absence of scripts on the public SSR pages. Real PostgreSQL behavior is
covered separately by the .NET integration suite.

## Deployment notes

- Set `POSTS_API_URL` to an address reachable from the Astro server.
- Run the standalone Node server behind the deployment platform or reverse
  proxy.
- Preserve the incoming public host/protocol so canonical URLs are correct.
- Add rate limiting or view deduplication before treating popularity as a
  production-grade ranking signal.
- Authorization and ownership checks are intentionally deferred by the team.
