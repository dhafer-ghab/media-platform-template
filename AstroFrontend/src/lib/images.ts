// The only place image delivery URLs are constructed.
//
// The data model carries storage keys; everything vendor-specific — URL shape,
// transform syntax, format negotiation — lives here. Swapping Cloudflare for
// Cloudinary, imgix or a self-hosted resizer means writing one function and
// changing one line in `buildUrl`, with no template changes anywhere.

/** Unset in dev and in mock mode, which puts us on the placeholder path. */
const IMAGE_BASE_URL = import.meta.env.PUBLIC_IMAGE_BASE_URL ?? '';

export type ImageFit = 'cover' | 'contain' | 'scale-down';

export type ImageRef = {
  /** Storage key from the API (Post.StorageKey). Empty means no asset yet. */
  key: string;
  /** Stable id, used only to pick a deterministic placeholder. */
  id: string;
};

export type ImageTransform = {
  width: number;
  height?: number;
  fit?: ImageFit;
  quality?: number;
};

/** Height that preserves an aspect ratio. Used for the height attribute so the
 *  box is reserved before the image loads. */
export function heightFor(width: number, aspectRatio: number): number {
  return Math.round(width / aspectRatio);
}

// ── Providers ────────────────────────────────────────────────────
// Each provider takes a key and a transform and returns a delivery URL.
// Only one is active; the rest of the app never knows which.

function cloudflareUrl(key: string, t: ImageTransform): string {
  const options = [
    `width=${t.width}`,
    t.height ? `height=${t.height}` : null,
    `fit=${t.fit ?? 'cover'}`,
    `quality=${t.quality ?? 80}`,
    'format=auto',
  ]
    .filter(Boolean)
    .join(',');

  return `${IMAGE_BASE_URL}/cdn-cgi/image/${options}/${key.replace(/^\/+/, '')}`;
}

/** Gradients mirror the design canvas so mock pages read as intended.
 *  They live here, not in the data model — a placeholder is a rendering
 *  decision about a missing asset. */
const PLACEHOLDER_PALETTE: readonly (readonly [string, string])[] = [
  ['#ff7a59', '#ffb347'],
  ['#f857a6', '#ff5858'],
  ['#6a5acd', '#48c6ef'],
  ['#43cea2', '#185a9d'],
  ['#f6d365', '#fda085'],
  ['#a18cd1', '#fbc2eb'],
  ['#2193b0', '#6dd5ed'],
  ['#c94b4b', '#4b134f'],
  ['#00b09b', '#96c93d'],
];

function placeholderIndex(id: string): number {
  let hash = 0;
  for (const character of id) hash = (hash * 31 + character.charCodeAt(0)) | 0;
  return Math.abs(hash) % PLACEHOLDER_PALETTE.length;
}

function placeholderUrl(id: string, width: number, height: number): string {
  const [from, to] =
    PLACEHOLDER_PALETTE[placeholderIndex(id)]!;

  const svg =
    `<svg xmlns="http://www.w3.org/2000/svg" width="${width}" height="${height}">` +
    `<defs><linearGradient id="g" x1="0" y1="0" x2="1" y2="1">` +
    `<stop offset="0" stop-color="${from}"/>` +
    `<stop offset="1" stop-color="${to}"/>` +
    `</linearGradient></defs>` +
    `<rect width="100%" height="100%" fill="url(#g)"/></svg>`;

  return `data:image/svg+xml,${encodeURIComponent(svg)}`;
}

// ── Public API ───────────────────────────────────────────────────

/** True when real assets are being served. Components use this to decide
 *  whether a srcset is worth emitting. */
export function hasRealAssets(ref: ImageRef): boolean {
  return Boolean(ref.key && IMAGE_BASE_URL);
}

export function imageUrl(ref: ImageRef, t: ImageTransform): string {
  if (!hasRealAssets(ref)) {
    return placeholderUrl(ref.id, t.width, t.height ?? t.width);
  }
  return cloudflareUrl(ref.key, t);
}

/**
 * Builds a srcset across the given widths.
 * Returns an empty string on the placeholder path — a data URI is identical at
 * every width, so a srcset would be pure markup weight with no benefit.
 * Callers should omit the attribute when this is empty.
 */
export function imageSrcSet(
  ref: ImageRef,
  widths: number[],
  t: { aspectRatio?: number; fit?: ImageFit; quality?: number } = {},
): string {
  if (!hasRealAssets(ref)) return '';

  return widths
    .map((width) => {
      const height = t.aspectRatio ? heightFor(width, t.aspectRatio) : undefined;
      return `${imageUrl(ref, { width, height, fit: t.fit, quality: t.quality })} ${width}w`;
    })
    .join(', ');
}

/** CSS gradient for decorative surfaces that have no asset of their own —
 *  the profile cover banner. ActorProfile has no cover field in the domain
 *  model, so this is a presentation fallback, not missing data. */
export function placeholderGradient(id: string): string {
  const [from, to] =
    PLACEHOLDER_PALETTE[placeholderIndex(id)]!;
  return `linear-gradient(115deg, ${from} 0%, ${to} 100%)`;
}
