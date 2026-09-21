import type { PostSort } from './posts';

export const HOME_CACHE_CONTROL =
  'public, max-age=60, s-maxage=300, stale-while-revalidate=3600';
export const COLLECTION_CACHE_CONTROL =
  'public, max-age=120, s-maxage=600, stale-while-revalidate=3600';

export function setHomeCache(headers: Headers): void {
  headers.set('Cache-Control', HOME_CACHE_CONTROL);
}

export function setCollectionCache(headers: Headers): void {
  headers.set('Cache-Control', COLLECTION_CACHE_CONTROL);
}

export function setNoStore(headers: Headers): void {
  headers.set('Cache-Control', 'no-store');
}

export function parsePage(value: string | null): number {
  const parsed = Number.parseInt(value ?? '1', 10);
  return Number.isSafeInteger(parsed) && parsed > 0 ? Math.min(parsed, 1_000_000) : 1;
}

export function parseSort(value: string | null): PostSort {
  return value === 'oldest' || value === 'title' || value === 'popular'
    ? value
    : 'newest';
}

export function cleanFilter(value: string | null, maxLength: number): string {
  return (value ?? '').trim().slice(0, maxLength);
}
