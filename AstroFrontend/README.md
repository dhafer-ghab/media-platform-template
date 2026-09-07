# Verso — Astro implementation

An Astro implementation of the "Verso Gallery" design originally exported from
Claude's design canvas. A slow, editorial index of contemporary visual artists
— home / discover, artists directory, artist profile, categories, category
detail, tag detail, artwork detail, and search.

Procedurally generated CSS gradients stand in for the artwork images, and the
36-work / 9-artist dataset lives in `src/data/gallery.ts`.

## Prerequisites

Node.js 22.12+ and npm. **No global Astro install needed** — Astro
is a project dev-dependency and is invoked via npm scripts.

### Install Node.js

**Windows (PowerShell, winget):**
```powershell
winget install OpenJS.NodeJS.LTS
```

**macOS (Homebrew):**
```bash
brew install node
```

**Linux (Ubuntu/Debian, NodeSource current LTS):**
```bash
curl -fsSL https://deb.nodesource.com/setup_lts.x | sudo -E bash -
sudo apt-get install -y nodejs
```

Verify (any platform):
```
node --version
npm --version
```

## Get the project

```
git clone <repo-url> verso
cd verso
```

Or copy the `verso/` folder. Do **not** copy `node_modules/` — it's
machine-specific and excluded by `.gitignore`.

## Install dependencies

```
npm install
```

Reads `package.json` and installs everything into `./node_modules`.

## Run

```
npm run dev       # dev server at http://localhost:4321
npm run build     # produce the Node server and prerendered pages in ./dist
npm run preview   # serve the built ./dist locally
```

That's the full loop.

## Project layout

```
verso/
├─ astro.config.mjs
├─ package.json
├─ tsconfig.json            # @/* → src/*
└─ src/
   ├─ data/gallery.ts       # artists, artworks, categories, helpers
   ├─ layouts/Layout.astro  # global tokens, fonts, follow/save script
   ├─ components/
   │  ├─ Header.astro       # sticky nav + search form
   │  ├─ Footer.astro
   │  ├─ WorkCard.astro     # variants: default | masonry | compact | spotlight | pin
   │  ├─ ArtistCard.astro
   │  └─ CategoryCard.astro
   └─ pages/
      ├─ index.astro
      ├─ artists/{index,[slug]}.astro
      ├─ categories/{index,[slug]}.astro
      ├─ tags/[tag].astro
      ├─ works/[slug].astro
      └─ search.astro
```

## Notes on routing

- Gallery, discovery, authentication and directory pages are statically prerendered.
  Actor, album and post pages are rendered by the Node adapter.
- Prerendered pages do not receive query strings at request time, so the **search** page
  and the **category tag filter** do their filtering client-side: the page
  ships a JSON index (search) or `data-tags` attributes (category) and a small
  inline script reads `window.location.search` to filter the DOM. No server
  adapter required.
- Follow / Save buttons persist in `localStorage` under `verso:following` and
  `verso:saved`. Logic lives in the inline script at the bottom of
  `src/layouts/Layout.astro` and is opted into via `data-follow-toggle` /
  `data-save-toggle` attributes.

## Editor (optional)

```
code --install-extension astro-build.astro-vscode
```

## Recommended npm scripts cheat-sheet

| Command            | What it does                                  |
| ------------------ | --------------------------------------------- |
| `npm run dev`      | Local dev server, HMR, port 4321              |
| `npm run build`    | Static build to `./dist`                      |
| `npm run preview`  | Serves `./dist` locally to smoke-test a build |
| `npx astro check`  | Type-check `.astro` files                     |
