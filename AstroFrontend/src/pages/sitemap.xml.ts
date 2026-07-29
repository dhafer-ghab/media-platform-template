import type { APIRoute } from 'astro';
import { actors } from '@/data/media';
import { artists, artworks, categories, galleryTags } from '@/data/gallery';
import { actorPath, absoluteUrl } from '@/lib/routes';

export const prerender = false;

/** Sitemaps are capped at 50,000 URLs / 50 MB uncompressed. Past that this
 *  has to become a sitemap index pointing at shards. */
const MAX_URLS = 50_000;

const escapeXml = (s: string) =>
  s
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&apos;');

type Entry = { loc: string; lastmod?: string };

export const GET: APIRoute = ({ site }) => {
  const entries: Entry[] = [
    { loc: absoluteUrl('/', site) },
    { loc: absoluteUrl('/explore', site) },
    { loc: absoluteUrl('/artists', site) },
    ...artists.map((artist) => ({ loc: absoluteUrl(`/artists/${artist.slug}`, site) })),
    { loc: absoluteUrl('/categories', site) },
    ...categories.map((category) => ({ loc: absoluteUrl(`/categories/${category.slug}`, site) })),
    { loc: absoluteUrl('/tags', site) },
    ...galleryTags.map((tag) => ({ loc: absoluteUrl(`/tags/${tag.slug}`, site) })),
    ...artworks.map((work) => ({ loc: absoluteUrl(`/works/${work.slug}`, site) })),

    // Actor profiles. Page 1 only — paginated pages are self-canonical and
    // reachable via rel=next, and listing them here would bury the entry
    // points. Album URLs join this list when that route exists.
    ...actors.map((a) => ({
      loc: absoluteUrl(actorPath(a.slug), site),
      lastmod: a.updatedAt,
    })),
  ];

  const body =
    '<?xml version="1.0" encoding="UTF-8"?>\n' +
    '<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">\n' +
    entries
      .slice(0, MAX_URLS)
      .map(
        (e) =>
          '  <url>\n' +
          `    <loc>${escapeXml(e.loc)}</loc>\n` +
          (e.lastmod ? `    <lastmod>${e.lastmod}</lastmod>\n` : '') +
          '  </url>\n',
      )
      .join('') +
    '</urlset>\n';

  return new Response(body, {
    headers: {
      'Content-Type': 'application/xml; charset=utf-8',
      'Cache-Control':
        'public, max-age=0, s-maxage=3600, stale-while-revalidate=86400',
    },
  });
};
