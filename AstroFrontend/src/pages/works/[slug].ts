import type { APIRoute } from 'astro';
import { resolveLegacyRedirect } from '@/lib/legacy';
import { legacyWorkPath } from '@/lib/routes';

export const prerender = false;
export const GET: APIRoute = ({ params, request }) => {
  if (!params.slug) return new Response(null, { status: 404 });
  return resolveLegacyRedirect(legacyWorkPath(params.slug), request.signal);
};
