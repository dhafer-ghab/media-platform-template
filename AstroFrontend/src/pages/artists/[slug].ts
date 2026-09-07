import type { APIRoute } from 'astro';
import { resolveLegacyRedirect } from '@/lib/legacy';
import { legacyArtistsPath } from '@/lib/routes';

export const prerender = false;
export const GET: APIRoute = ({ params, request }) =>
  resolveLegacyRedirect(legacyArtistsPath(params.slug), request.signal);
