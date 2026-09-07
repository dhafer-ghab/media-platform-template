import type { APIRoute } from 'astro';
import { resolveLegacyRedirect } from '@/lib/legacy';
import { legacyCategoriesPath } from '@/lib/routes';

export const prerender = false;
export const GET: APIRoute = ({ params, request }) =>
  resolveLegacyRedirect(legacyCategoriesPath(params.slug), request.signal);
