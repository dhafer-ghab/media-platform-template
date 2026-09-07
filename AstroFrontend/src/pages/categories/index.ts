import type { APIRoute } from 'astro';
import { resolveLegacyRedirect } from '@/lib/legacy';
import { legacyCategoriesPath } from '@/lib/routes';

export const prerender = false;
export const GET: APIRoute = ({ request }) => resolveLegacyRedirect(legacyCategoriesPath(), request.signal);
