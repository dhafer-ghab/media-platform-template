import type { APIRoute } from 'astro';
import { catalogApi, type PagedResult } from '@/api/catalog';
import { actorPath, actorsPath, albumPath, absoluteUrl, explorePath, homePath, tagPath, tagsPath } from '@/lib/routes';

export const prerender = false;

const MAX_URLS = 50_000;
const PAGE_SIZE = 100;
type Entry = { loc: string; lastmod?: string };

const escapeXml = (value: string) =>
  value.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&apos;');

async function allPages<T>(load: (page: number) => Promise<PagedResult<T>>): Promise<T[]> {
  const first = await load(1);
  const items = [...first.items];
  for (let page = 2; page <= first.totalPages && items.length < MAX_URLS; page += 1) {
    items.push(...(await load(page)).items);
  }
  return items;
}

export const GET: APIRoute = async ({ site, request }) => {
  const [actors, albums, tags] = await Promise.all([
    allPages((page) => catalogApi.actors({ page, pageSize: PAGE_SIZE }, { signal: request.signal })),
    allPages((page) => catalogApi.discovery({ page, pageSize: PAGE_SIZE }, { signal: request.signal })),
    catalogApi.tags({ signal: request.signal }),
  ]);

  const entries: Entry[] = [
    { loc: absoluteUrl(homePath(), site) },
    { loc: absoluteUrl(explorePath(), site) },
    { loc: absoluteUrl(actorsPath(), site) },
    { loc: absoluteUrl(tagsPath(), site) },
    ...tags.map((tag) => ({ loc: absoluteUrl(tagPath(tag), site) })),
    ...actors.map((actor) => ({ loc: absoluteUrl(actorPath(actor.slug), site), lastmod: actor.updatedAt })),
    ...albums.map((album) => ({ loc: absoluteUrl(albumPath(album.actorSlug, album.slug), site), lastmod: album.updatedAt })),
  ];

  const body = '<?xml version="1.0" encoding="UTF-8"?>\n<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">\n' +
    entries.slice(0, MAX_URLS).map((entry) =>
      `  <url>\n    <loc>${escapeXml(entry.loc)}</loc>\n${entry.lastmod ? `    <lastmod>${entry.lastmod}</lastmod>\n` : ''}  </url>\n`,
    ).join('') + '</urlset>\n';

  return new Response(body, {
    headers: {
      'Content-Type': 'application/xml; charset=utf-8',
      'Cache-Control': 'public, max-age=0, s-maxage=3600, stale-while-revalidate=86400',
    },
  });
};
