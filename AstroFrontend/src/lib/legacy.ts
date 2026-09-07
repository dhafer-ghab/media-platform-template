import { catalogApi } from '@/api/catalog';
import { ApiError } from '@/api/client';

function isSafeLocalPath(value: unknown): value is string {
  if (typeof value !== 'string' || value === '/' || value !== value.trim()) return value === '/';
  if (!value.startsWith('/') || value.startsWith('//') || /[\\%?#\u0000-\u001f\u007f]/.test(value)) return false;
  return value.slice(1).split('/').every((segment) => segment !== '' && segment !== '.' && segment !== '..');
}

export async function resolveLegacyRedirect(path: string, signal: AbortSignal): Promise<Response> {
  try {
    const route = await catalogApi.resolveLegacyRoute(path, { signal });
    const destination = route.destinationPath;
    if (!isSafeLocalPath(destination)) return new Response(null, { status: 404 });
    return new Response(null, {
      status: 301,
      headers: {
        Location: destination,
        'Cache-Control': 'public, max-age=3600, s-maxage=86400',
      },
    });
  } catch (error) {
    if (error instanceof ApiError && (error.status === 400 || error.status === 404)) {
      return new Response(null, { status: 404 });
    }
    throw error;
  }
}
