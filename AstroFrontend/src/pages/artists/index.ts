import type { APIRoute } from 'astro';
import { resolveLegacyRedirect } from '@/lib/legacy';
import { legacyArtistsPath } from '@/lib/routes';

export const prerender = false;
export const GET: APIRoute = ({ request }) => resolveLegacyRedirect(legacyArtistsPath(), request.signal);
