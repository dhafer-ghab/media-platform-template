// The only place video delivery URLs are constructed. Mirrors lib/images.ts:
// the model carries a storage key, the vendor-specific URL shape lives here.
// Video hosting is a separate decision from image transforms, which is why
// this is its own module rather than an addition to images.ts.

import { imageUrl } from './images';

const VIDEO_BASE_URL = import.meta.env.PUBLIC_VIDEO_BASE_URL ?? '';
const VIDEO_ORIGIN = VIDEO_BASE_URL.replace(/\/$/, '');

export type VideoRef = { key: string; id: string };

const NATIVE_VIDEO_TYPES = new Set(['video/mp4', 'video/webm', 'video/ogg']);

/**
 * Playable source, or null when no asset exists yet.
 * Callers must render a poster-and-link fallback rather than an empty
 * <video>, which would show controls that do nothing.
 */
export function videoUrl(ref: VideoRef): string | null {
  if (!ref.key || !VIDEO_BASE_URL) return null;
  const key = ref.key.replace(/^\/+/, '');
  return `${VIDEO_ORIGIN}/${key}`;
}

export function isNativeVideo(mimeType: string): boolean {
  return NATIVE_VIDEO_TYPES.has(mimeType.toLowerCase().split(';', 1)[0]!.trim());
}

/**
 * Poster frame, derived from the same storage key.
 * Post has no poster field in the domain model and inventing one would add
 * something the backend never returns.
 */
export function posterUrl(
  ref: VideoRef,
  t: { width: number; height: number },
): string {
  if (!ref.key || !VIDEO_BASE_URL) {
    // No asset — fall through to the image service's placeholder so the box
    // is still filled and sized.
    return imageUrl({ key: '', id: ref.id }, t);
  }
  const key = ref.key.replace(/^\/+/, '');
  return `${VIDEO_ORIGIN}/${key}/thumbnails/thumbnail.jpg?width=${t.width}`;
}
