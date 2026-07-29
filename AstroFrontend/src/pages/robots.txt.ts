import type { APIRoute } from 'astro';
import { absoluteUrl } from '@/lib/routes';

// Served dynamically so the Sitemap line is built from the configured origin
// rather than a hardcoded domain that would be wrong in every environment
// except the one it was written for.
export const prerender = false;

export const GET: APIRoute = ({ site }) => {
  const body = [
    'User-agent: *',
    'Allow: /',
    '',
    `Sitemap: ${absoluteUrl('/sitemap.xml', site)}`,
    '',
  ].join('\n');

  return new Response(body, {
    headers: {
      'Content-Type': 'text/plain; charset=utf-8',
      'Cache-Control':
        'public, max-age=0, s-maxage=3600, stale-while-revalidate=86400',
    },
  });
};
